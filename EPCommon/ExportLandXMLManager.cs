using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Autodesk.Revit.DB;

namespace EPCommon;

public class ExportLandXMLManager : EPForm
{
	private Document m_CurrentDocument;

	private IContainer components;

	private GroupBox m_ExportInformation;

	private Label m_ProjectName;

	private Button m_ButtonOk;

	private Button m_ButtonCancel;

	private CheckBox m_ExportSurfaceChkBox;

	private CheckBox m_ExportAlignmentChkBox;

	private TextBox m_CompanyTextBox;

	private TextBox m_EmailTextBox;

	private TextBox m_CreatedByTextBox;

	private TextBox m_ProjectTextBox;

	private Label m_CompanyLbl;

	private Label m_EmailLbl;

	private Label m_CreatedByLbl;

	private Button m_FileOpen;

	private TextBox m_FileNameTextBox;

	private Label m_FileName;

	private CheckBox m_BySharedCoordinatesChkBox;

	public ExportLandXMLManager(Document _CurrentDocument)
	{
		m_CurrentDocument = _CurrentDocument;
		InitializeComponent();
		ContextualHelpID = "HID_SDexportLandXML";
		SetupStrings();
		GetDialogPosition();
	}

	private void SetupStrings()
	{
		Text = EPShared.LocalRM.GetString("ExportLandXMLString");
		m_ButtonCancel.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		m_ButtonOk.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
		m_ExportInformation.Text = EPShared.LocalRM.GetString("UserInformationString");
		m_ExportSurfaceChkBox.Text = EPShared.LocalRM.GetString("Export_AMP_TopoSurfacesExportXMLDialog");
		m_ExportAlignmentChkBox.Text = EPShared.LocalRM.GetString("Export_AMP_AligmentsExportXMLDialog");
		m_ProjectName.Text = EPShared.LocalRM.GetString("AMP_ProjectNameExportXMLDialog");
		m_CompanyLbl.Text = EPShared.LocalRM.GetString("AMP_CompanyExportXMLDialog");
		m_EmailLbl.Text = EPShared.LocalRM.GetString("Created_AMP_ByExportXMLDialog");
		m_CreatedByLbl.Text = EPShared.LocalRM.GetString("CreatedBy_AMP_EmailExportXMLDialog");
		m_FileName.Text = EPShared.LocalRM.GetString("AMP_FileNameExportXMLDialog");
		m_BySharedCoordinatesChkBox.Text = EPShared.LocalRM.GetString("B_AMP_ySurveyPointsDialog");
		int num = m_CurrentDocument.PathName.LastIndexOf("\\");
		string text = string.Empty;
		if (num > 0)
		{
			text = m_CurrentDocument.PathName.Substring(num + 1);
		}
		m_ProjectTextBox.Text = text;
		m_CreatedByTextBox.Text = Environment.UserName;
		string _strReturnValue = string.Empty;
		m_EmailTextBox.Text = _strReturnValue;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ExportLandXMLEmailText", ref _strReturnValue))
		{
			m_EmailTextBox.Text = _strReturnValue;
		}
		string _strReturnValue2 = string.Empty;
		m_CompanyTextBox.Text = _strReturnValue2;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ExportLandXMLCompanyText", ref _strReturnValue2))
		{
			m_CompanyTextBox.Text = _strReturnValue2;
		}
		int _nReturnValue = 0;
		m_ExportSurfaceChkBox.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ExportLandXMLSurface", ref _nReturnValue))
		{
			m_ExportSurfaceChkBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_ExportAlignmentChkBox.Checked = true;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ExportLandXMLAlignment", ref _nReturnValue))
		{
			m_ExportAlignmentChkBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
		m_BySharedCoordinatesChkBox.Checked = false;
		if (ApplicationUtility.Settings.eplSetE_GetSetting("ExportLandXMLUseSharedCoordinates", ref _nReturnValue))
		{
			m_BySharedCoordinatesChkBox.Checked = Convert.ToBoolean(_nReturnValue);
		}
	}

	private void SaveSettings()
	{
		ApplicationUtility.Settings.eplSetE_SetSetting("ExportLandXMLEmailText", m_EmailTextBox.Text);
		ApplicationUtility.Settings.eplSetE_SetSetting("ExportLandXMLCompanyText", m_CompanyTextBox.Text);
		ApplicationUtility.Settings.eplSetE_SetSetting("ExportLandXMLSurface", Convert.ToInt32(m_ExportSurfaceChkBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting("ExportLandXMLAlignment", Convert.ToInt32(m_ExportAlignmentChkBox.Checked));
		ApplicationUtility.Settings.eplSetE_SetSetting("ExportLandXMLUseSharedCoordinates", Convert.ToInt32(m_BySharedCoordinatesChkBox.Checked));
	}

	private void OnOkButtonClicked(object sender, EventArgs e)
	{
		if (!VerifyFileNameAndPath())
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("InvalidFileNameLocationMessageString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		SaveSettings();
		if (!m_ExportAlignmentChkBox.Checked && !m_ExportSurfaceChkBox.Checked)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoExportOptionsSelectedString"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		DialogResult dialogResult = DialogResult.Cancel;
		DialogResult dialogResult2 = DialogResult.Cancel;
		List<LandXMLAlignment> _AlignmentList = new List<LandXMLAlignment>();
		List<TopographySurfaceContainer> _SurfaceList = new List<TopographySurfaceContainer>();
		if (m_ExportSurfaceChkBox.Checked)
		{
			using ExportLandXMLTypeList exportLandXMLTypeList = new ExportLandXMLTypeList(LandXMLElementType.Surface, m_CurrentDocument, ref _SurfaceList);
			if (exportLandXMLTypeList.ElementCount <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSurfacesInProjectString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			}
			else
			{
				exportLandXMLTypeList.ShowDialog();
				dialogResult = exportLandXMLTypeList.DialogResult;
			}
		}
		if (m_ExportAlignmentChkBox.Checked)
		{
			using ExportLandXMLTypeList exportLandXMLTypeList2 = new ExportLandXMLTypeList(LandXMLElementType.Allignment, m_CurrentDocument, ref _AlignmentList);
			if (exportLandXMLTypeList2.ElementCount <= 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("NoAlignmentInProjectString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			}
			else
			{
				exportLandXMLTypeList2.ShowDialog();
				dialogResult2 = exportLandXMLTypeList2.DialogResult;
			}
		}
		if ((dialogResult == DialogResult.OK || dialogResult2 == DialogResult.OK) && (_SurfaceList.Count > 0 || _AlignmentList.Count > 0))
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(m_FileNameTextBox.Text, Encoding.UTF8);
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.WriteStartDocument(standalone: false);
			xmlTextWriter.WriteStartElement("LandXML");
			xmlTextWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			xmlTextWriter.WriteAttributeString("xsi:schemaLocation", "http://www.landxml.org/schema/LandXML-1.1 http://www.landxml.org/schema/LandXML-1.1/LandXML-1.1.xsd");
			xmlTextWriter.WriteAttributeString("xmlns", "http://www.landxml.org/schema/LandXML-1.1");
			xmlTextWriter.WriteAttributeString("version", "1.1");
			string value = DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00");
			xmlTextWriter.WriteAttributeString("date", value);
			string value2 = DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Minute.ToString("00");
			xmlTextWriter.WriteAttributeString("time", value2);
			xmlTextWriter.WriteAttributeString("readOnly", "false");
			xmlTextWriter.WriteAttributeString("language", "English");
			xmlTextWriter.WriteStartElement("Project");
			xmlTextWriter.WriteAttributeString("name", m_ProjectTextBox.Text);
			xmlTextWriter.WriteEndElement();
			string text = "squareFoot";
			string strCurrentUnit = "foot";
			string text2 = "cubicFeet";
			string text3 = "fahrenheit";
			string text4 = "inHG";
			string text5 = "Imperial";
			if (m_CurrentDocument.DisplayUnitSystem == DisplayUnit.IMPERIAL)
			{
				text5 = "Imperial";
				text4 = "inHG";
				text3 = "fahrenheit";
			}
			else
			{
				text5 = "Metric";
				text4 = "mmHG";
				text3 = "celsius";
			}
			FormatOptions formatOptions = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Length);
			switch (formatOptions.LegacyDisplayUnits())
			{
			case DisplayUnitType.DUT_DECIMAL_FEET:
				strCurrentUnit = "foot";
				break;
			case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
				strCurrentUnit = "foot";
				break;
			case DisplayUnitType.DUT_DECIMAL_INCHES:
				strCurrentUnit = "inch";
				break;
			case DisplayUnitType.DUT_FRACTIONAL_INCHES:
				strCurrentUnit = "foot";
				break;
			case DisplayUnitType.DUT_METERS:
				strCurrentUnit = "meter";
				break;
			case DisplayUnitType.DUT_CENTIMETERS:
				strCurrentUnit = "centimeter";
				break;
			case DisplayUnitType.DUT_MILLIMETERS:
				strCurrentUnit = "millimeter";
				break;
			case DisplayUnitType.DUT_METERS_CENTIMETERS:
				strCurrentUnit = "meter";
				break;
			}
			FormatOptions formatOptions2 = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Area);
			switch (formatOptions2.LegacyDisplayUnits())
			{
			case DisplayUnitType.DUT_SQUARE_FEET:
				text = "squareFoot";
				break;
			case DisplayUnitType.DUT_SQUARE_INCHES:
				text = "squareInch";
				break;
			case DisplayUnitType.DUT_SQUARE_METERS:
				text = "squareMeter";
				break;
			case DisplayUnitType.DUT_SQUARE_CENTIMETERS:
				text = "squareCentimeter";
				break;
			case DisplayUnitType.DUT_SQUARE_MILLIMETERS:
				text = "squareMillimeter";
				break;
			case DisplayUnitType.DUT_ACRES:
				text = "acre";
				break;
			case DisplayUnitType.DUT_HECTARES:
				text = "hectare";
				break;
			}
			FormatOptions formatOptions3 = m_CurrentDocument.GetUnits().GetFormatOptions(UnitType.UT_Volume);
			switch (formatOptions3.LegacyDisplayUnits())
			{
			case DisplayUnitType.DUT_CUBIC_YARDS:
				text2 = "cubicYard";
				break;
			case DisplayUnitType.DUT_CUBIC_FEET:
				text2 = "cubicFeet";
				break;
			case DisplayUnitType.DUT_CUBIC_INCHES:
				text2 = "cubicInch";
				break;
			case DisplayUnitType.DUT_CUBIC_METERS:
				text2 = "cubicMeter";
				break;
			case DisplayUnitType.DUT_CUBIC_CENTIMETERS:
				text2 = "cubicMeter";
				break;
			case DisplayUnitType.DUT_CUBIC_MILLIMETERS:
				text2 = "cubicMeter";
				break;
			case DisplayUnitType.DUT_LITERS:
				text2 = "liter";
				break;
			case DisplayUnitType.DUT_GALLONS_US:
				text2 = "liter";
				break;
			}
			xmlTextWriter.WriteStartElement("Units");
			xmlTextWriter.WriteStartElement(text5);
			xmlTextWriter.WriteStartAttribute("areaUnit");
			xmlTextWriter.WriteString(text);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("linearUnit");
			xmlTextWriter.WriteString(strCurrentUnit);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("volumeUnit");
			xmlTextWriter.WriteString(text2);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("temperatureUnit");
			xmlTextWriter.WriteString(text3);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("pressureUnit");
			xmlTextWriter.WriteString(text4);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteStartElement("Application");
			xmlTextWriter.WriteStartAttribute("name");
			xmlTextWriter.WriteString($"Site Designer for Revit {ApplicationUtility.RevitVersion}");
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("manufacturer");
			xmlTextWriter.WriteString("Autodesk");
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("manufacturerURL");
			xmlTextWriter.WriteString("www.autodesk.com");
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartElement("Author");
			xmlTextWriter.WriteStartAttribute("createdBy");
			xmlTextWriter.WriteString(m_CreatedByTextBox.Text);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("createdByEmail");
			xmlTextWriter.WriteString(m_EmailTextBox.Text);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteStartAttribute("company");
			xmlTextWriter.WriteString(m_CompanyTextBox.Text);
			xmlTextWriter.WriteEndAttribute();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndElement();
			double _dPolarSlopeDistance = 0.0;
			double _dHorzPolarDirection = 0.0;
			double _dVertPolarDirection = 0.0;
			double num = 0.0;
			double _dDirection = 0.0;
			double _dDistance = 0.0;
			Eps_Point _NewPnt = null;
			if (m_BySharedCoordinatesChkBox.Checked)
			{
				ProjectLocation activeProjectLocation = m_CurrentDocument.ActiveProjectLocation;
				XYZ point = new XYZ();
				ProjectPosition projectPosition = activeProjectLocation.GetProjectPosition(point);
				Eps_Point eps_Point = new Eps_Point();
				eps_Point.x = projectPosition.EastWest;
				eps_Point.y = projectPosition.NorthSouth;
				eps_Point.z = projectPosition.Elevation;
				num = projectPosition.Angle;
				EPShared.eplGeoE_CalcXYZToPolar(eps_Point, new Eps_Point(), ref _dHorzPolarDirection, ref _dVertPolarDirection, ref _dPolarSlopeDistance);
			}
			if (_SurfaceList.Count > 0)
			{
				xmlTextWriter.WriteStartElement("Surfaces");
				xmlTextWriter.WriteAttributeString("name", "");
				xmlTextWriter.WriteAttributeString("desc", "");
				foreach (TopographySurfaceContainer item in _SurfaceList)
				{
					string value3 = string.Empty;
					Parameter parameter = ((Element)item.TopographySurface).get_Parameter(BuiltInParameter.ROOM_NAME);
					if (parameter != null && parameter.AsString() != null && parameter.AsString() != string.Empty)
					{
						value3 = parameter.AsString();
					}
					string value4 = string.Empty;
					parameter = ((Element)item.TopographySurface).get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
					if (parameter != null)
					{
						value4 = parameter.AsString();
					}
					string empty = string.Empty;
					long num2 = 0;
					parameter = ((Element)item.TopographySurface).get_Parameter(BuiltInParameter.PHASE_DEMOLISHED);
					if (parameter != null && parameter.AsElementId().Value64() >= 0)
					{
						foreach (Phase phase3 in m_CurrentDocument.Phases)
						{
							if (phase3.Id.Value64() == parameter.AsElementId().Value64())
							{
								num2 = parameter.AsElementId().Value64();
								break;
							}
						}
					}
					else
					{
						parameter = ((Element)item.TopographySurface).get_Parameter(BuiltInParameter.PHASE_CREATED);
						foreach (Phase phase4 in m_CurrentDocument.Phases)
						{
							if (phase4.Id.Value64() == parameter.AsElementId().Value64())
							{
								num2 = parameter.AsElementId().Value64();
								break;
							}
						}
					}
					if (num2 <= 12589)
					{
						if (num2 == 0 || num2 != 12589)
						{
							goto IL_09f5;
						}
					}
					else if (num2 != 32440)
					{
						if (num2 != 86961)
						{
							_ = 118390;
						}
						goto IL_09f5;
					}
					empty = "existing";
					goto IL_0a05;
					IL_09f5:
					empty = "proposed";
					goto IL_0a05;
					IL_0a05:
					bool flag = m_BySharedCoordinatesChkBox.Checked;
					if (!flag)
					{
						_dPolarSlopeDistance = 0.0;
						_dHorzPolarDirection = 0.0;
						_dVertPolarDirection = 0.0;
						FamilyInstance siteComponentPlacementFamilyInstance = ImportLandXMLClass.GetSiteComponentPlacementFamilyInstance(m_CurrentDocument, item.TopographySurface.UniqueId, _bCheckForSurface: true);
						if (siteComponentPlacementFamilyInstance != null)
						{
							flag = ImportLandXMLClass.FindPositioningParameters(siteComponentPlacementFamilyInstance, ref _dPolarSlopeDistance, ref _dHorzPolarDirection, ref _dVertPolarDirection);
						}
					}
					xmlTextWriter.WriteStartElement("Surface");
					xmlTextWriter.WriteAttributeString("name", value3);
					xmlTextWriter.WriteAttributeString("desc", value4);
					xmlTextWriter.WriteAttributeString("state", empty);
					xmlTextWriter.WriteStartElement("Definition");
					xmlTextWriter.WriteAttributeString("surfType", "TIN");
					xmlTextWriter.WriteStartElement("Pnts");
					item.TopoPoints.Sort((TopoPoint p1, TopoPoint p2) => p1.Number.CompareTo(p2.Number));
					foreach (TopoPoint topoPoint in item.TopoPoints)
					{
						xmlTextWriter.WriteStartElement("P");
						xmlTextWriter.WriteAttributeString("id", topoPoint.Number.ToString());
						if (flag)
						{
							_NewPnt = new Eps_Point(topoPoint.x, topoPoint.y, topoPoint.z);
							EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), _NewPnt, ref _dDirection);
							EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), _NewPnt, ref _dDistance);
							EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, _NewPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
							Eps_Point _NewPnt2 = new Eps_Point();
							EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt2);
							xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt2.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt2.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt2.z));
						}
						else
						{
							xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, topoPoint.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, topoPoint.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, topoPoint.z));
						}
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteStartElement("Faces");
					foreach (Eps_Triangle triangle in item.Triangles)
					{
						xmlTextWriter.WriteStartElement("F");
						xmlTextWriter.WriteString(triangle.Verticies[0].Number + " " + triangle.Verticies[1].Number + " " + triangle.Verticies[2].Number);
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();
			}
			if (_AlignmentList.Count > 0)
			{
				xmlTextWriter.WriteStartElement("Alignments");
				xmlTextWriter.WriteAttributeString("name", "");
				foreach (LandXMLAlignment item2 in _AlignmentList)
				{
					xmlTextWriter.WriteStartElement("Alignment");
					xmlTextWriter.WriteAttributeString("name", item2.Name);
					xmlTextWriter.WriteAttributeString("length", item2.Length);
					xmlTextWriter.WriteAttributeString("staStart", "0");
					xmlTextWriter.WriteStartElement("Start");
					if (m_BySharedCoordinatesChkBox.Checked)
					{
						EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), item2.StartPoint, ref _dDirection);
						EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), item2.StartPoint, ref _dDistance);
						EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, item2.StartPoint.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
						Eps_Point _NewPnt3 = new Eps_Point();
						EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt3);
						xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt3.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt3.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt3.z));
					}
					else
					{
						xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, item2.StartPoint.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, item2.StartPoint.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, item2.StartPoint.z));
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteStartElement("CoordGeom");
					Eps_Point eps_Point2 = item2.StartPoint;
					bool flag2 = true;
					while ((item2.Lines.Count > 0 || item2.Curves.Count > 0) && flag2)
					{
						flag2 = false;
						for (int num3 = 0; num3 < item2.Lines.Count; num3++)
						{
							Eps_Line eps_Line = item2.Lines[num3];
							if (eps_Point2.IsEqual(eps_Line.StartPnt))
							{
								xmlTextWriter.WriteStartElement("Line");
								double _dDirection2 = 0.0;
								string value5 = string.Empty;
								if (EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDirection2) == 0)
								{
									value5 = ValueConversion.ToString(_dDirection2);
								}
								xmlTextWriter.WriteAttributeString("dir", value5);
								double _dDistance2 = 0.0;
								string value6 = string.Empty;
								if (EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(eps_Line.StartPnt, eps_Line.EndPnt, ref _dDistance2) == 0)
								{
									value6 = ValueConversion.ToString(_dDistance2);
								}
								xmlTextWriter.WriteAttributeString("length", value6);
								xmlTextWriter.WriteStartElement("Start");
								if (m_BySharedCoordinatesChkBox.Checked)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), eps_Line.StartPnt, ref _dDirection);
									EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), eps_Line.StartPnt, ref _dDistance);
									EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, eps_Line.StartPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
									Eps_Point _NewPnt4 = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt4);
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt4.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt4.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt4.z));
								}
								else
								{
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, eps_Line.StartPnt.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Line.StartPnt.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Line.StartPnt.z));
								}
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.WriteStartElement("End");
								if (m_BySharedCoordinatesChkBox.Checked)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), eps_Line.EndPnt, ref _dDirection);
									EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), eps_Line.EndPnt, ref _dDistance);
									EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, eps_Line.EndPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
									Eps_Point _NewPnt5 = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt5);
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt5.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt5.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt5.z));
								}
								else
								{
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, eps_Line.EndPnt.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Line.EndPnt.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Line.EndPnt.z));
								}
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.WriteEndElement();
								flag2 = true;
								eps_Point2 = eps_Line.EndPnt;
								item2.Lines.RemoveAt(num3);
								num3--;
							}
						}
						for (int num4 = 0; num4 < item2.Curves.Count; num4++)
						{
							Eps_Arc eps_Arc = item2.Curves[num4];
							if (eps_Point2.IsEqual(eps_Arc.StartPnt))
							{
								xmlTextWriter.WriteStartElement("Curve");
								string value7 = "cw";
								if (eps_Arc.Direction == 1)
								{
									value7 = "ccw";
								}
								xmlTextWriter.WriteAttributeString("rot", value7);
								xmlTextWriter.WriteAttributeString("delta", ValueConversion.ToString(eps_Arc.Delta));
								double num5 = EPShared.eplGeoE_SolveArcLenByRadiusAndDelta(eps_Arc.Radius, eps_Arc.Delta);
								xmlTextWriter.WriteAttributeString("length", ValueConversion.ToString(num5));
								xmlTextWriter.WriteAttributeString("radius", ValueConversion.ToString(eps_Arc.Radius));
								xmlTextWriter.WriteStartElement("Start");
								if (m_BySharedCoordinatesChkBox.Checked)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), eps_Arc.StartPnt, ref _dDirection);
									EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), eps_Arc.StartPnt, ref _dDistance);
									EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, eps_Arc.StartPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
									Eps_Point _NewPnt6 = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt6);
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt6.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt6.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt6.z));
								}
								else
								{
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.StartPnt.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.StartPnt.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.StartPnt.z));
								}
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.WriteStartElement("Center");
								if (m_BySharedCoordinatesChkBox.Checked)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), eps_Arc.RadiusPnt, ref _dDirection);
									EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), eps_Arc.RadiusPnt, ref _dDistance);
									EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, eps_Arc.RadiusPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
									Eps_Point _NewPnt7 = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt7);
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt7.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt7.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt7.z));
								}
								else
								{
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.RadiusPnt.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.RadiusPnt.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.RadiusPnt.z));
								}
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.WriteStartElement("End");
								if (m_BySharedCoordinatesChkBox.Checked)
								{
									EPShared.eplGeoE_CalcHorzDirBtwnTwoPnts(new Eps_Point(), eps_Arc.EndPnt, ref _dDirection);
									EPShared.eplGeoE_CalcHorzDistBtwnTwoPnts(new Eps_Point(), eps_Arc.EndPnt, ref _dDistance);
									EPShared.eplGeoE_CalcPolarToXY(new Eps_Point(0.0, 0.0, eps_Arc.EndPnt.z), EPShared.eplGeoE_NormalizeRadians(_dDirection + num), _dDistance, ref _NewPnt);
									Eps_Point _NewPnt8 = new Eps_Point();
									EPShared.eplGeoE_CalcPolarToXYZ(_NewPnt, EPShared.eplGeoE_NormalizeRadians(_dHorzPolarDirection + Math.PI), 0.0 - _dVertPolarDirection, _dPolarSlopeDistance, ref _NewPnt8);
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, _NewPnt8.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt8.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, _NewPnt8.z));
								}
								else
								{
									xmlTextWriter.WriteString(ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.EndPnt.y) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.EndPnt.x) + " " + ConvertToLandXMLUnits(strCurrentUnit, eps_Arc.EndPnt.z));
								}
								xmlTextWriter.WriteEndElement();
								xmlTextWriter.WriteEndElement();
								flag2 = true;
								eps_Point2 = eps_Arc.EndPnt;
								item2.Curves.RemoveAt(num4);
								num4--;
							}
						}
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndDocument();
			xmlTextWriter.Close();
			if (m_ExportAlignmentChkBox.Checked && m_ExportSurfaceChkBox.Checked)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("ExportLandXMLComponentsCompleteString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
			}
			else if (m_ExportAlignmentChkBox.Checked)
			{
				if (_AlignmentList.Count > 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ExportAlignmentLandXMLCompleteString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoAlignmentToExportString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
			}
			else if (m_ExportSurfaceChkBox.Checked)
			{
				if (_SurfaceList.Count > 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ExportSurfaceLandXMLCompleteString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
				else
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("NoSurfacesToExportString"), EPShared.LocalRM.GetString("InformationString"), MessageBoxIcon.Asterisk);
				}
			}
		}
		DialogResult = DialogResult.OK;
	}

	private string ConvertToLandXMLUnits(string _strCurrentUnit, double _dValue)
	{
		double num = 0.0;
		if (string.Compare(_strCurrentUnit, "foot", ignoreCase: true) == 0)
		{
			num = _dValue;
			return ValueConversion.ToString(num);
		}
		if (string.Compare(_strCurrentUnit, "inch", ignoreCase: true) == 0)
		{
			num = _dValue * 12.0;
			return ValueConversion.ToString(num);
		}
		if (string.Compare(_strCurrentUnit, "meter", ignoreCase: true) == 0)
		{
			num = _dValue * 0.3048;
			return ValueConversion.ToString(num);
		}
		if (string.Compare(_strCurrentUnit, "centimeter", ignoreCase: true) == 0)
		{
			num = _dValue * 0.3048 * 100.0;
			return ValueConversion.ToString(num);
		}
		if (string.Compare(_strCurrentUnit, "millimeter", ignoreCase: true) == 0)
		{
			num = _dValue * 0.3048 * 1000.0;
			return ValueConversion.ToString(num);
		}
		num = _dValue;
		return ValueConversion.ToString(num);
	}

	private void OnCanceButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
	}

	private void OnExportXMLClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private bool VerifyFileNameAndPath()
	{
		if (string.IsNullOrEmpty(m_FileNameTextBox.Text))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(m_FileNameTextBox.Text))
		{
			FileInfo fileInfo = new FileInfo(m_FileNameTextBox.Text);
			if (File.Exists(m_FileNameTextBox.Text) && string.Compare(fileInfo.Extension, ".xml", ignoreCase: true) != 0)
			{
				RevitMessageBox.Show(EPShared.LocalRM.GetString("Source") + " " + EPShared.LocalRM.GetString("NoXMLExtension"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
				return false;
			}
		}
		return true;
	}

	private void OnXMLFileOpenClicked(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ApplicationUtility.GetString("XmlFilePickerFilter");
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			m_FileNameTextBox.Text = saveFileDialog.FileName;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.m_ExportInformation = new System.Windows.Forms.GroupBox();
		this.m_CompanyTextBox = new System.Windows.Forms.TextBox();
		this.m_EmailTextBox = new System.Windows.Forms.TextBox();
		this.m_CreatedByTextBox = new System.Windows.Forms.TextBox();
		this.m_ProjectTextBox = new System.Windows.Forms.TextBox();
		this.m_CompanyLbl = new System.Windows.Forms.Label();
		this.m_EmailLbl = new System.Windows.Forms.Label();
		this.m_CreatedByLbl = new System.Windows.Forms.Label();
		this.m_ProjectName = new System.Windows.Forms.Label();
		this.m_ButtonOk = new System.Windows.Forms.Button();
		this.m_ButtonCancel = new System.Windows.Forms.Button();
		this.m_ExportSurfaceChkBox = new System.Windows.Forms.CheckBox();
		this.m_ExportAlignmentChkBox = new System.Windows.Forms.CheckBox();
		this.m_FileOpen = new System.Windows.Forms.Button();
		this.m_FileNameTextBox = new System.Windows.Forms.TextBox();
		this.m_FileName = new System.Windows.Forms.Label();
		this.m_BySharedCoordinatesChkBox = new System.Windows.Forms.CheckBox();
		this.m_ExportInformation.SuspendLayout();
		base.SuspendLayout();
		this.m_ExportInformation.Controls.Add(this.m_CompanyTextBox);
		this.m_ExportInformation.Controls.Add(this.m_EmailTextBox);
		this.m_ExportInformation.Controls.Add(this.m_CreatedByTextBox);
		this.m_ExportInformation.Controls.Add(this.m_ProjectTextBox);
		this.m_ExportInformation.Controls.Add(this.m_CompanyLbl);
		this.m_ExportInformation.Controls.Add(this.m_EmailLbl);
		this.m_ExportInformation.Controls.Add(this.m_CreatedByLbl);
		this.m_ExportInformation.Controls.Add(this.m_ProjectName);
		this.m_ExportInformation.Location = new System.Drawing.Point(12, 32);
		this.m_ExportInformation.Name = "m_ExportInformation";
		this.m_ExportInformation.Size = new System.Drawing.Size(369, 123);
		this.m_ExportInformation.TabIndex = 3;
		this.m_ExportInformation.TabStop = false;
		this.m_ExportInformation.Text = "User Information";
		this.m_CompanyTextBox.Location = new System.Drawing.Point(126, 97);
		this.m_CompanyTextBox.Name = "m_CompanyTextBox";
		this.m_CompanyTextBox.Size = new System.Drawing.Size(235, 20);
		this.m_CompanyTextBox.TabIndex = 7;
		this.m_EmailTextBox.Location = new System.Drawing.Point(126, 71);
		this.m_EmailTextBox.Name = "m_EmailTextBox";
		this.m_EmailTextBox.Size = new System.Drawing.Size(235, 20);
		this.m_EmailTextBox.TabIndex = 5;
		this.m_CreatedByTextBox.Location = new System.Drawing.Point(126, 45);
		this.m_CreatedByTextBox.Name = "m_CreatedByTextBox";
		this.m_CreatedByTextBox.Size = new System.Drawing.Size(235, 20);
		this.m_CreatedByTextBox.TabIndex = 3;
		this.m_ProjectTextBox.Location = new System.Drawing.Point(126, 19);
		this.m_ProjectTextBox.Name = "m_ProjectTextBox";
		this.m_ProjectTextBox.Size = new System.Drawing.Size(235, 20);
		this.m_ProjectTextBox.TabIndex = 1;
		this.m_CompanyLbl.AutoSize = true;
		this.m_CompanyLbl.Location = new System.Drawing.Point(6, 100);
		this.m_CompanyLbl.Name = "m_CompanyLbl";
		this.m_CompanyLbl.Size = new System.Drawing.Size(51, 13);
		this.m_CompanyLbl.TabIndex = 6;
		this.m_CompanyLbl.Text = "Company";
		this.m_EmailLbl.AutoSize = true;
		this.m_EmailLbl.Location = new System.Drawing.Point(6, 74);
		this.m_EmailLbl.Name = "m_EmailLbl";
		this.m_EmailLbl.Size = new System.Drawing.Size(86, 13);
		this.m_EmailLbl.TabIndex = 4;
		this.m_EmailLbl.Text = "Created by Email";
		this.m_CreatedByLbl.AutoSize = true;
		this.m_CreatedByLbl.Location = new System.Drawing.Point(6, 48);
		this.m_CreatedByLbl.Name = "m_CreatedByLbl";
		this.m_CreatedByLbl.Size = new System.Drawing.Size(58, 13);
		this.m_CreatedByLbl.TabIndex = 2;
		this.m_CreatedByLbl.Text = "Created by";
		this.m_ProjectName.AutoSize = true;
		this.m_ProjectName.Location = new System.Drawing.Point(6, 22);
		this.m_ProjectName.Name = "m_ProjectName";
		this.m_ProjectName.Size = new System.Drawing.Size(40, 13);
		this.m_ProjectName.TabIndex = 0;
		this.m_ProjectName.Text = "Project";
		this.m_ButtonOk.Location = new System.Drawing.Point(225, 230);
		this.m_ButtonOk.Name = "m_ButtonOk";
		this.m_ButtonOk.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonOk.TabIndex = 7;
		this.m_ButtonOk.Text = "Ok";
		this.m_ButtonOk.UseVisualStyleBackColor = true;
		this.m_ButtonOk.Click += new System.EventHandler(this.OnOkButtonClicked);
		this.m_ButtonCancel.Location = new System.Drawing.Point(306, 230);
		this.m_ButtonCancel.Name = "m_ButtonCancel";
		this.m_ButtonCancel.Size = new System.Drawing.Size(75, 23);
		this.m_ButtonCancel.TabIndex = 8;
		this.m_ButtonCancel.Text = "Cancel";
		this.m_ButtonCancel.UseVisualStyleBackColor = true;
		this.m_ButtonCancel.Click += new System.EventHandler(this.OnCanceButtonClicked);
		this.m_ExportSurfaceChkBox.AutoSize = true;
		this.m_ExportSurfaceChkBox.Location = new System.Drawing.Point(12, 161);
		this.m_ExportSurfaceChkBox.Name = "m_ExportSurfaceChkBox";
		this.m_ExportSurfaceChkBox.Size = new System.Drawing.Size(162, 17);
		this.m_ExportSurfaceChkBox.TabIndex = 4;
		this.m_ExportSurfaceChkBox.Text = "Export Surfaces to LandXML";
		this.m_ExportSurfaceChkBox.UseVisualStyleBackColor = true;
		this.m_ExportAlignmentChkBox.AutoSize = true;
		this.m_ExportAlignmentChkBox.Location = new System.Drawing.Point(12, 184);
		this.m_ExportAlignmentChkBox.Name = "m_ExportAlignmentChkBox";
		this.m_ExportAlignmentChkBox.Size = new System.Drawing.Size(187, 17);
		this.m_ExportAlignmentChkBox.TabIndex = 5;
		this.m_ExportAlignmentChkBox.Text = "Export Alignments to LandXML file";
		this.m_ExportAlignmentChkBox.UseVisualStyleBackColor = true;
		this.m_FileOpen.Location = new System.Drawing.Point(357, 6);
		this.m_FileOpen.Name = "m_FileOpen";
		this.m_FileOpen.Size = new System.Drawing.Size(24, 20);
		this.m_FileOpen.TabIndex = 2;
		this.m_FileOpen.Text = "...";
		this.m_FileOpen.UseVisualStyleBackColor = true;
		this.m_FileOpen.Click += new System.EventHandler(this.OnXMLFileOpenClicked);
		this.m_FileNameTextBox.Location = new System.Drawing.Point(138, 6);
		this.m_FileNameTextBox.Name = "m_FileNameTextBox";
		this.m_FileNameTextBox.Size = new System.Drawing.Size(213, 20);
		this.m_FileNameTextBox.TabIndex = 1;
		this.m_FileName.AutoSize = true;
		this.m_FileName.Location = new System.Drawing.Point(12, 9);
		this.m_FileName.Name = "m_FileName";
		this.m_FileName.Size = new System.Drawing.Size(35, 13);
		this.m_FileName.TabIndex = 0;
		this.m_FileName.Text = "Name";
		this.m_BySharedCoordinatesChkBox.AutoSize = true;
		this.m_BySharedCoordinatesChkBox.Location = new System.Drawing.Point(12, 207);
		this.m_BySharedCoordinatesChkBox.Name = "m_BySharedCoordinatesChkBox";
		this.m_BySharedCoordinatesChkBox.Size = new System.Drawing.Size(141, 17);
		this.m_BySharedCoordinatesChkBox.TabIndex = 6;
		this.m_BySharedCoordinatesChkBox.Text = "Use Shared Coordinates";
		this.m_BySharedCoordinatesChkBox.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(393, 265);
		base.Controls.Add(this.m_BySharedCoordinatesChkBox);
		base.Controls.Add(this.m_FileOpen);
		base.Controls.Add(this.m_FileNameTextBox);
		base.Controls.Add(this.m_FileName);
		base.Controls.Add(this.m_ExportAlignmentChkBox);
		base.Controls.Add(this.m_ExportSurfaceChkBox);
		base.Controls.Add(this.m_ButtonCancel);
		base.Controls.Add(this.m_ButtonOk);
		base.Controls.Add(this.m_ExportInformation);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ExportLandXMLManager";
		this.Text = "ExportToLandXML";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnExportXMLClosing);
		this.m_ExportInformation.ResumeLayout(false);
		this.m_ExportInformation.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_ButtonOk;
		base.CancelButton = this.m_ButtonCancel;
	}
}
