using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

internal class ImportExport
{
	public static bool ImportFileWithSeparator(Document _Document, string _strNameFileLocation, string _strSeparator, ref List<FamilyTypeContainer> FamilyImportContainerList)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		Stream stream = new FileStream(_strNameFileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		StreamReader streamReader = new StreamReader(stream, Encoding.ASCII);
		string text;
		if ((text = streamReader.ReadLine()) != null)
		{
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = 10000;
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarImportMessage");
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			List<KeyValuePair<string, PlantField>> list = new List<KeyValuePair<string, PlantField>>();
			num = text.IndexOf(_strSeparator);
			while (num != -1)
			{
				num2 = text.IndexOf("\"");
				if (num2 != -1 && num2 < num)
				{
					num3 = text.IndexOf("\"", num2 + 1);
					string text2 = text.Substring(num2 + 1, num3 - 1);
					list.Add(new KeyValuePair<string, PlantField>(text2, ManagePlants.GetPlantFieldFromParamenter(text2)));
					text = ((num3 + 2 >= text.Length) ? string.Empty : text.Substring(num3 + 2));
				}
				else
				{
					string text2 = text.Substring(0, num);
					text2 = text2.Trim();
					list.Add(new KeyValuePair<string, PlantField>(text2, ManagePlants.GetPlantFieldFromParamenter(text2)));
					text = text.Substring(num + 1);
				}
				num = text.IndexOf(_strSeparator);
				progressBarForm.UpdateProgressBar(1);
			}
			text = text.Trim();
			text = text.Trim("\""[0]);
			if (text.Length > 0)
			{
				list.Add(new KeyValuePair<string, PlantField>(text, ManagePlants.GetPlantFieldFromParamenter(text)));
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			for (int i = 0; i < list.Count(); i++)
			{
				KeyValuePair<string, PlantField> keyValuePair = list[i];
				if (keyValuePair.Value == PlantField.PlantScientificName)
				{
					flag2 = true;
				}
				else if (keyValuePair.Value == PlantField.PlantCommonName)
				{
					flag3 = true;
				}
				else if (keyValuePair.Value == PlantField.TypeName)
				{
					flag = true;
				}
				else if (keyValuePair.Value == PlantField.PlantingSymbol)
				{
					flag4 = true;
				}
				else if (keyValuePair.Value == PlantField.GenericModelSymbol)
				{
					flag5 = true;
				}
				else if (keyValuePair.Value == PlantField.Height || keyValuePair.Value == PlantField.PlantHeight || keyValuePair.Value == PlantField.PlantWidth || keyValuePair.Value == PlantField.CharacteristicsFoliageSize || keyValuePair.Value == PlantField.PlantSpacing)
				{
					flag6 = true;
				}
				progressBarForm.UpdateProgressBar(2);
			}
			FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
			DisplayUnitType displayUnitType = formatOptions.LegacyDisplayUnits();
			if (!flag && (!flag2 || !flag3))
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ValidTypeCommonOrScientificName"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				progressBarForm.SetMaxValue();
				Thread.Sleep(300);
				progressBarForm.Close();
				streamReader.Close();
				return false;
			}
			if (flag6)
			{
				List<string> list2 = new List<string>();
				list2.Add(EPShared.LocalRM.GetString("UseFeetUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseInchUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseMeterUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseCentimeterUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseMillimeterUnitsOption"));
				int nDefaultOption = -1;
				switch (displayUnitType)
				{
				case DisplayUnitType.DUT_DECIMAL_FEET:
					nDefaultOption = 0;
					break;
				case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				case DisplayUnitType.DUT_FRACTIONAL_INCHES:
				case DisplayUnitType.DUT_DECIMAL_INCHES:
					nDefaultOption = 1;
					break;
				case DisplayUnitType.DUT_METERS:
				case DisplayUnitType.DUT_METERS_CENTIMETERS:
					nDefaultOption = 2;
					break;
				case DisplayUnitType.DUT_CENTIMETERS:
					nDefaultOption = 3;
					break;
				case DisplayUnitType.DUT_MILLIMETERS:
					nDefaultOption = 4;
					break;
				}
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(_Document.PathName, "ReadPlantFile", EPShared.LocalRM.GetString("SelectedFileHasLengthMessage"), list2, nDefaultOption);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult == DialogResult.OK)
				{
					if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseFeetUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_DECIMAL_FEET;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseInchUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_DECIMAL_INCHES;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseMeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_METERS;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseCentimeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_CENTIMETERS;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseMillimeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_MILLIMETERS;
					}
				}
			}
			if (!flag4)
			{
				list.Add(new KeyValuePair<string, PlantField>(ManagePlants.GetPlantParamenterName(PlantField.PlantingSymbol), PlantField.PlantingSymbol));
			}
			if (!flag5)
			{
				list.Add(new KeyValuePair<string, PlantField>(ManagePlants.GetPlantParamenterName(PlantField.GenericModelSymbol), PlantField.GenericModelSymbol));
			}
			string strFileLine;
			while ((strFileLine = streamReader.ReadLine()) != null)
			{
				FamilyTypeContainer _FamilyImportContainer = new FamilyTypeContainer();
				if (!PopulatePlantTableFields(strFileLine, _strSeparator, flag, list, displayUnitType, ref _FamilyImportContainer))
				{
					progressBarForm.SetMaxValue();
					Thread.Sleep(300);
					progressBarForm.Close();
					streamReader.Close();
					return false;
				}
				FamilyImportContainerList.Add(_FamilyImportContainer);
				progressBarForm.UpdateProgressBar(2);
			}
			streamReader.Close();
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
			return true;
		}
		return false;
	}

	private static bool PopulatePlantTableFields(string _strFileLine, string _strSeparator, bool _bTypeNamePresent, List<KeyValuePair<string, PlantField>> _FieldNames, DisplayUnitType _DisplayUnitType, ref FamilyTypeContainer _FamilyImportContainer)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		_FamilyImportContainer.FamilyParameterList = new List<FamilyParameterWrapper>();
		for (int i = 0; i < _FieldNames.Count; i++)
		{
			num = _strFileLine.IndexOf(_strSeparator);
			num2 = _strFileLine.IndexOf("\"");
			string text;
			if (num != -1)
			{
				if (num2 != -1 && num2 < num)
				{
					num3 = _strFileLine.IndexOf("\"", num2 + 1);
					text = _strFileLine.Substring(num2 + 1, num3 - 1);
					_strFileLine = ((num3 + 2 >= _strFileLine.Length) ? string.Empty : _strFileLine.Substring(num3 + 2));
				}
				else
				{
					text = _strFileLine.Substring(0, num);
					text = text.Trim();
					_strFileLine = _strFileLine.Substring(num + 1);
				}
				if (i == _FieldNames.Count - 1 && _strFileLine.IndexOf(_strSeparator) >= 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ColumnMisMatchMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return false;
				}
			}
			else
			{
				_strFileLine = _strFileLine.Trim();
				_strFileLine = _strFileLine.Trim('"');
				text = _strFileLine;
			}
			KeyValuePair<string, PlantField> keyValuePair = _FieldNames[i];
			if (keyValuePair.Value == PlantField.None)
			{
				continue;
			}
			StorageType storageType = StorageType.String;
			if (_bTypeNamePresent && keyValuePair.Value == PlantField.TypeName)
			{
				_FamilyImportContainer.FamilyTypeWrapper = new FamilyTypeWrapper(text);
			}
			else if (!_bTypeNamePresent && keyValuePair.Value == PlantField.PlantScientificName)
			{
				if (_FamilyImportContainer.FamilyTypeWrapper == null)
				{
					_FamilyImportContainer.FamilyTypeWrapper = new FamilyTypeWrapper("/" + text);
				}
				else
				{
					FamilyTypeWrapper familyTypeWrapper = _FamilyImportContainer.FamilyTypeWrapper;
					familyTypeWrapper.FamilyTypeName = familyTypeWrapper.FamilyTypeName + "/" + text;
				}
			}
			else if (!_bTypeNamePresent && keyValuePair.Value == PlantField.PlantCommonName)
			{
				if (_FamilyImportContainer.FamilyTypeWrapper == null)
				{
					_FamilyImportContainer.FamilyTypeWrapper = new FamilyTypeWrapper(text);
				}
				else
				{
					_FamilyImportContainer.FamilyTypeWrapper.FamilyTypeName = text + _FamilyImportContainer.FamilyTypeWrapper.FamilyTypeName;
				}
			}
			else if (keyValuePair.Value == PlantField.Height || keyValuePair.Value == PlantField.PlantHeight || keyValuePair.Value == PlantField.PlantWidth || keyValuePair.Value == PlantField.CharacteristicsFoliageSize || keyValuePair.Value == PlantField.PlantSpacing || keyValuePair.Value == PlantField.GrowthServiceLife)
			{
				storageType = StorageType.Double;
				if (keyValuePair.Value == PlantField.Height || keyValuePair.Value == PlantField.PlantHeight || keyValuePair.Value == PlantField.PlantWidth || keyValuePair.Value == PlantField.CharacteristicsFoliageSize || keyValuePair.Value == PlantField.PlantSpacing)
				{
					double num4 = 1.0;
					if (text != string.Empty)
					{
						num4 = RevitGeometry.ConvertLinearValue(ValueConversion.ToDouble(text), _DisplayUnitType, DisplayUnitType.DUT_DECIMAL_FEET);
					}
					text = ValueConversion.ToString(num4);
				}
			}
			else if (keyValuePair.Value == PlantField.PlantingSymbol || keyValuePair.Value == PlantField.GenericModelSymbol)
			{
				storageType = StorageType.ElementId;
				text = string.Empty;
			}
			FamilyParameterWrapper item = new FamilyParameterWrapper(keyValuePair.Key, keyValuePair.Key, text, storageType);
			_FamilyImportContainer.FamilyParameterList.Add(item);
		}
		FamilyParameterWrapperComparer comparer = new FamilyParameterWrapperComparer();
		_FamilyImportContainer.FamilyParameterList.Sort(comparer);
		return true;
	}

	public static void ExportToFile(Document _Document, string _strSeperator, string _strExportFileName, List<FamilyTypeContainer> _PlantListFamilyTypeList, List<ParameterColumn> _ParameterList)
	{
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		if (File.Exists(_strExportFileName))
		{
			DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("FileExistsOverwriteMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.No)
			{
				return;
			}
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(_strExportFileName, append: false, Encoding.ASCII);
			bool flag = false;
			List<string> list = new List<string>();
			for (num2 = 0; num2 < _ParameterList.Count; num2++)
			{
				if (_ParameterList[num2].PlantField == PlantField.Height || _ParameterList[num2].PlantField == PlantField.PlantHeight || _ParameterList[num2].PlantField == PlantField.PlantWidth || _ParameterList[num2].PlantField == PlantField.CharacteristicsFoliageSize || _ParameterList[num2].PlantField == PlantField.PlantSpacing)
				{
					flag = true;
				}
				list.Add(ManagePlants.GetPlantParamenterName(_ParameterList[num2].PlantField));
			}
			FormatOptions formatOptions = _Document.GetUnits().GetFormatOptions(UnitType.UT_Length);
			DisplayUnitType displayUnitType = formatOptions.LegacyDisplayUnits();
			if (flag)
			{
				List<string> list2 = new List<string>();
				list2.Add(EPShared.LocalRM.GetString("UseFeetUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseInchUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseMeterUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseCentimeterUnitsOption"));
				list2.Add(EPShared.LocalRM.GetString("UseMillimeterUnitsOption"));
				int nDefaultOption = -1;
				switch (displayUnitType)
				{
				case DisplayUnitType.DUT_DECIMAL_FEET:
					nDefaultOption = 0;
					break;
				case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				case DisplayUnitType.DUT_FRACTIONAL_INCHES:
				case DisplayUnitType.DUT_DECIMAL_INCHES:
					nDefaultOption = 1;
					break;
				case DisplayUnitType.DUT_METERS:
				case DisplayUnitType.DUT_METERS_CENTIMETERS:
					nDefaultOption = 2;
					break;
				case DisplayUnitType.DUT_CENTIMETERS:
					nDefaultOption = 3;
					break;
				case DisplayUnitType.DUT_MILLIMETERS:
					nDefaultOption = 4;
					break;
				}
				WarningOptionsForm warningOptionsForm = new WarningOptionsForm(_Document.PathName, "WritePlantFile", EPShared.LocalRM.GetString("FieldsFoundWithLengthMessage"), list2, nDefaultOption);
				warningOptionsForm.ShowDialog();
				if (warningOptionsForm.DialogResult == DialogResult.OK)
				{
					if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseFeetUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_DECIMAL_FEET;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseInchUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_DECIMAL_INCHES;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseMeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_METERS;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseCentimeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_CENTIMETERS;
					}
					else if (string.Compare(warningOptionsForm.WarningOption, EPShared.LocalRM.GetString("UseMillimeterUnitsOption"), ignoreCase: true) == 0)
					{
						displayUnitType = DisplayUnitType.DUT_MILLIMETERS;
					}
				}
			}
			empty = GetFileLine(list, _strSeperator);
			streamWriter.WriteLine(empty);
			bool flag2 = false;
			List<string> list3 = new List<string>();
			FamilyParameterWrapper familyParameterWrapper = null;
			for (int i = 0; i < _PlantListFamilyTypeList.Count; i++)
			{
				for (num2 = 0; num2 < list.Count; num2++)
				{
					for (num = 0; num < _PlantListFamilyTypeList[i].FamilyParameterList.Count; num++)
					{
						familyParameterWrapper = _PlantListFamilyTypeList[i].FamilyParameterList[num];
						if (familyParameterWrapper == null || string.Compare(list[num2], familyParameterWrapper.DisplayName, ignoreCase: true) != 0)
						{
							continue;
						}
						if (string.Compare(familyParameterWrapper.DisplayName, EPShared.LocalRM.GetString("HeightPlantFamilyParameter"), ignoreCase: true) != 0 && string.Compare(familyParameterWrapper.DisplayName, EPShared.LocalRM.GetString("CharacteristicsFoliageSizePlantFamilyParameter"), ignoreCase: true) != 0 && string.Compare(familyParameterWrapper.DisplayName, EPShared.LocalRM.GetString("PlantWidthPlantFamilyParameter"), ignoreCase: true) != 0 && string.Compare(familyParameterWrapper.DisplayName, EPShared.LocalRM.GetString("PlantHeightPlantFamilyParameter"), ignoreCase: true) != 0 && string.Compare(familyParameterWrapper.DisplayName, EPShared.LocalRM.GetString("PlantSpacingPlantFamilyParameter"), ignoreCase: true) != 0)
						{
							list3.Add(familyParameterWrapper.Value);
						}
						else
						{
							double num3 = 0.0;
							if (familyParameterWrapper.Value != string.Empty)
							{
								num3 = RevitGeometry.ConvertLinearValue(ValueConversion.ToDouble(familyParameterWrapper.Value), DisplayUnitType.DUT_DECIMAL_FEET, displayUnitType);
							}
							list3.Add(ValueConversion.ToString(num3));
						}
						flag2 = true;
						break;
					}
					if (!flag2)
					{
						list3.Add(string.Empty);
					}
					else
					{
						flag2 = false;
					}
				}
				empty = GetFileLine(list3, _strSeperator);
				streamWriter.WriteLine(empty);
				list3.Clear();
			}
			streamWriter.Close();
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ExportCompleteMessage"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			Process.Start("\"" + _strExportFileName + "\"");
		}
		catch (Exception ex)
		{
			RevitMessageBox.Show(ex.Message, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}

	private static string GetFileLine(List<string> paRowData, string szSeperator)
	{
		string text = string.Empty;
		for (int i = 0; i < paRowData.Count; i++)
		{
			if (i != 0)
			{
				text += szSeperator;
			}
			text += "\"";
			text += PadQuotesInString(paRowData[i]);
			text += "\"";
		}
		return text;
	}

	private static string PadQuotesInString(string strToPad)
	{
		string text = string.Empty;
		int num = -1;
		if (strToPad != null && strToPad != string.Empty)
		{
			while ((num = strToPad.IndexOf("\"")) >= 0)
			{
				text += strToPad.Substring(0, num + 1);
				text += "\"";
				strToPad = strToPad.Substring(num + 1);
			}
		}
		return text + strToPad;
	}

	public static void ExportToFile(string _strSeperator, string _strExportFileName, List<ExportReportLine> _ReportLineList)
	{
		string empty = string.Empty;
		if (File.Exists(_strExportFileName))
		{
			DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("FileExistsOverwriteMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.No)
			{
				return;
			}
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(_strExportFileName, append: false, Encoding.ASCII);
			List<string> list = new List<string>();
			for (int i = 0; i < _ReportLineList.Count; i++)
			{
				if (!_ReportLineList[i].BlankLine)
				{
					list.Add(_ReportLineList[i].LevelName);
					list.Add(_ReportLineList[i].Parameter);
					list.Add(_ReportLineList[i].Value);
					empty = GetFileLine(list, _strSeperator);
					streamWriter.WriteLine(empty);
					list.Clear();
				}
				else
				{
					streamWriter.WriteLine();
				}
			}
			streamWriter.Close();
			RevitMessageBox.Show(EPShared.LocalRM.GetString("ExportCompleteMessage"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			Process.Start("\"" + _strExportFileName + "\"");
		}
		catch (Exception ex)
		{
			RevitMessageBox.Show(ex.Message, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}
}
