using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using SITES.Utilities;

namespace EPCommon;

public class FamilyManagerForm : EPForm
{
	private Document m_Document;

	private List<FamilyBinder> m_FamilyList;

	private EPTransactionContainer m_TransactionContainer;

	private LandcaddID m_CurrentFamilyType = LandcaddID.Street;

	private int m_nMixtListViewSortColumn = -1;

	private List<string> m_OpenedFileList = new List<string>();

	private IContainer components;

	private GroupBox m_FamilyManagerGrpBox;

	private ListView m_FamilyManagerListView;

	private Button m_CopyButton;

	private Button m_ModifyButton;

	private Button m_AddButton;

	private Button m_CloseButton;

	private Button m_SaveButton;

	private Label m_FamilyManagerLbl;

	private Button m_LoadButton;

	public Document Document => m_Document;

	public ReadOnlyCollection<FamilyBinder> FamilyList => new ReadOnlyCollection<FamilyBinder>(m_FamilyList);

	public EPTransactionContainer TransactionContainer => m_TransactionContainer;

	public FamilyManagerForm(Document _CurDocument, LandcaddID _CurrType)
	{
		InitializeComponent();
		m_CurrentFamilyType = _CurrType;
		SetStringValues();
		m_Document = _CurDocument;
		m_TransactionContainer = new EPTransactionContainer(m_Document);
		m_FamilyList = new List<FamilyBinder>();
		PopulateFamilyAndSymbolLists(_bCheckForFamily: true);
		UpdateListView();
		GetDialogPosition();
	}

	private void UpdateListView()
	{
		m_FamilyManagerListView.Items.Clear();
		foreach (FamilyBinder family in m_FamilyList)
		{
			ListViewItem listViewItem = new ListViewItem();
			listViewItem = m_FamilyManagerListView.Items.Add(family.FamilyName);
			listViewItem.Tag = family.FamilyName;
			listViewItem.SubItems.Add(family.FamilyPath);
		}
		if (m_FamilyManagerListView.Items.Count > 0)
		{
			m_FamilyManagerListView.Items[0].Selected = true;
		}
	}

	private void PopulateFamilyAndSymbolLists(bool _bCheckForFamily)
	{
		int num = 0;
		Document document = null;
		string strCurrentFamilyType = Convert.ToInt32(m_CurrentFamilyType).ToString("000");
		m_FamilyList.Clear();
		foreach (Document document2 in m_Document.Application.Documents)
		{
			m_OpenedFileList.Add(document2.PathName);
		}
		Parameter CurLanddCadID = null;
		FamilySymbol familySymbol = null;
		FilteredElementCollector filteredElementCollector = new FilteredElementCollector(m_Document);
		filteredElementCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Site);
		IEnumerable<Element> source = filteredElementCollector.Where((Element CurrentElement) => (CurLanddCadID = CurrentElement.LookupParameter(RevitParameters.SiteworksIDFamilyParameter)) != null && string.Compare(CurLanddCadID.AsString(), strCurrentFamilyType, ignoreCase: true) == 0);
		List<Element> list = source.ToList();
		familySymbol = null;
		foreach (Element item in list)
		{
			if (!(item is FamilySymbol { Category: not null } familySymbol2))
			{
				continue;
			}
			CurLanddCadID = familySymbol2.LookupParameter(RevitParameters.SiteworksIDFamilyParameter);
			if (CurLanddCadID == null || string.Compare(CurLanddCadID.AsString(), Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) != 0)
			{
				continue;
			}
			bool flag = false;
			for (num = 0; num < m_FamilyList.Count; num++)
			{
				if (string.Compare(m_FamilyList[num].FamilyName, familySymbol2.Family.Name, ignoreCase: true) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				FamilyBinder familyBinder = null;
				string empty = string.Empty;
				document = m_Document.EditFamily(familySymbol2.Family);
				if (document != null)
				{
					empty = document.PathName;
					familyBinder = new FamilyBinder(familySymbol2.Family.Name, empty, null, familySymbol2.Family);
					familyBinder.FamilyTypeList = PopulateFamilyTypeSetList(document, familySymbol2.Family.Name);
					familyBinder.FamilyDocument = document;
					familyBinder.Loaded = true;
					m_FamilyList.Add(familyBinder);
				}
			}
		}
	}

