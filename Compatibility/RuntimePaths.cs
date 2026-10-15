using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EPCommon;

public static class RuntimePaths
{
    private static IntPtr nativeModule;
    private static string assemblyDirectory;
    public static string AssemblyDirectory
    {
        get
        {
            if (assemblyDirectory != null) return assemblyDirectory;
            assemblyDirectory = CandidateDirectories().FirstOrDefault();
            if (assemblyDirectory == null)
                throw new DirectoryNotFoundException("Cannot locate the Site Designer installation folder. "
                    + "For an in-memory command loader, rebuild/load SiteDesigner.dll from its build output, "
                    + "install it with Install.ps1, or set SITE_DESIGNER_HOME to its folder.");
            RuntimeDiagnostics.Write("Runtime folder: " + assemblyDirectory + "; memory-loaded=" + string.IsNullOrEmpty(typeof(RuntimePaths).Assembly.Location));
            return assemblyDirectory;
        }
    }
    public static string ManagedAssemblyPath
    {
        get
        {
            string location = typeof(RuntimePaths).Assembly.Location;
            if (!string.IsNullOrEmpty(location)) return location;
            foreach (var directory in CandidateDirectories())
            {
                var path = Path.Combine(directory, "SiteDesigner.dll");
                if (File.Exists(path)) return path;
            }
            throw new FileNotFoundException("Cannot locate SiteDesigner.dll on disk. Set SITE_DESIGNER_HOME to its folder before loading the application.");
        }
    }
    private static IEnumerable<string> CandidateDirectories()
    {
        var assembly = typeof(RuntimePaths).Assembly;
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(assembly.Location)) candidates.Add(Path.GetDirectoryName(assembly.Location));
        candidates.Add(Environment.GetEnvironmentVariable("SITE_DESIGNER_HOME"));
        candidates.Add(assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "SiteDesigner.BuildDirectory")?.Value);
        string version = (assembly.GetReferencedAssemblies().First(x => x.Name == "RevitAPI").Version.Major + 2000).ToString();
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "Revit", "Addins", version, "SiteDesigner"));
        candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "Revit", "Addins", version, "SiteDesigner"));
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate) || !Path.IsPathRooted(candidate)) continue;
            var path = Path.GetFullPath(candidate);
            if (Directory.Exists(path) && seen.Add(path)) yield return path;
        }
    }
    public static string FindDataRoot(string version)
    {
        var candidates = CandidateDirectories().Concat(new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "Site Designer for Revit " + version),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "Site Designer for Revit 2020")
        });
        foreach (var path in candidates)
            if (File.Exists(Path.Combine(path, "Data", "Support", "SITEDESIGNER_enu.xml"))) return path;
        throw new FileNotFoundException("Site Designer content is missing. Copy the licensed 2020 Data folder beside SiteDesigner.dll or supply -ContentRoot to Install.ps1.");
    }
    public static string SettingsFile(string version, string name, string dataRoot)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SiteDesigner", version);
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, name);
        if (!File.Exists(path))
        {
            // Seed supported defaults; machine-specific 2020 family-template paths are not portable.
            var defaults = Path.Combine(AssemblyDirectory, "Defaults", name);
            if (File.Exists(defaults)) File.Copy(defaults, path);
            else File.WriteAllText(path, "[Setting]" + Environment.NewLine);
        }
        return path;
    }
    public static void LoadNativeLibrary()
    {
        if (nativeModule != IntPtr.Zero) return;
        var candidates = CandidateDirectories().SelectMany(directory => new[]
        {
            Path.Combine(directory, "SitesDLLWrapper.dll"),
            Path.Combine(directory, "native", "SitesDLLWrapper.dll")
        }).Concat(new[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Autodesk", "Revit Site Designer Extension 2020", "SitesDLLWrapper.dll")
        });
        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;
            nativeModule = LoadLibraryEx(path, IntPtr.Zero, 0x00000100 | 0x00001000);
            if (nativeModule == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Cannot load " + path);
            RuntimeDiagnostics.Write("Native geometry library: " + path);
            return;
        }
        throw new DllNotFoundException("SitesDLLWrapper.dll is required. Supply the licensed x64 library beside SiteDesigner.dll.");
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);
}
