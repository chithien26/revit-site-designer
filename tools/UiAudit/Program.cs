using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Read-only audit. Use the actual evaluated build inputs, including generated WPF code.
var root = Path.GetFullPath(args[0]);
using var project = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "artifacts", "ui-cleanup-project-items.json")));
var items = project.RootElement.GetProperty("Items");
var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols:
    ["NET", "NET8_0", "NET8_0_OR_GREATER", "NETCOREAPP", "WINDOWS", "REVIT2025"]);
var trees = items.GetProperty("Compile").EnumerateArray()
    .Select(x => x.GetProperty("FullPath").GetString()).Distinct(StringComparer.OrdinalIgnoreCase)
    .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parseOptions, path)).ToArray();
var references = items.GetProperty("ReferencePath").EnumerateArray()
    .Select(x => x.GetProperty("FullPath").GetString()).Distinct(StringComparer.OrdinalIgnoreCase)
    .Select(path => MetadataReference.CreateFromFile(path)).ToArray();
var compilation = CSharpCompilation.Create("UiAudit", trees, references,
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
var errors = compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
if (errors.Length != 0)
{
    foreach (var error in errors) Console.Error.WriteLine(error);
    return 1;
}
var types = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
var declarations = new List<(INamedTypeSymbol Type, string File)>();
foreach (var tree in trees)
{
    var model = compilation.GetSemanticModel(tree);
    foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
    {
        var symbol = model.GetDeclaredSymbol(declaration);
        if (symbol == null) continue;
        types[symbol.OriginalDefinition] = tree.FilePath;
        declarations.Add((symbol, tree.FilePath));
    }
    foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>())
    {
        var symbol = model.GetDeclaredSymbol(declaration);
        if (symbol != null) types[symbol.OriginalDefinition] = tree.FilePath;
    }
}
var edges = trees.ToDictionary(x => x.FilePath, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
var referencedMethods = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
foreach (var tree in trees)
{
    var model = compilation.GetSemanticModel(tree);
    foreach (var node in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
    {
        var symbol = model.GetSymbolInfo(node).Symbol;
        if (symbol is IMethodSymbol method) referencedMethods.Add(method.OriginalDefinition);
        if (symbol is IAliasSymbol alias) symbol = alias.Target;
        var type = symbol as INamedTypeSymbol ?? symbol?.ContainingType;
        if (type != null && types.TryGetValue(type.OriginalDefinition, out var target) && target != tree.FilePath)
            edges[tree.FilePath].Add(target);
    }
}
bool IsUi(INamedTypeSymbol type)
{
    for (var current = type; current != null; current = current.BaseType)
        if (current.ToDisplayString() == "System.Windows.Forms.Control") return true;
    return false;
}
bool IsEntry(INamedTypeSymbol type) => type.AllInterfaces.Any(x =>
    x.ToDisplayString() is "Autodesk.Revit.UI.IExternalApplication" or "Autodesk.Revit.UI.IExternalCommand" or "Autodesk.Revit.DB.IExternalDBApplication");
var unusedLaunchers = new List<object>();
foreach (var tree in trees)
{
    var model = compilation.GetSemanticModel(tree);
    foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
    {
        var symbol = model.GetDeclaredSymbol(method);
        if (symbol == null || referencedMethods.Contains(symbol.OriginalDefinition) || symbol.IsOverride || IsEntry(symbol.ContainingType)) continue;
        var dialogs = method.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
            .Select(x => model.GetTypeInfo(x).Type as INamedTypeSymbol).Where(x => x != null && IsUi(x))
            .Select(x => x.ToDisplayString()).Distinct().ToArray();
        if (dialogs.Length > 0)
            unusedLaunchers.Add(new { method = symbol.ToDisplayString(), file = Path.GetRelativePath(root, tree.FilePath),
                start = method.FullSpan.Start, length = method.FullSpan.Length, dialogs });
    }
}
var roots = new HashSet<string>(declarations.Where(x => IsEntry(x.Type)).Select(x => x.File), StringComparer.OrdinalIgnoreCase);
var testNames = new HashSet<string>(StringComparer.Ordinal);
foreach (var folder in new[] { "tools/SmokeTests", "tools/RevitSmoke" })
    foreach (var path in Directory.EnumerateFiles(Path.Combine(root, folder), "*.cs"))
        foreach (var name in CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
            testNames.Add(name.Identifier.ValueText);
foreach (var declaration in declarations)
    if (testNames.Contains(declaration.Type.Name)) roots.Add(declaration.File);
var reachable = new HashSet<string>(roots, StringComparer.OrdinalIgnoreCase);
var queue = new Queue<string>(roots);
while (queue.TryDequeue(out var file))
    foreach (var next in edges[file])
        if (reachable.Add(next)) queue.Enqueue(next);
var unusedUi = declarations.Where(x => IsUi(x.Type) && !reachable.Contains(x.File)).ToArray();
var remove = new HashSet<string>(unusedUi.Select(x => x.File), StringComparer.OrdinalIgnoreCase);
// Include only unreachable workflow callers that would otherwise reference a removed dialog.
bool changed;
do
{
    changed = false;
    foreach (var pair in edges)
        if (!reachable.Contains(pair.Key) && !remove.Contains(pair.Key) && pair.Value.Any(remove.Contains))
        { remove.Add(pair.Key); changed = true; }
} while (changed);
var report = new
{
    date = DateTimeOffset.Now,
    method = "Roslyn semantic type references across evaluated production build inputs; all external entry points and types referenced by smoke harnesses retained as roots. Whole classes are retained, including every member and inherited UI dependency.",
    roots = roots.Order().Select(x => Path.GetRelativePath(root, x)),
    unused_ui_launchers = unusedLaunchers,
    unused_ui = unusedUi.Select(x => new { name = x.Type.ToDisplayString(), file = Path.GetRelativePath(root, x.File) }).OrderBy(x => x.file),
    removal_files = remove.Order().Select(x => Path.GetRelativePath(root, x)),
    retained_ui = declarations.Where(x => IsUi(x.Type) && reachable.Contains(x.File)).Select(x => x.Type.ToDisplayString()).Distinct().Order(),
    outside_references = edges.Where(x => !remove.Contains(x.Key) && x.Value.Any(remove.Contains))
        .Select(x => new { file = Path.GetRelativePath(root, x.Key), targets = x.Value.Where(remove.Contains).Select(y => Path.GetRelativePath(root, y)) })
};
File.WriteAllText(Path.Combine(root, "artifacts", "unused-ui-analysis.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Unused UI types: {unusedUi.Length}; source files in closed removal group: {remove.Count}");
foreach (var file in remove.Order()) Console.WriteLine(Path.GetRelativePath(root, file));
return 0;