	private int CheckForDuplicates(FamilyBinder _Binder)
	{
		if (_Binder == null)
		{
			return -1;
		}
		FamilyBinder familyBinder = null;
		for (int i = 0; i < m_FamilyList.Count; i++)
		{
			familyBinder = m_FamilyList[i];
			if (string.Compare(familyBinder.FamilyName, _Binder.FamilyName, ignoreCase: false) == 0 && string.Compare(familyBinder.FamilyPath, _Binder.FamilyPath, ignoreCase: false) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	private void CheckForDuplicates(DialogResult _CurrentResult)
	{
		if (_CurrentResult != DialogResult.OK)
		{
			return;
		}
		FamilyBinder familyBinder = m_FamilyList[m_FamilyList.Count - 1];
		string familyName = familyBinder.FamilyName;
		string familyPath = familyBinder.FamilyPath;
		bool flag = false;
		for (int i = 0; i < m_FamilyManagerListView.Items.Count - 1; i++)
		{
			if (string.Compare(m_FamilyManagerListView.Items[i].SubItems[0].Text, familyName, ignoreCase: false) == 0 && string.Compare(m_FamilyManagerListView.Items[i].SubItems[1].Text, familyPath, ignoreCase: false) == 0)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateFamilyInFamilyManagerMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxIcon.Exclamation);
			m_FamilyList.RemoveAt(m_FamilyList.Count - 1);
			UpdateListView();
		}
	}

	private void SetStringValues()
	{
		m_LoadButton.Text = EPShared.LocalRM.GetString("AMP_LoadDialog");
		m_AddButton.Text = EPShared.LocalRM.GetString("AMP_New_NewFamilyDialog");
		m_ModifyButton.Text = EPShared.LocalRM.GetString("AMP_ModifyDialog");
		m_CopyButton.Text = EPShared.LocalRM.GetString("AMP_CopyDialog");
		string empty = string.Empty;
		string text = string.Empty;
		empty = EPShared.LocalRM.GetString("FamilyManagerRibbonPanel");
		switch (m_CurrentFamilyType)
		{
		case LandcaddID.PlantMix:
			text = ApplicationUtility.GetString("");
			break;
		case LandcaddID.PatioWalk:
			text = ApplicationUtility.GetString("");
			break;
		case LandcaddID.Street:
			text = EPShared.LocalRM.GetString("StreetTypeManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblStreetText");
			ContextualHelpID = "HID_SDfamilyMgrStreet";
			break;
		case LandcaddID.Curb:
			text = EPShared.LocalRM.GetString("CurbTypeManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblCurbText");
			ContextualHelpID = "HID_SDfamilyMgrCurb";
			break;
		case LandcaddID.Sidewalk:
			text = EPShared.LocalRM.GetString("SidewalkTypeManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblSidewalkText");
			ContextualHelpID = "HID_SDfamilyMgrSidewalk";
			break;
		case LandcaddID.RetainingWall:
			text = EPShared.LocalRM.GetString("RetainingWallManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblRetainingWallText");
			ContextualHelpID = "HID_SDfamilyMgrRetainingWall";
			break;
		case LandcaddID.Steps:
			text = EPShared.LocalRM.GetString("StepsManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblStepsText");
			break;
		case LandcaddID.ParkingLot:
			text = EPShared.LocalRM.GetString("ParkingLotManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblParkingLotText");
			ContextualHelpID = "HID_SDfamilyMgrParkinglot";
			break;
		case LandcaddID.SoftTerrain:
			text = EPShared.LocalRM.GetString("SoftTerrainManagerString");
			m_FamilyManagerLbl.Text = EPShared.LocalRM.GetString("FamilyManagerLblSoftTerrainText");
			ContextualHelpID = "HID_SDfamilyMgrSoftTerrain";
			break;
		}
		Text = text + EPShared.LocalRM.GetString("_FamilyManagerString");
		m_FamilyManagerGrpBox.Text = text + EPShared.LocalRM.GetString("_FamiliesInModelString");
		List<string> list = new List<string>();
		list.Add(EPShared.LocalRM.GetString("FamilyString"));
		list.Add(EPShared.LocalRM.GetString("Path"));
		m_FamilyManagerListView.View = System.Windows.Forms.View.Details;
		m_FamilyManagerListView.MultiSelect = false;
		for (int i = 0; i < list.Count; i++)
		{
			m_FamilyManagerListView.Columns.Add(list[i], (m_FamilyManagerListView.Width - 6) / 2, HorizontalAlignment.Center);
		}
		m_FamilyManagerListView.Columns[1].TextAlign = HorizontalAlignment.Left;
	}

	private void OnFamilyManagerModifyClicked(object sender, EventArgs e)
	{
		Modify();
	}

	private void Modify()
	{
		int _nSelectedIndex = -1;
		List<FamilyBinder> list = new List<FamilyBinder>();
		FindSelectedFamilyIndex(ref _nSelectedIndex);
		Document CurDocument = null;
		if (_nSelectedIndex < 0)
		{
			return;
		}
		FamilyBinder SelectedFamily = m_FamilyList[_nSelectedIndex];
		try
		{
			if (SelectedFamily.FamilyDocument == null)
			{
				CurDocument = m_Document.Application.OpenDocumentFile(SelectedFamily.FamilyPath);
			}
			else if (File.Exists(SelectedFamily.FamilyDocument.PathName))
			{
				if (!ManagePlants.IsDocumentOpen(SelectedFamily.FamilyDocument.Application, SelectedFamily.FamilyPath, ref CurDocument))
				{
					CurDocument = m_Document.Application.OpenDocumentFile(SelectedFamily.FamilyPath);
				}
			}
			else
			{
				CurDocument = SelectedFamily.FamilyDocument;
			}
		}
		catch (Exception)
		{
			if (!ManagePlants.IsDocumentOpen(SelectedFamily.FamilyDocument.Application, SelectedFamily.FamilyPath, ref CurDocument))
			{
				CurDocument = m_Document.Application.OpenDocumentFile(SelectedFamily.FamilyPath);
			}
		}
		TypeManagerForm typeManagerForm = new TypeManagerForm(ref SelectedFamily, CurDocument, m_CurrentFamilyType, m_Document);
		if (typeManagerForm.ShowDialog() == DialogResult.OK)
		{
			m_FamilyList[_nSelectedIndex] = SelectedFamily;
		}
		m_FamilyManagerListView.Items[_nSelectedIndex].Selected = true;
	}

	private void OnFamilyManagerAddClicked(object sender, EventArgs e)
	{
		AddFamily();
	}

	private void AddFamily()
	{
		FamilyBinder _NewFamily = new FamilyBinder();
		Document _FamilyDocument = null;
		List<FamilyTypeContainer> list = new List<FamilyTypeContainer>();
		NewFamilyForm newFamilyForm = new NewFamilyForm(ref _NewFamily, m_Document, m_CurrentFamilyType);
		newFamilyForm.ShowDialog();
		if (newFamilyForm.DialogResult != DialogResult.OK)
		{
			return;
		}
		if (CheckForDuplicates(_NewFamily) >= 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateFamilyInFamilyManagerMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
		else
		{
			if (!_NewFamily.IsModified)
			{
				return;
			}
			_NewFamily.IsModified = true;
			if (ManageFamily.CreateNewFamilyDocument(m_Document, _NewFamily.FamilyTypeList, m_CurrentFamilyType, _NewFamily.FamilyPath, ref _FamilyDocument))
			{
				_NewFamily.FamilyDocument = _FamilyDocument;
				if (_FamilyDocument != null)
				{
					_NewFamily.FamilyTypeList = PopulateFamilyTypeSetList(_FamilyDocument, _NewFamily.FamilyName);
				}
				m_FamilyList.Add(_NewFamily);
				UpdateListView();
				if (m_FamilyManagerListView.Items.Count > 0)
				{
					m_FamilyManagerListView.Items[m_FamilyManagerListView.Items.Count - 1].Selected = true;
					Modify();
				}
			}
			else
			{
				string strText = EPShared.LocalRM.GetString("UnableCreateFamilyString");
				RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			}
		}
	}

	private bool FindSelectedFamilyIndex(ref int _nSelectedIndex)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		_nSelectedIndex = num3;
		if ((num = m_FamilyManagerListView.SelectedItems.Count) >= 1)
		{
			for (num2 = 0; num2 < num; num2++)
			{
				for (num3 = 0; num3 < m_FamilyList.Count; num3++)
				{
					if (string.Compare((string)m_FamilyManagerListView.SelectedItems[num2].Tag, m_FamilyList[num3].FamilyName, ignoreCase: true) == 0)
					{
						_nSelectedIndex = num3;
					}
				}
			}
			return true;
		}
		string empty = string.Empty;
		empty = EPShared.LocalRM.GetString("NoFamilySelectedMessage");
		RevitMessageBox.Show(empty, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		return false;
	}

	private List<FamilyTypeContainer> PopulateFamilyTypeSetList(Document _FamilyDocument, string _strFamilyName)
	{
		EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(_FamilyDocument);
		ePTransactionContainer.Start(EPShared.LocalRM.GetString("GetFamilyTypesTransaction"));
		ePTransactionContainer.BeginTransaction();
		FamilyManager familyManager = _FamilyDocument.FamilyManager;
		List<FamilyTypeContainer> list = null;
		string empty = string.Empty;
		string empty2 = string.Empty;
		switch (m_CurrentFamilyType)
		{
		case LandcaddID.PlantMix:
		{
			List<FamilyParameterTypeWrapper> list2 = ManagePlants.PlantMixParamenterList();
			break;
		}
		case LandcaddID.PatioWalk:
		{
			List<FamilyParameterTypeWrapper> list2 = ManagePlants.PatioWalkParamenterList();
			break;
		}
		case LandcaddID.Street:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.StreetParamenterList();
			break;
		}
		case LandcaddID.Curb:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.CurbParameterList();
			break;
		}
		case LandcaddID.Sidewalk:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.SideWalkParamterList();
			break;
		}
		case LandcaddID.RetainingWall:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.RetainingWallParameterList();
			break;
		}
		case LandcaddID.Steps:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.StepsParameterList();
			break;
		}
		case LandcaddID.ParkingLot:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.ParkingLotParameterList();
			break;
		}
		case LandcaddID.SoftTerrain:
		{
			List<FamilyParameterTypeWrapper> list2 = ManageFamily.SoftTerrainParameterList();
			break;
		}
		}
		ProgressBarForm progressBarForm = new ProgressBarForm();
		progressBarForm.MinValue = 0;
		progressBarForm.MaxValue = familyManager.Types.Size;
		progressBarForm.Message = EPShared.LocalRM.GetString("ProgressbarLoadingFamilyMessage");
		progressBarForm.ShowForm();
		progressBarForm.Refresh();
		foreach (FamilyType type in familyManager.Types)
		{
			FamilyParameter familyParameter = familyManager.get_Parameter(RevitParameters.SiteworksIDFamilyParameter);
			if (familyParameter != null && type.HasValue(familyParameter) && string.Compare(type.AsString(familyParameter), Convert.ToInt32(m_CurrentFamilyType).ToString("000"), ignoreCase: true) == 0)
			{
				if (list == null)
				{
					list = new List<FamilyTypeContainer>();
				}
				FamilyTypeWrapper familyTypeWrapper = new FamilyTypeWrapper(type);
				if (familyTypeWrapper.FamilyTypeName.Trim() == string.Empty)
				{
					familyTypeWrapper.FamilyTypeName = _strFamilyName;
				}
				switch (m_CurrentFamilyType)
				{
				case LandcaddID.PlantMix:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManagePlants.GetPlantMixFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.PatioWalk:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManagePlants.GetPatioWalkFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.Street:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetStreetFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.Curb:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetCurbFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.Sidewalk:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetSidewalkFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.RetainingWall:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetRetainingWallFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.Steps:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetStepsFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.ParkingLot:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetParkingLotFamilyParameterSetForType(type, familyManager)));
					break;
				case LandcaddID.SoftTerrain:
					list.Add(new FamilyTypeContainer(familyTypeWrapper, ManageFamily.GetSoftTerrainFamilyParameterSetForType(type, familyManager)));
					break;
				}
				progressBarForm.UpdateProgressBar(1);
			}
		}
		progressBarForm.SetMaxValue();
		Thread.Sleep(300);
		progressBarForm.Close();
		ePTransactionContainer.EndTransaction();
		ePTransactionContainer.Rollback();
		return list;
	}

	private bool FindParamenterValue(FamilyManager FamilyManager, FamilyType type, string _strParamter, ref string _strValue)
	{
		bool result = false;
		FamilyParameter familyParameter = FamilyManager.get_Parameter(_strParamter);
		if (familyParameter != null && type.HasValue(familyParameter))
		{
			switch (familyParameter.StorageType)
			{
			case StorageType.Double:
				if (type.AsDouble(familyParameter) is double)
				{
					result = true;
					_strValue = ValueConversion.ToString(type.AsDouble(familyParameter).Value);
				}
				break;
			case StorageType.Integer:
				if (type.AsInteger(familyParameter) is int)
				{
					result = true;
					_strValue = Convert.ToString(type.AsInteger(familyParameter));
				}
				break;
			case StorageType.String:
				if (type.AsString(familyParameter) != null)
				{
					result = true;
					_strValue = type.AsString(familyParameter);
				}
				break;
			}
		}
		return result;
	}

	private void OnCloseButtonClicked(object sender, EventArgs e)
	{
		bool flag = false;
		foreach (FamilyBinder family in m_FamilyList)
		{
			if (family.IsModified)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("SaveFamilyMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.Yes)
			{
				CloseDocuments();
				DialogResult = DialogResult.OK;
			}
		}
		else
		{
			CloseDocuments();
			DialogResult = DialogResult.OK;
		}
	}

	private void OnSaveButtonClicked(object sender, EventArgs e)
	{
		List<FamilyTypeContainer> list = new List<FamilyTypeContainer>();
		LandcaddID currentFamilyType = m_CurrentFamilyType;
		if (currentFamilyType != LandcaddID.PlantMix && currentFamilyType != LandcaddID.PatioWalk && (uint)(currentFamilyType - 101) <= 6u)
		{
			foreach (FamilyBinder family in m_FamilyList)
			{
				if (family.FamilyTypeList.Count <= 0)
				{
					RevitMessageBox.Show(EPShared.LocalRM.GetString("ValidTypeMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
					return;
				}
				if (family.IsModified)
				{
					bool flag = true;
					if (family.FamilyPath == string.Empty)
					{
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						saveFileDialog.FilterIndex = 1;
						saveFileDialog.RestoreDirectory = true;
						saveFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							family.FamilyPath = saveFileDialog.FileName;
						}
						else
						{
							flag = false;
						}
					}
					if (family.FamilyDocument.IsReadOnlyFile)
					{
						DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("ReadOnlySaveAsMessage"), EPShared.LocalRM.GetString("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
						if (dialogResult != DialogResult.Yes)
						{
							return;
						}
						SaveFileDialog saveFileDialog2 = new SaveFileDialog();
						saveFileDialog2.FilterIndex = 1;
						saveFileDialog2.RestoreDirectory = true;
						saveFileDialog2.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
						if (saveFileDialog2.ShowDialog() != DialogResult.OK)
						{
							flag = false;
							return;
						}
						family.FamilyPath = saveFileDialog2.FileName;
					}
					if (flag)
					{
						if (family.BinderFamily != null)
						{
							EPTransactionContainer ePTransactionContainer = new EPTransactionContainer(m_Document);
							ePTransactionContainer.Start(EPShared.LocalRM.GetString("UpdatingFamilyTransaction"));
							ePTransactionContainer.BeginTransaction();
							m_Document.Delete(family.BinderFamily.Id);
							ePTransactionContainer.EndTransaction();
							ePTransactionContainer.Commit();
						}
						if (family.FamilyPath != string.Empty)
						{
							SaveAsOptions saveAsOptions = new SaveAsOptions();
							saveAsOptions.OverwriteExistingFile = true;
							family.FamilyDocument.SaveAs(family.FamilyPath, saveAsOptions);
							m_TransactionContainer.Start(EPShared.LocalRM.GetString("LoadFamilyToolTip") + family.FamilyPath);
							RevitUtility.LoadFamily(m_Document, family.FamilyPath, _bUseFailureHandling: true, ref m_TransactionContainer);
							m_TransactionContainer.Commit();
						}
					}
				}
				else if (!family.Loaded && family.FamilyPath != string.Empty)
				{
					m_TransactionContainer.Start(EPShared.LocalRM.GetString("LoadFamilyToolTip") + family.FamilyPath);
					m_TransactionContainer.BeginTransaction();
					RevitUtility.LoadFamily(m_Document, family.FamilyPath, _bUseFailureHandling: true, ref m_TransactionContainer);
					m_TransactionContainer.EndTransaction();
					m_TransactionContainer.Commit();
				}
			}
		}
		CloseDocuments();
		DialogResult = DialogResult.OK;
	}

	private void OnRowSelected(object sender, EventArgs e)
	{
		Modify();
	}

	private void OnListViewColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column != m_nMixtListViewSortColumn)
		{
			m_nMixtListViewSortColumn = e.Column;
			m_FamilyManagerListView.Sorting = SortOrder.Ascending;
		}
		else if (m_FamilyManagerListView.Sorting == SortOrder.Ascending)
		{
			m_FamilyManagerListView.Sorting = SortOrder.Descending;
		}
		else
		{
			m_FamilyManagerListView.Sorting = SortOrder.Ascending;
		}
		m_FamilyManagerListView.Sort();
		m_FamilyManagerListView.ListViewItemSorter = new ListViewItemComparer(e.Column, m_FamilyManagerListView.Sorting);
	}

	private void CloseDocuments()
	{
		foreach (Document document in m_Document.Application.Documents)
		{
			bool flag = false;
			foreach (string openedFile in m_OpenedFileList)
			{
				if (RevitUtility.CompareFileNames(openedFile, document.PathName) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				document.Close(saveModified: false);
			}
		}
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OnCopyButtonClicked(object sender, EventArgs e)
	{
		int _nSelectedIndex = -1;
		List<FamilyBinder> list = new List<FamilyBinder>();
		FindSelectedFamilyIndex(ref _nSelectedIndex);
		if (_nSelectedIndex < 0)
		{
			return;
		}
		FamilyBinder familyBinder = m_FamilyList[_nSelectedIndex];
		CopyFamilyForm copyFamilyForm = new CopyFamilyForm(familyBinder, m_Document, m_CurrentFamilyType);
		DialogResult dialogResult = copyFamilyForm.ShowDialog();
		if (dialogResult != DialogResult.OK)
		{
			return;
		}
		FamilyBinder familyBinder2 = new FamilyBinder();
		Document _FamilyDocument = null;
		string newPath = copyFamilyForm.NewPath;
		if (newPath != string.Empty)
		{
			familyBinder2.FamilyName = Path.GetFileNameWithoutExtension(newPath);
			familyBinder2.FamilyPath = newPath;
			familyBinder2.IsModified = true;
		}
		if (!familyBinder2.IsModified)
		{
			return;
		}
		List<FamilyTypeContainer> list2 = new List<FamilyTypeContainer>();
		list2 = familyBinder.FamilyTypeList;
		if (ManageFamily.CreateNewFamilyDocument(m_Document, familyBinder2.FamilyTypeList, m_CurrentFamilyType, familyBinder2.FamilyPath, ref _FamilyDocument))
		{
			familyBinder2.FamilyDocument = _FamilyDocument;
			if (_FamilyDocument != null)
			{
				if (familyBinder2.FamilyTypeList == null)
				{
					familyBinder2.FamilyTypeList = new List<FamilyTypeContainer>();
				}
				if (list2 != null)
				{
					foreach (FamilyTypeContainer item in list2)
					{
						familyBinder2.FamilyTypeList.Add(item);
					}
				}
			}
			m_FamilyList.Add(familyBinder2);
			UpdateListView();
			if (m_FamilyManagerListView.Items.Count > 0)
			{
				m_FamilyManagerListView.Items[m_FamilyManagerListView.Items.Count - 1].Selected = true;
				Modify();
			}
		}
		else
		{
			string strText = EPShared.LocalRM.GetString("UnableCreateFamilyString");
			RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}

	private void OnLoadButtonClicked(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = ApplicationUtility.GetString("FamilyFilePickerFilter");
		openFileDialog.FilterIndex = 1;
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Multiselect = false;
		DialogResult dialogResult = openFileDialog.ShowDialog();
		if (dialogResult == DialogResult.OK)
		{
			LoadFamily(openFileDialog.FileName);
		}
		CheckForDuplicates(dialogResult);
	}

	private void LoadFamily(string _strFilePath)
	{
		FamilyBinder familyBinder = new FamilyBinder();
		Document _FamilyDocument = null;
		string text = _strFilePath.Substring(_strFilePath.LastIndexOf("\\") + 1);
		text = text.Substring(0, text.LastIndexOf('.'));
		familyBinder.FamilyName = text;
		familyBinder.FamilyPath = _strFilePath;
		familyBinder.IsModified = true;
		if (!familyBinder.IsModified)
		{
			return;
		}
		int num = CheckForDuplicates(familyBinder);
		if (num >= 0)
		{
			DialogResult dialogResult = RevitMessageBox.Show(EPShared.LocalRM.GetString("DuplicateFamilyInFamilyManagerMessage") + " " + EPShared.LocalRM.GetString("OverwriteMessage"), EPShared.LocalRM.GetString("Error"), MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
			if (dialogResult != DialogResult.Yes)
			{
				return;
			}
			m_FamilyList.RemoveAt(num);
		}
		if (ManageFamily.CreateNewFamilyDocument(m_Document, familyBinder.FamilyTypeList, m_CurrentFamilyType, familyBinder.FamilyPath, ref _FamilyDocument))
		{
			familyBinder.IsModified = true;
			familyBinder.FamilyDocument = _FamilyDocument;
			if (_FamilyDocument != null)
			{
				familyBinder.FamilyTypeList = PopulateFamilyTypeSetList(_FamilyDocument, familyBinder.FamilyName);
			}
			m_FamilyList.Add(familyBinder);
			UpdateListView();
			if (m_FamilyManagerListView.Items.Count > 0)
			{
				m_FamilyManagerListView.Items[m_FamilyManagerListView.Items.Count - 1].Selected = true;
				Modify();
			}
		}
		else
		{
			string strText = EPShared.LocalRM.GetString("UnableCreateFamilyString");
			RevitMessageBox.Show(strText, EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
		}
	}

	private void OnFamilyManagerFormKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			OnCloseButtonClicked(null, null);
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
		this.m_FamilyManagerGrpBox = new System.Windows.Forms.GroupBox();
		this.m_LoadButton = new System.Windows.Forms.Button();
		this.m_CopyButton = new System.Windows.Forms.Button();
		this.m_ModifyButton = new System.Windows.Forms.Button();
		this.m_AddButton = new System.Windows.Forms.Button();
		this.m_FamilyManagerListView = new System.Windows.Forms.ListView();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_SaveButton = new System.Windows.Forms.Button();
		this.m_FamilyManagerLbl = new System.Windows.Forms.Label();
		this.m_FamilyManagerGrpBox.SuspendLayout();
		base.SuspendLayout();
		this.m_FamilyManagerGrpBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_FamilyManagerGrpBox.Controls.Add(this.m_LoadButton);
		this.m_FamilyManagerGrpBox.Controls.Add(this.m_CopyButton);
		this.m_FamilyManagerGrpBox.Controls.Add(this.m_ModifyButton);
		this.m_FamilyManagerGrpBox.Controls.Add(this.m_AddButton);
		this.m_FamilyManagerGrpBox.Controls.Add(this.m_FamilyManagerListView);
		this.m_FamilyManagerGrpBox.Location = new System.Drawing.Point(12, 43);
		this.m_FamilyManagerGrpBox.Name = "m_FamilyManagerGrpBox";
		this.m_FamilyManagerGrpBox.Size = new System.Drawing.Size(500, 289);
		this.m_FamilyManagerGrpBox.TabIndex = 1;
		this.m_FamilyManagerGrpBox.TabStop = false;
		this.m_FamilyManagerGrpBox.Text = "Family Manager";
		this.m_LoadButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_LoadButton.Location = new System.Drawing.Point(419, 106);
		this.m_LoadButton.Name = "m_LoadButton";
		this.m_LoadButton.Size = new System.Drawing.Size(75, 23);
		this.m_LoadButton.TabIndex = 4;
		this.m_LoadButton.Text = "Load";
		this.m_LoadButton.UseVisualStyleBackColor = true;
		this.m_LoadButton.Click += new System.EventHandler(this.OnLoadButtonClicked);
		this.m_CopyButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_CopyButton.Location = new System.Drawing.Point(419, 77);
		this.m_CopyButton.Name = "m_CopyButton";
		this.m_CopyButton.Size = new System.Drawing.Size(75, 23);
		this.m_CopyButton.TabIndex = 3;
		this.m_CopyButton.Text = "Copy";
		this.m_CopyButton.UseVisualStyleBackColor = true;
		this.m_CopyButton.Click += new System.EventHandler(this.OnCopyButtonClicked);
		this.m_ModifyButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_ModifyButton.Location = new System.Drawing.Point(419, 48);
		this.m_ModifyButton.Name = "m_ModifyButton";
		this.m_ModifyButton.Size = new System.Drawing.Size(75, 23);
		this.m_ModifyButton.TabIndex = 2;
		this.m_ModifyButton.Text = "Modify";
		this.m_ModifyButton.UseVisualStyleBackColor = true;
		this.m_ModifyButton.Click += new System.EventHandler(this.OnFamilyManagerModifyClicked);
		this.m_AddButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.m_AddButton.Location = new System.Drawing.Point(419, 19);
		this.m_AddButton.Name = "m_AddButton";
		this.m_AddButton.Size = new System.Drawing.Size(75, 23);
		this.m_AddButton.TabIndex = 1;
		this.m_AddButton.Text = "Add";
		this.m_AddButton.UseVisualStyleBackColor = true;
		this.m_AddButton.Click += new System.EventHandler(this.OnFamilyManagerAddClicked);
		this.m_FamilyManagerListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_FamilyManagerListView.FullRowSelect = true;
		this.m_FamilyManagerListView.Location = new System.Drawing.Point(6, 19);
		this.m_FamilyManagerListView.MultiSelect = false;
		this.m_FamilyManagerListView.Name = "m_FamilyManagerListView";
		this.m_FamilyManagerListView.Size = new System.Drawing.Size(407, 264);
		this.m_FamilyManagerListView.TabIndex = 0;
		this.m_FamilyManagerListView.UseCompatibleStateImageBehavior = false;
		this.m_FamilyManagerListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnListViewColumnClick);
		this.m_FamilyManagerListView.DoubleClick += new System.EventHandler(this.OnRowSelected);
		this.m_CloseButton.Location = new System.Drawing.Point(435, 338);
		this.m_CloseButton.Name = "m_CloseButton";
		this.m_CloseButton.Size = new System.Drawing.Size(75, 23);
		this.m_CloseButton.TabIndex = 3;
		this.m_CloseButton.Text = "Close";
		this.m_CloseButton.UseVisualStyleBackColor = true;
		this.m_CloseButton.Click += new System.EventHandler(this.OnCloseButtonClicked);
		this.m_SaveButton.Location = new System.Drawing.Point(354, 338);
		this.m_SaveButton.Name = "m_SaveButton";
		this.m_SaveButton.Size = new System.Drawing.Size(75, 23);
		this.m_SaveButton.TabIndex = 2;
		this.m_SaveButton.Text = "Save";
		this.m_SaveButton.UseVisualStyleBackColor = true;
		this.m_SaveButton.Click += new System.EventHandler(this.OnSaveButtonClicked);
		this.m_FamilyManagerLbl.AutoSize = true;
		this.m_FamilyManagerLbl.Location = new System.Drawing.Point(18, 13);
		this.m_FamilyManagerLbl.Name = "m_FamilyManagerLbl";
		this.m_FamilyManagerLbl.Size = new System.Drawing.Size(35, 13);
		this.m_FamilyManagerLbl.TabIndex = 0;
		this.m_FamilyManagerLbl.Text = "label1";
		base.AcceptButton = this.m_SaveButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(524, 373);
		base.Controls.Add(this.m_FamilyManagerLbl);
		base.Controls.Add(this.m_SaveButton);
		base.Controls.Add(this.m_CloseButton);
		base.Controls.Add(this.m_FamilyManagerGrpBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		this.MinimumSize = new System.Drawing.Size(530, 337);
		base.Name = "FamilyManagerForm";
		this.Text = "FamilyManager";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnFamilyManagerFormKeyDown);
		this.m_FamilyManagerGrpBox.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
