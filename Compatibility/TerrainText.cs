using System.Text.RegularExpressions;

namespace EPCommon;

// Change presentation text only. Persisted settings and family parameter names
// keep their original keys so existing Site Designer content remains compatible.
public static class TerrainText
{
    public static string ForDisplay(string value)
    {
#if REVIT2022 || REVIT2023
        return value;
#else
        if (string.IsNullOrEmpty(value)) return value;
        return Regex.Replace(value, @"toposurface|topography\s*surface", match =>
            match.Value == match.Value.ToUpperInvariant() ? "TOPOSOLID" :
            char.IsUpper(match.Value[0]) ? "Toposolid" : "toposolid", RegexOptions.IgnoreCase);
#endif
    }
}
