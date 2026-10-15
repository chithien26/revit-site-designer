using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyLoadOptions : IFamilyLoadOptions
{
	private bool m_bOverwriteParameterValues;

	public FamilyLoadOptions(bool _bOverwriteParameterValues)
	{
		m_bOverwriteParameterValues = _bOverwriteParameterValues;
	}

	public bool OnFamilyFound(bool _bFamilyInUse, out bool _bOverwriteParameterValues)
	{
		_bOverwriteParameterValues = false;
		if (_bFamilyInUse)
		{
			_bOverwriteParameterValues = m_bOverwriteParameterValues;
		}
		return true;
	}

	public bool OnSharedFamilyFound(Family _SharedFamily, bool _bFamilyInUse, out FamilySource _Source, out bool _bOverwriteParameterValues)
	{
		_bOverwriteParameterValues = false;
		_Source = FamilySource.Family;
		if (_bFamilyInUse)
		{
			_bOverwriteParameterValues = m_bOverwriteParameterValues;
		}
		return true;
	}
}
