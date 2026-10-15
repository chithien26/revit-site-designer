using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public static class ElementSetConverter
{
	public static List<ElementId> ToIDList(ElementSet elm_set)
	{
		List<ElementId> list = new List<ElementId>();
		foreach (Element item in elm_set)
		{
			list.Add(item.Id);
		}
		return list;
	}

	public static List<GeometryObject> ToGeometryObjectList(IEnumerator<GeometryObject> e)
	{
		List<GeometryObject> list = new List<GeometryObject>();
		e.Reset();
		while (e.MoveNext())
		{
			list.Add(e.Current);
		}
		return list;
	}
}
