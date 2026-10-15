using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Autodesk.Revit.DB;

namespace EPCommon;

public class LandXMLParser
{
	public const double EPC_GRADS_TO_RADIANS = Math.PI / 200.0;

	private List<LandXMLSurface> m_Surfaces;

	private List<LandXMLAlignment> m_Alignments;

	private string m_strFileName = string.Empty;

	private int m_nSpiralMessageCount;

	private bool m_bHasSpiral;

	private Dictionary<int, CgPoint> m_CgPntList;

	public List<LandXMLSurface> Surfaces => m_Surfaces;

	public List<LandXMLAlignment> Allignments => m_Alignments;

	public string FileName => m_strFileName;

	public bool HasSpiral => m_bHasSpiral;

	public LandXMLParser(string _strFileName)
	{
		m_Surfaces = new List<LandXMLSurface>();
		m_Alignments = new List<LandXMLAlignment>();
		m_strFileName = _strFileName;
		m_CgPntList = null;
	}

	public void ParseLandXMLSurfaces()
	{
		char[] array = new char[1] { ' ' };
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(m_strFileName);
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Units");
		string areaUnit = string.Empty;
		string linearUnit = string.Empty;
		string volumeUnit = string.Empty;
		foreach (XmlNode item in elementsByTagName)
		{
			foreach (XmlAttribute attribute in item.FirstChild.Attributes)
			{
				if (string.Compare(attribute.Name, "areaUnit", ignoreCase: true) == 0)
				{
					areaUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "linearUnit", ignoreCase: true) == 0)
				{
					linearUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "volumeUnit", ignoreCase: true) == 0)
				{
					volumeUnit = attribute.Value;
				}
			}
		}
		XmlNodeList elementsByTagName2 = xmlDocument.GetElementsByTagName("Surface");
		int num = 1;
		foreach (XmlNode item2 in elementsByTagName2)
		{
			LandXMLSurface landXMLSurface = new LandXMLSurface();
			foreach (XmlAttribute attribute2 in item2.Attributes)
			{
				if (string.Compare(attribute2.Name, "name", ignoreCase: true) == 0)
				{
					landXMLSurface.SurfaceName = attribute2.Value;
				}
				else if (string.Compare(attribute2.Name, "desc", ignoreCase: true) == 0)
				{
					landXMLSurface.SurfaceDescription = attribute2.Value;
				}
			}
			XmlNodeList childNodes = item2.ChildNodes;
			foreach (XmlNode item3 in childNodes)
			{
				if (string.Compare(item3.Name, "Definition", ignoreCase: true) != 0)
				{
					continue;
				}
				foreach (XmlAttribute attribute3 in item3.Attributes)
				{
					if (string.Compare(attribute3.Name, "area2DSurf", ignoreCase: true) == 0)
					{
						landXMLSurface.SurfaceArea2D = attribute3.Value;
					}
					else if (string.Compare(attribute3.Name, "area3DSurf", ignoreCase: true) == 0)
					{
						landXMLSurface.SurfaceArea3D = attribute3.Value;
					}
					else if (string.Compare(attribute3.Name, "elevMax", ignoreCase: true) == 0)
					{
						landXMLSurface.MaxElevation = attribute3.Value;
					}
					else if (string.Compare(attribute3.Name, "elevMin", ignoreCase: true) == 0)
					{
						landXMLSurface.MinElevation = attribute3.Value;
					}
				}
				XmlNodeList childNodes2 = item3.ChildNodes;
				foreach (XmlNode item4 in childNodes2)
				{
					if (string.Compare(item4.Name, "Pnts", ignoreCase: true) == 0)
					{
						landXMLSurface.SurfacePointCount += item4.ChildNodes.Count;
					}
				}
			}
			landXMLSurface.LinearUnit = linearUnit;
			landXMLSurface.AreaUnit = areaUnit;
			landXMLSurface.VolumeUnit = volumeUnit;
			if (CheckForDuplicateSurfaces(landXMLSurface))
			{
				landXMLSurface.SurfaceName += num;
				num++;
			}
			landXMLSurface.XMLSurfaceName = landXMLSurface.SurfaceName;
			if (landXMLSurface.SurfacePointCount > 0)
			{
				m_Surfaces.Add(landXMLSurface);
			}
		}
	}

	public void ParseLandXMLSurfacePoints(ref LandXMLSurface _CurrentSurface, ref ProgressBarForm ProgressFrm, int _nTotalPointCount)
	{
		char[] separator = new char[1] { ' ' };
		bool flag = false;
		bool flag2 = true;
		int num = 0;
		Eps_Point eps_Point = null;
		XmlNodeList xmlNodeList = null;
		XmlNode xmlNode = null;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(m_strFileName);
		if (m_CgPntList == null)
		{
			m_CgPntList = ParseCgPnts(xmlDocument, _CurrentSurface.LinearUnit);
		}
		if (ProgressFrm == null)
		{
			ProgressFrm = new ProgressBarForm();
			ProgressFrm.ShowForm();
			ProgressFrm.Refresh();
			ProgressFrm.MinValue = 0;
			ProgressFrm.MaxValue = _nTotalPointCount;
		}
		ProgressFrm.Message = EPShared.LocalRM.GetString("ProgressBarProcessingPointsForSurfaceMessage") + _CurrentSurface.SurfaceName;
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Surface");
		int num2 = 0;
		foreach (XmlNode item in elementsByTagName)
		{
			bool flag3 = false;
			List<Eps_Point> list = new List<Eps_Point>();
			foreach (XmlAttribute attribute in item.Attributes)
			{
				if (string.Compare(attribute.Name, "name", ignoreCase: true) != 0)
				{
					continue;
				}
				if (string.Compare(attribute.Value, _CurrentSurface.XMLSurfaceName, ignoreCase: true) == 0)
				{
					flag3 = true;
				}
				else if (_CurrentSurface.XMLSurfaceName.StartsWith(attribute.Value) && _CurrentSurface.XMLSurfaceName.Length > attribute.Value.Length)
				{
					if (_CurrentSurface.XMLSurfaceName.LastIndexOf(Convert.ToString(num2), attribute.Value.Length) >= 0)
					{
						flag3 = true;
					}
					else
					{
						num2++;
					}
				}
				break;
			}
			if (!flag3)
			{
				continue;
			}
			XmlNodeList childNodes = item.ChildNodes;
			foreach (XmlNode item2 in childNodes)
			{
				if (string.Compare(item2.Name, "Definition", ignoreCase: true) != 0)
				{
					continue;
				}
				XmlNodeList childNodes2 = item2.ChildNodes;
				foreach (XmlNode item3 in childNodes2)
				{
					if (string.Compare(item3.Name, "Pnts", ignoreCase: true) != 0)
					{
						continue;
					}
					xmlNodeList = item3.ChildNodes;
					num = xmlNodeList.Count;
					int num3 = 0;
					for (int i = 0; i < num; i++)
					{
						num3++;
						xmlNode = xmlNodeList[i];
						flag = false;
						eps_Point = new Eps_Point();
						if (xmlNode.Attributes.Count > 0)
						{
							foreach (XmlAttribute attribute2 in xmlNode.Attributes)
							{
								if (string.Compare(attribute2.Name, "pntRef", ignoreCase: true) != 0 || !(attribute2.Value != string.Empty))
								{
									continue;
								}
								flag2 = true;
								for (int j = 0; j < attribute2.Value.Length; j++)
								{
									if (!char.IsDigit(attribute2.Value[j]))
									{
										flag2 = false;
										break;
									}
								}
								if (flag2)
								{
									eps_Point = m_CgPntList[Convert.ToInt32(attribute2.Value)].CgPnt;
								}
								else
								{
									for (int k = 0; k < m_CgPntList.Count; k++)
									{
										if (string.Compare(attribute2.Value, m_CgPntList[k].NameUnit, ignoreCase: true) == 0)
										{
											eps_Point = m_CgPntList[k].CgPnt;
											break;
										}
									}
								}
								flag = true;
							}
						}
						if (!flag)
						{
							int num4 = 1;
							string[] array = xmlNode.InnerText.Split(separator);
							foreach (string strValue in array)
							{
								switch (num4)
								{
								case 1:
									eps_Point.y = ValueConversion.Parse(strValue);
									break;
								case 2:
									eps_Point.x = ValueConversion.Parse(strValue);
									break;
								case 3:
									eps_Point.z = ValueConversion.Parse(strValue);
									break;
								}
								num4++;
							}
						}
						list.Add(eps_Point);
						if (num3 == 100)
						{
							ProgressFrm.UpdateProgressBar(100);
							num3 = 0;
						}
					}
					if (num3 > 0)
					{
						ProgressFrm.UpdateProgressBar(num3);
					}
				}
			}
			List<Eps_Point> _RevitPntList = new List<Eps_Point>();
			ConvertPntsToUnitsPnts(list, ref _RevitPntList, _CurrentSurface.LinearUnit);
			_CurrentSurface.SurfacePoints = _RevitPntList;
			break;
		}
	}

	public void ParseLandXMLAlignments()
	{
		char[] array = new char[1] { ' ' };
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(m_strFileName);
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Units");
		string areaUnit = string.Empty;
		string linearUnit = string.Empty;
		string volumeUnit = string.Empty;
		string angularUnit = string.Empty;
		string empty = string.Empty;
		foreach (XmlNode item in elementsByTagName)
		{
			foreach (XmlAttribute attribute in item.FirstChild.Attributes)
			{
				if (string.Compare(attribute.Name, "areaUnit", ignoreCase: true) == 0)
				{
					areaUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "linearUnit", ignoreCase: true) == 0)
				{
					linearUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "volumeUnit", ignoreCase: true) == 0)
				{
					volumeUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "angularUnit", ignoreCase: true) == 0)
				{
					angularUnit = attribute.Value;
				}
				else if (string.Compare(attribute.Name, "directionUnit", ignoreCase: true) == 0)
				{
					empty = attribute.Value;
				}
			}
		}
		XmlNodeList elementsByTagName2 = xmlDocument.GetElementsByTagName("Alignment");
		int num = 1;
		bool flag = false;
		foreach (XmlNode item2 in elementsByTagName2)
		{
			LandXMLAlignment landXMLAlignment = new LandXMLAlignment();
			foreach (XmlAttribute attribute2 in item2.Attributes)
			{
				if (string.Compare(attribute2.Name, "name", ignoreCase: true) == 0)
				{
					landXMLAlignment.Name = attribute2.Value;
				}
				else if (string.Compare(attribute2.Name, "length", ignoreCase: true) == 0)
				{
					landXMLAlignment.Length = attribute2.Value;
				}
				else if (string.Compare(attribute2.Name, "staStart", ignoreCase: true) == 0)
				{
					landXMLAlignment.StartStation = attribute2.Value;
				}
			}
			XmlNodeList childNodes = item2.ChildNodes;
			foreach (XmlNode item3 in childNodes)
			{
				if (string.Compare(item3.Name, "CoordGeom", ignoreCase: true) != 0)
				{
					continue;
				}
				XmlNodeList childNodes2 = item3.ChildNodes;
				foreach (XmlNode item4 in childNodes2)
				{
					if (string.Compare(item4.Name, "Spiral", ignoreCase: true) == 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				landXMLAlignment.LinearUnit = linearUnit;
				landXMLAlignment.AreaUnit = areaUnit;
				landXMLAlignment.VolumeUnit = volumeUnit;
				landXMLAlignment.AngularUnit = angularUnit;
				if (CheckForDuplicateAlignments(landXMLAlignment))
				{
					landXMLAlignment.Name += num;
					num++;
				}
				m_Alignments.Add(landXMLAlignment);
			}
			else
			{
				if (m_nSpiralMessageCount == 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("SpiralsNotSupportedMessages"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Hand);
					m_nSpiralMessageCount++;
				}
				flag = false;
			}
		}
		if (m_nSpiralMessageCount > 0)
		{
			m_bHasSpiral = true;
		}
	}

	public void ParseLandXMLAlignmentSegments(ref LandXMLAlignment _CurrentAlignment, ref ProgressBarForm ProgressFrm, int _nAlignmentCount)
	{
		char[] separator = new char[1] { ' ' };
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(m_strFileName);
		if (m_CgPntList == null)
		{
			m_CgPntList = ParseCgPnts(xmlDocument, _CurrentAlignment.LinearUnit);
		}
		if (ProgressFrm == null)
		{
			ProgressFrm = new ProgressBarForm();
			ProgressFrm.ShowForm();
			ProgressFrm.Refresh();
			ProgressFrm.MinValue = 0;
			ProgressFrm.MaxValue = _nAlignmentCount;
		}
		ProgressFrm.Message = EPShared.LocalRM.GetString("ProgressBarProcessingPointsForAlignmentMessage") + _CurrentAlignment.Name;
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Alignment");
		bool flag = false;
		bool flag2 = true;
		foreach (XmlNode item in elementsByTagName)
		{
			bool flag3 = false;
			int num = 1;
			foreach (XmlAttribute attribute in item.Attributes)
			{
				if (string.Compare(attribute.Name, "name", ignoreCase: true) != 0)
				{
					continue;
				}
				if (string.Compare(attribute.Value, _CurrentAlignment.Name, ignoreCase: true) == 0)
				{
					flag3 = true;
					break;
				}
				if (_CurrentAlignment.Name.StartsWith(attribute.Value) && _CurrentAlignment.Name.Length > attribute.Value.Length)
				{
					if (_CurrentAlignment.Name.LastIndexOf(Convert.ToString(num), attribute.Value.Length) >= 0)
					{
						flag3 = true;
						break;
					}
					num++;
				}
			}
			if (!flag3)
			{
				continue;
			}
			XmlNodeList childNodes = item.ChildNodes;
			foreach (XmlNode item2 in childNodes)
			{
				if (string.Compare(item2.Name, "Start", ignoreCase: true) == 0)
				{
					bool flag4 = false;
					Eps_Point eps_Point = new Eps_Point();
					if (item2.Attributes.Count > 0)
					{
						foreach (XmlAttribute attribute2 in item2.Attributes)
						{
							if (string.Compare(attribute2.Name, "pntRef", ignoreCase: true) != 0 || !(attribute2.Value != string.Empty))
							{
								continue;
							}
							flag2 = true;
							for (int i = 0; i < attribute2.Value.Length; i++)
							{
								if (!char.IsDigit(attribute2.Value[i]))
								{
									flag2 = false;
									break;
								}
							}
							if (flag2)
							{
								eps_Point = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute2.Value)].CgPnt, _CurrentAlignment.LinearUnit);
							}
							else
							{
								for (int j = 0; j < m_CgPntList.Count; j++)
								{
									if (string.Compare(attribute2.Value, m_CgPntList[j].NameUnit, ignoreCase: true) == 0)
									{
										eps_Point = ConvertPntToUnitsPnt(m_CgPntList[j].CgPnt, _CurrentAlignment.LinearUnit);
										break;
									}
								}
							}
							flag4 = true;
						}
					}
					if (!flag4)
					{
						int num2 = 1;
						string[] array = item2.InnerText.Split(separator);
						foreach (string strValue in array)
						{
							switch (num2)
							{
							case 1:
								eps_Point.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue));
								break;
							case 2:
								eps_Point.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue));
								break;
							case 3:
								eps_Point.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue));
								break;
							}
							num2++;
						}
					}
					_CurrentAlignment.StartPoint = eps_Point;
				}
				else
				{
					if (string.Compare(item2.Name, "CoordGeom", ignoreCase: true) != 0)
					{
						continue;
					}
					XmlNodeList childNodes2 = item2.ChildNodes;
					foreach (XmlNode item3 in childNodes2)
					{
						if (string.Compare(item3.Name, "Spiral", ignoreCase: true) == 0)
						{
							flag = true;
							break;
						}
						if (string.Compare(item3.Name, "Line", ignoreCase: true) == 0)
						{
							Eps_Line eps_Line = new Eps_Line();
							XmlNodeList childNodes3 = item3.ChildNodes;
							foreach (XmlNode item4 in childNodes3)
							{
								if (string.Compare(item4.Name, "start", ignoreCase: true) == 0)
								{
									bool flag5 = false;
									Eps_Point eps_Point2 = new Eps_Point();
									if (item4.Attributes.Count > 0)
									{
										foreach (XmlAttribute attribute3 in item4.Attributes)
										{
											if (string.Compare(attribute3.Name, "pntRef", ignoreCase: true) != 0 || !(attribute3.Value != string.Empty))
											{
												continue;
											}
											flag2 = true;
											for (int l = 0; l < attribute3.Value.Length; l++)
											{
												if (!char.IsDigit(attribute3.Value[l]))
												{
													flag2 = false;
													break;
												}
											}
											if (flag2)
											{
												eps_Point2 = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute3.Value)].CgPnt, _CurrentAlignment.LinearUnit);
											}
											else
											{
												for (int m = 0; m < m_CgPntList.Count; m++)
												{
													if (string.Compare(attribute3.Value, m_CgPntList[m].NameUnit, ignoreCase: true) == 0)
													{
														eps_Point2 = ConvertPntToUnitsPnt(m_CgPntList[m].CgPnt, _CurrentAlignment.LinearUnit);
														break;
													}
												}
											}
											flag5 = true;
										}
									}
									if (!flag5)
									{
										int num3 = 1;
										string[] array2 = item4.InnerText.Split(separator);
										foreach (string strValue2 in array2)
										{
											switch (num3)
											{
											case 1:
												eps_Point2.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue2));
												break;
											case 2:
												eps_Point2.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue2));
												break;
											case 3:
												eps_Point2.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue2));
												break;
											}
											num3++;
										}
									}
									eps_Line.StartPnt = eps_Point2;
								}
								else
								{
									if (string.Compare(item4.Name, "end", ignoreCase: true) != 0)
									{
										continue;
									}
									bool flag6 = false;
									Eps_Point eps_Point3 = new Eps_Point();
									if (item4.Attributes.Count > 0)
									{
										foreach (XmlAttribute attribute4 in item4.Attributes)
										{
											if (string.Compare(attribute4.Name, "pntRef", ignoreCase: true) != 0 || !(attribute4.Value != string.Empty))
											{
												continue;
											}
											flag2 = true;
											for (int num4 = 0; num4 < attribute4.Value.Length; num4++)
											{
												if (!char.IsDigit(attribute4.Value[num4]))
												{
													flag2 = false;
													break;
												}
											}
											if (flag2)
											{
												eps_Point3 = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute4.Value)].CgPnt, _CurrentAlignment.LinearUnit);
											}
											else
											{
												for (int num5 = 0; num5 < m_CgPntList.Count; num5++)
												{
													if (string.Compare(attribute4.Value, m_CgPntList[num5].NameUnit, ignoreCase: true) == 0)
													{
														eps_Point3 = ConvertPntToUnitsPnt(m_CgPntList[num5].CgPnt, _CurrentAlignment.LinearUnit);
														break;
													}
												}
											}
											flag6 = true;
										}
									}
									if (!flag6)
									{
										int num6 = 1;
										string[] array3 = item4.InnerText.Split(separator);
										foreach (string strValue3 in array3)
										{
											switch (num6)
											{
											case 1:
												eps_Point3.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue3));
												break;
											case 2:
												eps_Point3.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue3));
												break;
											case 3:
												eps_Point3.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue3));
												break;
											}
											num6++;
										}
									}
									eps_Line.EndPnt = eps_Point3;
								}
							}
							_CurrentAlignment.Lines.Add(eps_Line);
						}
						else
						{
							if (string.Compare(item3.Name, "Curve", ignoreCase: true) != 0)
							{
								continue;
							}
							Eps_Arc eps_Arc = new Eps_Arc();
							foreach (XmlAttribute attribute5 in item3.Attributes)
							{
								if (string.Compare(attribute5.Name, "rot", ignoreCase: true) == 0)
								{
									if (string.Compare(attribute5.Value, "cw", ignoreCase: true) == 0)
									{
										eps_Arc.Direction = -1;
									}
									else
									{
										eps_Arc.Direction = 1;
									}
								}
								else if (string.Compare(attribute5.Name, "delta", ignoreCase: true) == 0)
								{
									if (attribute5.Value != string.Empty)
									{
										if (_CurrentAlignment.AngularUnit == string.Empty)
										{
											_CurrentAlignment.AngularUnit = "radians";
										}
										eps_Arc.Delta = ConvertToRevitAngularUnits(_CurrentAlignment.AngularUnit, ValueConversion.ToDouble(attribute5.Value));
									}
								}
								else if (string.Compare(attribute5.Name, "radius", ignoreCase: true) == 0 && attribute5.Value != string.Empty)
								{
									eps_Arc.Radius = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.ToDouble(attribute5.Value));
								}
							}
							XmlNodeList childNodes4 = item3.ChildNodes;
							foreach (XmlNode item5 in childNodes4)
							{
								if (string.Compare(item5.Name, "start", ignoreCase: true) == 0)
								{
									bool flag7 = false;
									Eps_Point eps_Point4 = new Eps_Point();
									if (item5.Attributes.Count > 0)
									{
										foreach (XmlAttribute attribute6 in item5.Attributes)
										{
											if (string.Compare(attribute6.Name, "pntRef", ignoreCase: true) != 0 || !(attribute6.Value != string.Empty))
											{
												continue;
											}
											flag2 = true;
											for (int num8 = 0; num8 < attribute6.Value.Length; num8++)
											{
												if (!char.IsDigit(attribute6.Value[num8]))
												{
													flag2 = false;
													break;
												}
											}
											if (flag2)
											{
												eps_Point4 = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute6.Value)].CgPnt, _CurrentAlignment.LinearUnit);
											}
											else
											{
												for (int num9 = 0; num9 < m_CgPntList.Count; num9++)
												{
													if (string.Compare(attribute6.Value, m_CgPntList[num9].NameUnit, ignoreCase: true) == 0)
													{
														eps_Point4 = ConvertPntToUnitsPnt(m_CgPntList[num9].CgPnt, _CurrentAlignment.LinearUnit);
														break;
													}
												}
											}
											flag7 = true;
										}
									}
									if (!flag7)
									{
										int num10 = 1;
										string[] array4 = item5.InnerText.Split(separator);
										foreach (string strValue4 in array4)
										{
											switch (num10)
											{
											case 1:
												eps_Point4.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue4));
												break;
											case 2:
												eps_Point4.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue4));
												break;
											case 3:
												eps_Point4.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue4));
												break;
											}
											num10++;
										}
									}
									eps_Arc.StartPnt = eps_Point4;
								}
								else if (string.Compare(item5.Name, "end", ignoreCase: true) == 0)
								{
									bool flag8 = false;
									Eps_Point eps_Point5 = new Eps_Point();
									if (item5.Attributes.Count > 0)
									{
										foreach (XmlAttribute attribute7 in item5.Attributes)
										{
											if (string.Compare(attribute7.Name, "pntRef", ignoreCase: true) != 0 || !(attribute7.Value != string.Empty))
											{
												continue;
											}
											flag2 = true;
											for (int num12 = 0; num12 < attribute7.Value.Length; num12++)
											{
												if (!char.IsDigit(attribute7.Value[num12]))
												{
													flag2 = false;
													break;
												}
											}
											if (flag2)
											{
												eps_Point5 = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute7.Value)].CgPnt, _CurrentAlignment.LinearUnit);
											}
											else
											{
												for (int num13 = 0; num13 < m_CgPntList.Count; num13++)
												{
													if (string.Compare(attribute7.Value, m_CgPntList[num13].NameUnit, ignoreCase: true) == 0)
													{
														eps_Point5 = ConvertPntToUnitsPnt(m_CgPntList[num13].CgPnt, _CurrentAlignment.LinearUnit);
														break;
													}
												}
											}
											flag8 = true;
										}
									}
									if (!flag8)
									{
										int num14 = 1;
										string[] array5 = item5.InnerText.Split(separator);
										foreach (string strValue5 in array5)
										{
											switch (num14)
											{
											case 1:
												eps_Point5.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue5));
												break;
											case 2:
												eps_Point5.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue5));
												break;
											case 3:
												eps_Point5.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue5));
												break;
											}
											num14++;
										}
									}
									eps_Arc.EndPnt = eps_Point5;
								}
								else
								{
									if (string.Compare(item5.Name, "center", ignoreCase: true) != 0)
									{
										continue;
									}
									bool flag9 = false;
									Eps_Point eps_Point6 = new Eps_Point();
									if (item5.Attributes.Count > 0)
									{
										foreach (XmlAttribute attribute8 in item5.Attributes)
										{
											if (string.Compare(attribute8.Name, "pntRef", ignoreCase: true) != 0 || !(attribute8.Value != string.Empty))
											{
												continue;
											}
											flag2 = true;
											for (int num16 = 0; num16 < attribute8.Value.Length; num16++)
											{
												if (!char.IsDigit(attribute8.Value[num16]))
												{
													flag2 = false;
													break;
												}
											}
											if (flag2)
											{
												eps_Point6 = ConvertPntToUnitsPnt(m_CgPntList[Convert.ToInt32(attribute8.Value)].CgPnt, _CurrentAlignment.LinearUnit);
											}
											else
											{
												for (int num17 = 0; num17 < m_CgPntList.Count; num17++)
												{
													if (string.Compare(attribute8.Value, m_CgPntList[num17].NameUnit, ignoreCase: true) == 0)
													{
														eps_Point6 = ConvertPntToUnitsPnt(m_CgPntList[num17].CgPnt, _CurrentAlignment.LinearUnit);
														break;
													}
												}
											}
											flag9 = true;
										}
									}
									if (!flag9)
									{
										int num18 = 1;
										string[] array6 = item5.InnerText.Split(separator);
										foreach (string strValue6 in array6)
										{
											switch (num18)
											{
											case 1:
												eps_Point6.y = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue6));
												break;
											case 2:
												eps_Point6.x = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue6));
												break;
											case 3:
												eps_Point6.z = ConvertToRevitUnits(_CurrentAlignment.LinearUnit, ValueConversion.Parse(strValue6));
												break;
											}
											num18++;
										}
									}
									eps_Arc.RadiusPnt = eps_Point6;
								}
							}
							_CurrentAlignment.Curves.Add(eps_Arc);
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				if (m_nSpiralMessageCount == 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("SpiralsNotSupportedMessages"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Hand);
					m_nSpiralMessageCount++;
				}
				flag = false;
			}
			break;
		}
		if (m_nSpiralMessageCount > 0)
		{
			m_bHasSpiral = true;
		}
		ProgressFrm.UpdateProgressBar(1);
	}

	private Dictionary<int, CgPoint> ParseCgPnts(XmlDocument _CurrentXMLDocument, string strLinearUnit)
	{
		char[] separator = new char[1] { ' ' };
		Dictionary<int, CgPoint> dictionary = new Dictionary<int, CgPoint>();
		XmlNodeList elementsByTagName = _CurrentXMLDocument.GetElementsByTagName("CgPoints");
		foreach (XmlNode item in elementsByTagName)
		{
			XmlNodeList childNodes = item.ChildNodes;
			ProgressBarForm progressBarForm = new ProgressBarForm();
			progressBarForm.ShowForm();
			progressBarForm.Refresh();
			progressBarForm.Message = EPShared.LocalRM.GetString("ProgressBarProcessingCogoPointsMessage");
			progressBarForm.MinValue = 0;
			progressBarForm.MaxValue = childNodes.Count;
			int i = 0;
			int num = 0;
			string empty = string.Empty;
			string empty2 = string.Empty;
			string empty3 = string.Empty;
			bool flag = false;
			bool flag2 = true;
			CgPoint cgPoint = null;
			int num2 = 0;
			foreach (XmlNode item2 in childNodes)
			{
				empty = string.Empty;
				empty2 = string.Empty;
				empty3 = string.Empty;
				flag = false;
				cgPoint = null;
				foreach (XmlAttribute attribute in item2.Attributes)
				{
					if (string.Compare(attribute.Name, "name", ignoreCase: true) == 0)
					{
						empty3 = attribute.Value;
						flag2 = true;
						for (int j = 0; j < empty3.Length; j++)
						{
							if (!char.IsDigit(empty3[j]))
							{
								flag2 = false;
								break;
							}
						}
						i = ((!flag2) ? (i + 1) : Convert.ToInt32(empty3));
					}
					else if (string.Compare(attribute.Name, "code", ignoreCase: true) == 0)
					{
						empty = attribute.Value;
					}
					else if (string.Compare(attribute.Name, "desc", ignoreCase: true) == 0)
					{
						empty2 = attribute.Value;
					}
					else
					{
						if (string.Compare(attribute.Name, "PntRef", ignoreCase: true) != 0)
						{
							continue;
						}
						flag2 = true;
						for (int k = 0; k < attribute.Value.Length; k++)
						{
							if (!char.IsDigit(attribute.Value[k]))
							{
								flag2 = false;
								break;
							}
						}
						num = ((!flag2) ? (num + 1) : Convert.ToInt32(attribute.Value));
						flag = true;
					}
				}
				if (flag && dictionary.ContainsKey(num))
				{
					continue;
				}
				Eps_Point eps_Point = new Eps_Point();
				int num3 = 1;
				string[] array = item2.InnerText.Split(separator);
				foreach (string strValue in array)
				{
					switch (num3)
					{
					case 1:
						eps_Point.y = ValueConversion.Parse(strValue);
						break;
					case 2:
						eps_Point.x = ValueConversion.Parse(strValue);
						break;
					case 3:
						eps_Point.z = ValueConversion.Parse(strValue);
						break;
					}
					num3++;
				}
				cgPoint = new CgPoint(i, empty, empty2, empty3, eps_Point);
				if (i >= 0)
				{
					for (; dictionary.ContainsKey(i); i++)
					{
					}
					dictionary.Add(i, cgPoint);
				}
				num2++;
				if (num2 == 100)
				{
					progressBarForm.UpdateProgressBar(100);
					num2 = 0;
				}
			}
			progressBarForm.SetMaxValue();
			Thread.Sleep(300);
			progressBarForm.Close();
		}
		return dictionary;
	}

	private bool CheckForDuplicateAlignments(LandXMLAlignment _SelectedAlignment)
	{
		foreach (LandXMLAlignment alignment in m_Alignments)
		{
			if (string.Compare(_SelectedAlignment.Name, alignment.Name, ignoreCase: false) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckForDuplicateSurfaces(LandXMLSurface _SelectedSurface)
	{
		foreach (LandXMLSurface surface in m_Surfaces)
		{
			if (string.Compare(_SelectedSurface.SurfaceName, surface.SurfaceName, ignoreCase: false) == 0)
			{
				return true;
			}
		}
		return false;
	}

	private double ConvertToRevitUnits(string _strLandXMLUnit, double _dValue)
	{
		DisplayUnitType inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_FEET;
		if (string.Compare(_strLandXMLUnit, EPShared.LocalRM.GetString("LinearUnitfootString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLandXMLUnit, EPShared.LocalRM.GetString("LinearUnitfeetString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLandXMLUnit, EPShared.LocalRM.GetString("LinearUnitFootString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLandXMLUnit, EPShared.LocalRM.GetString("LinearUnitFeetString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLandXMLUnit, "USSurveyFoot", ignoreCase: true) == 0)
		{
			return _dValue;
		}
		if (string.Compare(_strLandXMLUnit, "inch", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_INCHES;
		}
		else
		{
			if (string.Compare(_strLandXMLUnit, "mile", ignoreCase: true) == 0)
			{
				return 5280.0 * _dValue;
			}
			if (string.Compare(_strLandXMLUnit, "millimeter", ignoreCase: true) == 0)
			{
				inputLinearDistanceType = DisplayUnitType.DUT_MILLIMETERS;
			}
			else if (string.Compare(_strLandXMLUnit, "centimeter", ignoreCase: true) == 0)
			{
				inputLinearDistanceType = DisplayUnitType.DUT_CENTIMETERS;
			}
			else if (string.Compare(_strLandXMLUnit, "meter", ignoreCase: true) == 0)
			{
				inputLinearDistanceType = DisplayUnitType.DUT_METERS;
			}
			else if (string.Compare(_strLandXMLUnit, "kilometer", ignoreCase: true) == 0)
			{
				return 3280.8399 * _dValue;
			}
		}
		return RevitGeometry.ConvertLinearValue(_dValue, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
	}

	private double ConvertToRevitAngularUnits(string _strLandXMLUnit, double _dValue)
	{
		double result = 0.0;
		if (string.Compare(_strLandXMLUnit, "radians", ignoreCase: true) == 0)
		{
			result = _dValue;
		}
		else if (string.Compare(_strLandXMLUnit, "grads", ignoreCase: true) == 0)
		{
			result = Math.PI / 200.0 * _dValue;
		}
		else if (string.Compare(_strLandXMLUnit, "decimal degrees", ignoreCase: true) == 0)
		{
			result = DecimalDegreesToRadians(_dValue);
		}
		else if (string.Compare(_strLandXMLUnit, "decimal dd.mm.ss", ignoreCase: true) == 0)
		{
			result = ConvertDMSToRadians(_dValue.ToString("000.0000"));
		}
		return result;
	}

	private void ConvertPntsToUnitsPnts(List<Eps_Point> _AllPnts, ref List<Eps_Point> _RevitPntList, string _strLinearUnit)
	{
		_RevitPntList.Clear();
		DisplayUnitType inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_FEET;
		bool flag = false;
		if (string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitfootString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitfeetString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitFootString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitFeetString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, "USSurveyFoot", ignoreCase: true) == 0)
		{
			flag = true;
		}
		else if (string.Compare(_strLinearUnit, "inch", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_INCHES;
		}
		else if (string.Compare(_strLinearUnit, "millimeter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_MILLIMETERS;
		}
		else if (string.Compare(_strLinearUnit, "centimeter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_CENTIMETERS;
		}
		else if (string.Compare(_strLinearUnit, "meter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_METERS;
		}
		foreach (Eps_Point _AllPnt in _AllPnts)
		{
			Eps_Point eps_Point = null;
			if (flag)
			{
				eps_Point = new Eps_Point(_AllPnt.x, _AllPnt.y, _AllPnt.z);
			}
			else if (string.Compare(_strLinearUnit, "mile", ignoreCase: true) == 0)
			{
				double dX = _AllPnt.x * 5280.0;
				double dY = _AllPnt.y * 5280.0;
				double dZ = _AllPnt.z * 5280.0;
				eps_Point = new Eps_Point(dX, dY, dZ);
			}
			else if (string.Compare(_strLinearUnit, "kilometer", ignoreCase: true) == 0)
			{
				double dX2 = _AllPnt.x * 3280.8399;
				double dY2 = _AllPnt.y * 3280.8399;
				double dZ2 = _AllPnt.z * 3280.8399;
				eps_Point = new Eps_Point(dX2, dY2, dZ2);
			}
			else
			{
				double dX3 = RevitGeometry.ConvertLinearValue(_AllPnt.x, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
				double dY3 = RevitGeometry.ConvertLinearValue(_AllPnt.y, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
				double dZ3 = RevitGeometry.ConvertLinearValue(_AllPnt.z, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
				eps_Point = new Eps_Point(dX3, dY3, dZ3);
			}
			_RevitPntList.Add(eps_Point);
		}
	}

	private Eps_Point ConvertPntToUnitsPnt(Eps_Point CurPoint, string _strLinearUnit)
	{
		DisplayUnitType inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_FEET;
		bool flag = false;
		if (string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitfootString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitfeetString_Lower"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitFootString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, EPShared.LocalRM.GetString("LinearUnitFeetString_Upper"), ignoreCase: true) == 0 || string.Compare(_strLinearUnit, "USSurveyFoot", ignoreCase: true) == 0)
		{
			flag = true;
		}
		else if (string.Compare(_strLinearUnit, "inch", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_DECIMAL_INCHES;
		}
		else if (string.Compare(_strLinearUnit, "millimeter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_MILLIMETERS;
		}
		else if (string.Compare(_strLinearUnit, "centimeter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_CENTIMETERS;
		}
		else if (string.Compare(_strLinearUnit, "meter", ignoreCase: true) == 0)
		{
			inputLinearDistanceType = DisplayUnitType.DUT_METERS;
		}
		Eps_Point eps_Point = null;
		if (flag)
		{
			return new Eps_Point(CurPoint.x, CurPoint.y, CurPoint.z);
		}
		if (string.Compare(_strLinearUnit, "mile", ignoreCase: true) == 0)
		{
			double dX = CurPoint.x * 5280.0;
			double dY = CurPoint.y * 5280.0;
			double dZ = CurPoint.z * 5280.0;
			return new Eps_Point(dX, dY, dZ);
		}
		if (string.Compare(_strLinearUnit, "kilometer", ignoreCase: true) == 0)
		{
			double dX2 = CurPoint.x * 3280.8399;
			double dY2 = CurPoint.y * 3280.8399;
			double dZ2 = CurPoint.z * 3280.8399;
			return new Eps_Point(dX2, dY2, dZ2);
		}
		double dX3 = RevitGeometry.ConvertLinearValue(CurPoint.x, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
		double dY3 = RevitGeometry.ConvertLinearValue(CurPoint.y, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
		double dZ3 = RevitGeometry.ConvertLinearValue(CurPoint.z, inputLinearDistanceType, DisplayUnitType.DUT_DECIMAL_FEET);
		return new Eps_Point(dX3, dY3, dZ3);
	}

	public static double DecimalDegreesToRadians(double _dDecimalDegrees)
	{
		return _dDecimalDegrees * (Math.PI / 180.0);
	}

	private double ConvertDMSToRadians(string _strDMS)
	{
		double _dRadians = 0.0;
		int _nDegree = 0;
		int _nMin = 0;
		double _dSec = 0.0;
		SeperateDMS(_strDMS, ref _nDegree, ref _nMin, ref _dSec);
		if (_nDegree >= 0)
		{
			DMSToRadians(ref _dRadians, 1, _nDegree, _nMin, _dSec);
		}
		else
		{
			DMSToRadians(ref _dRadians, -1, _nDegree, _nMin, _dSec);
		}
		return _dRadians;
	}

	private void SeperateDMS(string _strDMS, ref int _nDegree, ref int _nMin, ref double _dSec)
	{
		string value = _strDMS.Substring(0, 3);
		string value2 = _strDMS.Substring(4, 2);
		string strValue = _strDMS.Substring(6, 2);
		_nDegree = Convert.ToInt32(value);
		_nMin = Convert.ToInt32(value2);
		_dSec = ValueConversion.ToDouble(strValue);
	}

	public static void DMSToRadians(ref double _dRadians, int _nSign, int _nDegrees, int _nMinutes, double _dSeconds)
	{
		_dRadians = (double)_nSign * (((double)Math.Abs(_nDegrees) + (double)Math.Abs(_nMinutes) / 60.0 + Math.Abs(_dSeconds) / 3600.0) * (Math.PI / 180.0));
	}
}
