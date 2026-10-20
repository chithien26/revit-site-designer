using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;

namespace EPCommon;

// Toposolid host + proposed copies occupy the same XY, which Revit warns about.
// Site Designer still needs both surfaces; swallow that expected warning so
// Set Base / Soft Terrain finish like the 2020 TopographySurface workflow.
public static class TerrainFailures
{
	private static readonly FailureDefinitionId[] OverlapIds = LoadOverlapIds();

	private static FailureDefinitionId[] LoadOverlapIds()
	{
		var ids = new List<FailureDefinitionId>();
		var type = typeof(BuiltInFailures.OverlapFailures);
		foreach (var name in new[]
		{
			"DuplicateInstances",
			"ToposolidOverlap",
			"ToposolidSubregionOverlap",
			"ToposolidFloorOverlap",
			"SubregionOverlap"
		})
		{
			var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
			if (property?.GetValue(null) is FailureDefinitionId id)
			{
				ids.Add(id);
			}
		}
		return ids.ToArray();
	}

	public static List<FailureDefinitionId> ExpectedWarningIds()
	{
		return OverlapIds.ToList();
	}

	public static void Log(FailuresAccessor accessor)
	{
		if (accessor == null)
		{
			return;
		}
		foreach (FailureMessageAccessor message in accessor.GetFailureMessages())
		{
			RuntimeDiagnostics.Write("Revit " + message.GetSeverity() + ": " + (message.GetDescriptionText() ?? message.GetFailureDefinitionId()?.Guid.ToString()));
		}
	}

	public static void DeleteExpectedWarnings(FailuresAccessor accessor)
	{
		if (accessor == null)
		{
			return;
		}
		Log(accessor);
		foreach (FailureMessageAccessor message in accessor.GetFailureMessages().ToList())
		{
			if (message.GetSeverity() != FailureSeverity.Warning || !IsExpected(message))
			{
				continue;
			}
			try
			{
				accessor.DeleteWarning(message);
			}
			catch (Autodesk.Revit.Exceptions.ArgumentException)
			{
			}
		}
	}

	private static bool IsExpected(FailureMessageAccessor message)
	{
		FailureDefinitionId id = message.GetFailureDefinitionId();
		if (OverlapIds.Any(known => known == id))
		{
			return true;
		}
		string text = message.GetDescriptionText() ?? string.Empty;
		if (text.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0
			&& text.IndexOf("mark", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return true;
		}
		if (text.IndexOf("overlap", StringComparison.OrdinalIgnoreCase) < 0)
		{
			return false;
		}
		return text.IndexOf("toposolid", StringComparison.OrdinalIgnoreCase) >= 0
			|| text.IndexOf("toposurface", StringComparison.OrdinalIgnoreCase) >= 0
			|| text.IndexOf("sub-division", StringComparison.OrdinalIgnoreCase) >= 0
			|| text.IndexOf("subregion", StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
