using System;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyParameterWrapper
{
	private string m_displayName;

	private string m_strParameterName;

	private string m_strParameterValue;

	private double m_dParameterValue;

	private int m_nParameterValue;

	private ElementId m_IdParameterValue;

	private StorageType m_StorageType;

	private FamilyParameter m_FamilyParameter;

	private bool m_bInternalValuesSet;

	public string DisplayName
	{
		get
		{
			return m_displayName;
		}
		set
		{
			m_displayName = value;
		}
	}

	public string ParameterName
	{
		get
		{
			return m_strParameterName;
		}
		set
		{
			m_strParameterName = value;
		}
	}

	public string Value
	{
		get
		{
			return m_strParameterValue;
		}
		set
		{
			m_strParameterValue = value;
			m_bInternalValuesSet = true;
			if (m_strParameterValue == null || !(m_strParameterValue != string.Empty))
			{
				return;
			}
			switch (m_StorageType)
			{
			case StorageType.Double:
			{
				double _dReturnedDouble = 0.0;
				if (RevitGeometry.StrE_GetValidRealNumber(m_strParameterValue, ref _dReturnedDouble))
				{
					m_dParameterValue = _dReturnedDouble;
				}
				break;
			}
			case StorageType.Integer:
			{
				int _nReturnedInt2 = 0;
				if (string.Compare(m_strParameterValue, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
				{
					m_nParameterValue = 1;
				}
				else if (string.Compare(m_strParameterValue, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
				{
					m_nParameterValue = 0;
				}
				else if (RevitGeometry.StrE_GetValidWholeNumber(m_strParameterValue, ref _nReturnedInt2))
				{
					m_nParameterValue = _nReturnedInt2;
				}
				break;
			}
			case StorageType.ElementId:
			{
				int _nReturnedInt = 0;
				if (RevitGeometry.StrE_GetValidWholeNumber(m_strParameterValue, ref _nReturnedInt))
				{
					m_IdParameterValue = RevitIds.Create(_nReturnedInt);
				}
				break;
			}
			case StorageType.String:
				break;
			}
		}
	}

	public double DoubleValue
	{
		get
		{
			return m_dParameterValue;
		}
		set
		{
			m_dParameterValue = value;
			m_strParameterValue = ValueConversion.ToString(m_dParameterValue);
			m_bInternalValuesSet = true;
		}
	}

	public int IntegerValue
	{
		get
		{
			return m_nParameterValue;
		}
		set
		{
			m_nParameterValue = value;
			m_strParameterValue = Convert.ToString(m_nParameterValue);
			m_bInternalValuesSet = true;
		}
	}

	public ElementId ElementIdValue
	{
		get
		{
			return m_IdParameterValue;
		}
		set
		{
			m_IdParameterValue = value;
			m_strParameterValue = Convert.ToString(m_IdParameterValue.Value64());
			m_bInternalValuesSet = true;
		}
	}

	public StorageType StorageType
	{
		get
		{
			return m_StorageType;
		}
		set
		{
			m_StorageType = value;
		}
	}

	public FamilyParameter FamilyParameter
	{
		get
		{
			return m_FamilyParameter;
		}
		set
		{
			m_FamilyParameter = value;
			if (m_FamilyParameter != null && m_FamilyParameter.Definition != null)
			{
				m_strParameterName = m_FamilyParameter.Definition.Name;
				m_StorageType = m_FamilyParameter.StorageType;
			}
		}
	}

	public bool InternalValueSet => m_bInternalValuesSet;

	public FamilyParameterWrapper(string _strParameterDisplayName, string _strParameterName, string _strParameterValue, StorageType _StorageType)
	{
		m_displayName = _strParameterDisplayName;
		m_strParameterName = _strParameterName;
		m_strParameterValue = _strParameterValue;
		m_StorageType = _StorageType;
		m_bInternalValuesSet = true;
		if (m_strParameterValue == null || !(m_strParameterValue != string.Empty))
		{
			return;
		}
		switch (m_StorageType)
		{
		case StorageType.Double:
		{
			double _dReturnedDouble = 0.0;
			if (RevitGeometry.StrE_GetValidRealNumber(m_strParameterValue, ref _dReturnedDouble))
			{
				m_dParameterValue = _dReturnedDouble;
			}
			break;
		}
		case StorageType.Integer:
		{
			int _nReturnedInt2 = 0;
			if (string.Compare(_strParameterValue, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
			{
				m_nParameterValue = 1;
			}
			else if (string.Compare(_strParameterValue, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
			{
				m_nParameterValue = 0;
			}
			else if (RevitGeometry.StrE_GetValidWholeNumber(m_strParameterValue, ref _nReturnedInt2))
			{
				m_nParameterValue = _nReturnedInt2;
			}
			break;
		}
		case StorageType.ElementId:
		{
			int _nReturnedInt = 0;
			if (RevitGeometry.StrE_GetValidWholeNumber(m_strParameterValue, ref _nReturnedInt))
			{
				m_IdParameterValue = RevitIds.Create(_nReturnedInt);
			}
			break;
		}
		case StorageType.String:
			break;
		}
	}

	public FamilyParameterWrapper(string _strParameterDisplayName, FamilyParameter _FamilyParameter, string _strParameterValue)
	{
		m_displayName = _strParameterDisplayName;
		m_strParameterValue = _strParameterValue;
		m_FamilyParameter = _FamilyParameter;
		if (_FamilyParameter != null && _FamilyParameter.Definition != null)
		{
			m_strParameterName = _FamilyParameter.Definition.Name;
			m_StorageType = _FamilyParameter.StorageType;
		}
		m_bInternalValuesSet = false;
	}
}
