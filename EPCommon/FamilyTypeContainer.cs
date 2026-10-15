using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace EPCommon;

public class FamilyTypeContainer
{
	private FamilyTypeWrapper m_FamilyTypeWrapper;

	private List<FamilyParameterWrapper> m_FamilyParameterList;

	private FamilyParameterWrapperStatus m_Status;

	public FamilyTypeWrapper FamilyTypeWrapper
	{
		get
		{
			return m_FamilyTypeWrapper;
		}
		set
		{
			m_FamilyTypeWrapper = value;
		}
	}

	public List<FamilyParameterWrapper> FamilyParameterList
	{
		get
		{
			return m_FamilyParameterList;
		}
		set
		{
			m_FamilyParameterList = value;
		}
	}

	public FamilyParameterWrapperStatus Status
	{
		get
		{
			return m_Status;
		}
		set
		{
			m_Status = value;
		}
	}

	public FamilyTypeContainer(FamilyTypeWrapper _FamilyTypeWrapper, List<FamilyParameterWrapper> _FamilyParameterList)
	{
		m_Status = FamilyParameterWrapperStatus.Included;
		m_FamilyTypeWrapper = _FamilyTypeWrapper;
		m_FamilyParameterList = _FamilyParameterList;
		if (m_FamilyTypeWrapper == null)
		{
			return;
		}
		FamilyParameterWrapper familyParameterWrapper = null;
		double _dValue = 0.0;
		int _nValue = 0;
		string _strValue = string.Empty;
		ElementId _Value = null;
		for (int i = 0; i < m_FamilyParameterList.Count; i++)
		{
			familyParameterWrapper = m_FamilyParameterList[i];
			if (familyParameterWrapper == null || familyParameterWrapper.FamilyParameter == null || (familyParameterWrapper.FamilyParameter.Formula != null && !(familyParameterWrapper.FamilyParameter.Formula == string.Empty)) || !HasValue(i))
			{
				continue;
			}
			switch (familyParameterWrapper.StorageType)
			{
			case StorageType.Double:
				if (GetDoubleValue(i, ref _dValue))
				{
					familyParameterWrapper.DoubleValue = _dValue;
				}
				break;
			case StorageType.Integer:
				if (GetIntegerValue(i, ref _nValue))
				{
					familyParameterWrapper.IntegerValue = _nValue;
				}
				break;
			case StorageType.String:
				if (GetStringValue(i, ref _strValue))
				{
					familyParameterWrapper.Value = _strValue;
				}
				break;
			case StorageType.ElementId:
				if (GetElementIdValue(i, ref _Value))
				{
					familyParameterWrapper.ElementIdValue = _Value;
				}
				break;
			}
		}
	}

	public FamilyTypeContainer()
	{
		m_Status = FamilyParameterWrapperStatus.Included;
	}

