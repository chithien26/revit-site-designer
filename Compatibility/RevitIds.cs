using Autodesk.Revit.DB;
namespace EPCommon;
public static class RevitIds
{
    public static ElementId Create(long value)
    {
#if REVIT2022 || REVIT2023
        return new ElementId(checked((int)value));
#else
        return new ElementId(value);
#endif
    }
    public static ElementId Create(BuiltInCategory value) => new ElementId(value);
    public static ElementId Create(BuiltInParameter value) => new ElementId(value);
    public static long Value64(this ElementId id)
    {
#if REVIT2022 || REVIT2023
        return id.IntegerValue;
#else
        return id.Value;
#endif
    }
}