	public bool HasValue(int _nParameterIndex)
	{
		if (_nParameterIndex >= 0 && _nParameterIndex < m_FamilyParameterList.Count)
		{
			if (!m_FamilyParameterList[_nParameterIndex].InternalValueSet && m_FamilyTypeWrapper.FamilyType != null && m_FamilyParameterList[_nParameterIndex].FamilyParameter != null)
			{
				return m_FamilyTypeWrapper.FamilyType.HasValue(m_FamilyParameterList[_nParameterIndex].FamilyParameter);
			}
			if (m_FamilyParameterList[_nParameterIndex].Value != null && m_FamilyParameterList[_nParameterIndex].Value != string.Empty)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetDoubleValue(int _nParameterIndex, ref double _dValue)
	{
		if (_nParameterIndex >= 0 && _nParameterIndex < m_FamilyParameterList.Count)
		{
			if (!m_FamilyParameterList[_nParameterIndex].InternalValueSet && m_FamilyTypeWrapper.FamilyType != null && m_FamilyParameterList[_nParameterIndex].FamilyParameter != null)
			{
				if (m_FamilyTypeWrapper.FamilyType.AsDouble(m_FamilyParameterList[_nParameterIndex].FamilyParameter) is double)
				{
					_dValue = m_FamilyTypeWrapper.FamilyType.AsDouble(m_FamilyParameterList[_nParameterIndex].FamilyParameter).Value;
					m_FamilyParameterList[_nParameterIndex].Value = ValueConversion.ToString(_dValue);
					return true;
				}
			}
			else if (m_FamilyParameterList[_nParameterIndex].Value != null && m_FamilyParameterList[_nParameterIndex].Value != string.Empty)
			{
				_dValue = m_FamilyParameterList[_nParameterIndex].DoubleValue;
				return true;
			}
		}
		return false;
	}

	public bool GetIntegerValue(int _nParameterIndex, ref int _nValue)
	{
		if (_nParameterIndex >= 0 && _nParameterIndex < m_FamilyParameterList.Count)
		{
			if (!m_FamilyParameterList[_nParameterIndex].InternalValueSet && m_FamilyTypeWrapper.FamilyType != null && m_FamilyParameterList[_nParameterIndex].FamilyParameter != null)
			{
				if (m_FamilyTypeWrapper.FamilyType.AsInteger(m_FamilyParameterList[_nParameterIndex].FamilyParameter) is int)
				{
					_nValue = m_FamilyTypeWrapper.FamilyType.AsInteger(m_FamilyParameterList[_nParameterIndex].FamilyParameter).Value;
					m_FamilyParameterList[_nParameterIndex].Value = Convert.ToString(_nValue);
					return true;
				}
			}
			else if (m_FamilyParameterList[_nParameterIndex].Value != null && m_FamilyParameterList[_nParameterIndex].Value != string.Empty)
			{
				if (string.Compare(m_FamilyParameterList[_nParameterIndex].Value, EPShared.LocalRM.GetString("YesString"), ignoreCase: true) == 0)
				{
					_nValue = 1;
				}
				else if (string.Compare(m_FamilyParameterList[_nParameterIndex].Value, EPShared.LocalRM.GetString("NoString"), ignoreCase: true) == 0)
				{
					_nValue = 0;
				}
				else
				{
					_nValue = m_FamilyParameterList[_nParameterIndex].IntegerValue;
				}
				return true;
			}
		}
		return false;
	}

	public bool GetStringValue(int _nParameterIndex, ref string _strValue)
	{
		if (_nParameterIndex >= 0 && _nParameterIndex < m_FamilyParameterList.Count)
		{
			if (!m_FamilyParameterList[_nParameterIndex].InternalValueSet && m_FamilyTypeWrapper.FamilyType != null && m_FamilyParameterList[_nParameterIndex].FamilyParameter != null)
			{
				if (m_FamilyTypeWrapper.FamilyType.AsString(m_FamilyParameterList[_nParameterIndex].FamilyParameter) != null)
				{
					_strValue = m_FamilyTypeWrapper.FamilyType.AsString(m_FamilyParameterList[_nParameterIndex].FamilyParameter);
					m_FamilyParameterList[_nParameterIndex].Value = _strValue;
					return true;
				}
			}
			else if (m_FamilyParameterList[_nParameterIndex].Value != null)
			{
				_strValue = m_FamilyParameterList[_nParameterIndex].Value;
				return true;
			}
		}
		return false;
	}

	public bool GetElementIdValue(int _nParameterIndex, ref ElementId _Value)
	{
		if (_nParameterIndex >= 0 && _nParameterIndex < m_FamilyParameterList.Count)
		{
			if (!m_FamilyParameterList[_nParameterIndex].InternalValueSet && m_FamilyTypeWrapper.FamilyType != null && m_FamilyParameterList[_nParameterIndex].FamilyParameter != null)
			{
				_Value = m_FamilyTypeWrapper.FamilyType.AsElementId(m_FamilyParameterList[_nParameterIndex].FamilyParameter);
				m_FamilyParameterList[_nParameterIndex].Value = Convert.ToString(_Value.Value64());
				return true;
			}
			if (m_FamilyParameterList[_nParameterIndex].Value != null && m_FamilyParameterList[_nParameterIndex].Value != string.Empty)
			{
				_Value = m_FamilyParameterList[_nParameterIndex].ElementIdValue;
				return true;
			}
		}
		return false;
	}
}
