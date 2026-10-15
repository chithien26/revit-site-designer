using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EPCommon;

public class QuantumListView : ListView
{
	public string[] WpfCellChoices(ListViewItem item)
	{
		string key = item.SubItems[0].Text;
		string[] options = { "SpacingMethodPlantMixFamilyParameter", "EdgePathWalkFamilyParameter", "MowingStripPathWalkFamilyParameter", "SideWalkLocationLineParameter", "StreetCurbDisplayParameter", "MassCreationOnlyMode", "SiteComponentPromptForStartingSide", "UseSharedElevation", "RetainingWallSlopeProjectionParameter", "CheckForUpdateNotification" };
		string[] materials = { "ProposedToposurfaceMaterialParameter", "ParkingLotIslandToposurfaceMaterialParameter" };
		foreach (string option in options)
			if (string.Equals(key, EPShared.LocalRM.GetString(option), StringComparison.CurrentCultureIgnoreCase))
				return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Cast<object>(m_OptionsCmbBox.Items), x => x.ToString()));
		foreach (string material in materials)
			if (string.Equals(key, EPShared.LocalRM.GetString(material), StringComparison.CurrentCultureIgnoreCase))
				return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(System.Linq.Enumerable.Cast<object>(m_CmbBox.Items), x => x.ToString()));
		return new string[0];
	}
	public void SetWpfCellValue(ListViewItem item, int column, string value)
	{
		item.SubItems[column].Text = value;
		OnListViewItemChanged?.Invoke(item.Index);
	}
	private ListViewItem m_ListItem;

	private int m_nX;

	private int m_nY;

	private string m_strSubItemText;

	private int m_nSubItemSelected;

	private TextBox m_EditBox = new TextBox();

	private TextBox m_FileEditBox = new TextBox();

	private Button m_FileOpenButton = new Button();

	private TextBox m_DirectoryEditBox = new TextBox();

	private Button m_DirectoryOpenButton = new Button();

	private Button m_ColorOpenButton = new Button();

	private ComboBox m_OptionsCmbBox = new ComboBox();

	private ComboBox m_CmbBox = new ComboBox();

	public DialogType m_DlgType;

	private bool m_bKeyBoard;

	private bool m_bHandleListViewItem = true;

	private string m_strPreviousFilePath = string.Empty;

	private string m_strPreviousDirectoryPath = string.Empty;

	private const int m_nPopupControlHeight = 20;

	public ComboBox CmbBox
	{
		get
		{
			return m_OptionsCmbBox;
		}
		set
		{
			m_OptionsCmbBox = value;
		}
	}

	public ComboBox MaterialCmbBox
	{
		get
		{
			return m_CmbBox;
		}
		set
		{
			m_CmbBox = value;
		}
	}

	public ListViewItem CurrListItem
	{
		get
		{
			return m_ListItem;
		}
		set
		{
			m_ListItem = value;
		}
	}

	public event ListViewItemChanged OnListViewItemChanged;

	public QuantumListView()
	{
		Dock = DockStyle.None;
		FullRowSelect = true;
		MinimumSize = new Size(430, 280);
		Name = "QuantumListView";
		Size = new Size(430, 280);
		Location = new Point(10, 3);
		TabIndex = 0;
		View = View.Details;
		MouseDown += ListViewMouseDown;
		KeyDown += ListViewKeyDown;
		Click += ListViewSingleClick;
		ColumnWidthChanging += ColumnWidthChanged;
		GridLines = true;
		Scrollable = false;
		OwnerDraw = false;
		Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_EditBox.Size = new Size(0, 20);
		m_EditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_EditBox });
		m_EditBox.KeyPress += EditOver;
		m_EditBox.LostFocus += FocusOver;
		m_EditBox.BackColor = Color.White;
		m_EditBox.BorderStyle = BorderStyle.Fixed3D;
		m_EditBox.Hide();
		m_EditBox.Text = "";
		m_FileEditBox.Size = new Size(0, 20);
		m_FileEditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_FileEditBox });
		m_FileEditBox.KeyPress += FileEditEditOver;
		m_FileEditBox.LostFocus += FileEditFocusOver;
		m_FileEditBox.BackColor = Color.White;
		m_FileEditBox.BorderStyle = BorderStyle.Fixed3D;
		m_FileEditBox.Hide();
		m_FileEditBox.Text = "";
		Controls.AddRange(new Control[1] { m_FileOpenButton });
		m_FileOpenButton.Size = new Size(0, 20);
		m_FileOpenButton.Location = new Point(0, 0);
		m_FileOpenButton.BackColor = Control.DefaultBackColor;
		m_FileOpenButton.Click += FileOpenButtonClick;
		m_FileOpenButton.Text = "...";
		m_FileOpenButton.Hide();
		m_DirectoryEditBox.Size = new Size(0, 20);
		m_DirectoryEditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_DirectoryEditBox });
		m_DirectoryEditBox.KeyPress += DirectoryEditEditOver;
		m_DirectoryEditBox.LostFocus += DirectoryEditFocusOver;
		m_DirectoryEditBox.BackColor = Color.White;
		m_DirectoryEditBox.BorderStyle = BorderStyle.Fixed3D;
		m_DirectoryEditBox.Hide();
		m_DirectoryEditBox.Text = "";
		Controls.AddRange(new Control[1] { m_DirectoryOpenButton });
		m_DirectoryOpenButton.Size = new Size(0, 20);
		m_DirectoryOpenButton.Location = new Point(0, 0);
		m_DirectoryOpenButton.BackColor = Control.DefaultBackColor;
		m_DirectoryOpenButton.Click += DirectoryOpenButtonClick;
		m_DirectoryOpenButton.Text = "...";
		m_DirectoryOpenButton.Hide();
		Controls.AddRange(new Control[1] { m_ColorOpenButton });
		m_ColorOpenButton.Size = new Size(0, 20);
		m_ColorOpenButton.Location = new Point(0, 0);
		m_ColorOpenButton.ImageAlign = ContentAlignment.MiddleLeft;
		m_ColorOpenButton.BackColor = Control.DefaultBackColor;
		m_ColorOpenButton.Click += ColorOpenButtonClick;
		m_ColorOpenButton.LostFocus += ColorFocusOver;
		m_ColorOpenButton.KeyPress += ColorKeyPress;
		m_ColorOpenButton.Text = string.Empty;
		m_ColorOpenButton.Hide();
		Controls.AddRange(new Control[1] { m_OptionsCmbBox });
		m_OptionsCmbBox.SelectedIndexChanged += CmbSelected;
		m_OptionsCmbBox.LostFocus += CmbFocusOver;
		m_OptionsCmbBox.KeyPress += CmbKeyPress;
		m_OptionsCmbBox.BackColor = Color.White;
		m_OptionsCmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
		m_OptionsCmbBox.Hide();
		Controls.AddRange(new Control[1] { m_CmbBox });
		m_CmbBox.SelectedIndexChanged += MaterialCmbSelected;
		m_CmbBox.LostFocus += MaterialCmbFocusOver;
		m_CmbBox.KeyPress += MaterialCmbKeyPress;
		m_CmbBox.BackColor = Color.White;
		m_CmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
		m_CmbBox.Hide();
	}

	public QuantumListView(int _nWidth, int _nHeight)
	{
		Dock = DockStyle.None;
		FullRowSelect = true;
		MinimumSize = new Size(_nWidth, _nHeight);
		Name = "QuantumListView";
		Size = new Size(_nWidth, _nHeight);
		Location = new Point(10, 3);
		TabIndex = 0;
		View = View.Details;
		MouseDown += ListViewMouseDown;
		KeyDown += ListViewKeyDown;
		Click += ListViewSingleClick;
		ColumnWidthChanging += ColumnWidthChanged;
		GridLines = true;
		Scrollable = false;
		OwnerDraw = false;
		m_EditBox.Size = new Size(0, 20);
		m_EditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_EditBox });
		m_EditBox.KeyPress += EditOver;
		m_EditBox.LostFocus += FocusOver;
		m_EditBox.BackColor = Color.White;
		m_EditBox.BorderStyle = BorderStyle.Fixed3D;
		m_EditBox.Hide();
		m_EditBox.Text = "";
		Controls.AddRange(new Control[1] { m_OptionsCmbBox });
		m_OptionsCmbBox.SelectedIndexChanged += CmbSelected;
		m_OptionsCmbBox.LostFocus += CmbFocusOver;
		m_OptionsCmbBox.KeyPress += CmbKeyPress;
		m_OptionsCmbBox.BackColor = Color.White;
		m_OptionsCmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
		m_OptionsCmbBox.Hide();
		Controls.AddRange(new Control[1] { m_CmbBox });
		m_CmbBox.SelectedIndexChanged += MaterialCmbSelected;
		m_CmbBox.LostFocus += MaterialCmbFocusOver;
		m_CmbBox.KeyPress += MaterialCmbKeyPress;
		m_CmbBox.BackColor = Color.White;
		m_CmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
		m_CmbBox.Hide();
		m_FileEditBox.Size = new Size(0, 20);
		m_FileEditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_FileEditBox });
		m_FileEditBox.KeyPress += FileEditEditOver;
		m_FileEditBox.LostFocus += FileEditFocusOver;
		m_FileEditBox.BackColor = Color.White;
		m_FileEditBox.BorderStyle = BorderStyle.Fixed3D;
		m_FileEditBox.Hide();
		m_FileEditBox.Text = "";
		Controls.AddRange(new Control[1] { m_FileOpenButton });
		m_FileOpenButton.Size = new Size(0, 20);
		m_FileOpenButton.Location = new Point(0, 0);
		m_FileOpenButton.BackColor = Control.DefaultBackColor;
		m_FileOpenButton.Click += FileOpenButtonClick;
		m_FileOpenButton.Text = "...";
		m_FileOpenButton.Hide();
		m_DirectoryEditBox.Size = new Size(0, 20);
		m_DirectoryEditBox.Location = new Point(0, 0);
		Controls.AddRange(new Control[1] { m_DirectoryEditBox });
		m_DirectoryEditBox.KeyPress += DirectoryEditEditOver;
		m_DirectoryEditBox.LostFocus += DirectoryEditFocusOver;
		m_DirectoryEditBox.BackColor = Color.White;
		m_DirectoryEditBox.BorderStyle = BorderStyle.Fixed3D;
		m_DirectoryEditBox.Hide();
		m_DirectoryEditBox.Text = "";
		Controls.AddRange(new Control[1] { m_DirectoryOpenButton });
		m_DirectoryOpenButton.Size = new Size(0, 20);
		m_DirectoryOpenButton.Location = new Point(0, 0);
		m_DirectoryOpenButton.BackColor = Control.DefaultBackColor;
		m_DirectoryOpenButton.Click += DirectoryOpenButtonClick;
		m_DirectoryOpenButton.Text = "...";
		m_DirectoryOpenButton.Hide();
		Controls.AddRange(new Control[1] { m_ColorOpenButton });
		m_ColorOpenButton.Size = new Size(0, 20);
		m_ColorOpenButton.Location = new Point(0, 0);
		m_ColorOpenButton.ImageAlign = ContentAlignment.MiddleLeft;
		m_ColorOpenButton.BackColor = Control.DefaultBackColor;
		m_ColorOpenButton.Click += ColorOpenButtonClick;
		m_ColorOpenButton.LostFocus += ColorFocusOver;
		m_ColorOpenButton.KeyPress += ColorKeyPress;
		m_ColorOpenButton.Text = string.Empty;
		m_ColorOpenButton.Hide();
	}

	private void ColorOpenButtonClick(object sender, EventArgs e)
	{
		using ColorDialog colorDialog = new ColorDialog();
		Color _FindColor = default;
		if (RevitUtility.FindColorFromString(m_ListItem.SubItems[m_nSubItemSelected].Text, ref _FindColor))
		{
			colorDialog.Color = _FindColor;
		}
		DialogResult dialogResult = colorDialog.ShowDialog();
		if (dialogResult == DialogResult.OK)
		{
			if (colorDialog.Color.IsNamedColor)
			{
				m_ListItem.SubItems[m_nSubItemSelected].Text = colorDialog.Color.Name;
			}
			else
			{
				m_ListItem.SubItems[m_nSubItemSelected].Text = ApplicationUtility.GetString("RGBString") + " " + Convert.ToString(colorDialog.Color.R) + "-" + Convert.ToString(colorDialog.Color.G) + "-" + Convert.ToString(colorDialog.Color.B);
			}
		}
		m_ColorOpenButton.Hide();
		m_bHandleListViewItem = true;
	}

	private void FileOpenButtonClick(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = EPShared.LocalRM.GetString("SelectTemplateMessage");
		openFileDialog.RestoreDirectory = true;
		openFileDialog.Filter = ApplicationUtility.GetString("TemplateFilePickerFilter");
		openFileDialog.Multiselect = false;
		DialogResult dialogResult = openFileDialog.ShowDialog();
		if (dialogResult == DialogResult.OK)
		{
			m_ListItem.SubItems[m_nSubItemSelected].Text = openFileDialog.FileName;
		}
		m_FileEditBox.Hide();
		m_FileOpenButton.Hide();
		m_bHandleListViewItem = true;
	}

	private void DirectoryOpenButtonClick(object sender, EventArgs e)
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.ShowNewFolderButton = true;
		if (m_ListItem.SubItems[m_nSubItemSelected].Text != string.Empty)
		{
			folderBrowserDialog.SelectedPath = Path.GetDirectoryName(m_ListItem.SubItems[m_nSubItemSelected].Text);
		}
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			m_ListItem.SubItems[m_nSubItemSelected].Text = folderBrowserDialog.SelectedPath;
		}
		m_DirectoryEditBox.Hide();
		m_DirectoryOpenButton.Hide();
		m_bHandleListViewItem = true;
	}

	public void RefreshListView()
	{
		foreach (Control control in Controls)
		{
			control.Hide();
		}
	}

	private void EditOver(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (m_bHandleListViewItem)
			{
				m_bHandleListViewItem = false;
				m_ListItem.SubItems[m_nSubItemSelected].Text = m_EditBox.Text;
				if (OnListViewItemChanged != null)
				{
					OnListViewItemChanged(m_ListItem.Index);
				}
				m_EditBox.Hide();
				m_bHandleListViewItem = true;
			}
		}
		else if (e.KeyChar == '\t' && m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_EditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_EditBox.Hide();
			m_bHandleListViewItem = true;
		}
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_EditBox.Hide();
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void FocusOver(object sender, EventArgs e)
	{
		if (m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_EditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_EditBox.Hide();
			Refresh();
			FocusedItem = SelectedItems[0];
			Focus();
			m_bHandleListViewItem = true;
		}
	}

	private void FileEditEditOver(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (m_bHandleListViewItem)
			{
				m_bHandleListViewItem = false;
				m_ListItem.SubItems[m_nSubItemSelected].Text = m_FileEditBox.Text;
				if (OnListViewItemChanged != null)
				{
					OnListViewItemChanged(m_ListItem.Index);
				}
				m_FileEditBox.Hide();
				m_FileOpenButton.Hide();
				m_bHandleListViewItem = true;
			}
		}
		else if (e.KeyChar == '\t' && m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_FileEditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_FileEditBox.Hide();
			m_FileOpenButton.Hide();
			m_bHandleListViewItem = true;
		}
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_FileEditBox.Hide();
		m_FileOpenButton.Hide();
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void FileEditFocusOver(object sender, EventArgs e)
	{
		if (m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_FileEditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_FileEditBox.Hide();
			m_FileOpenButton.Hide();
			Refresh();
			FocusedItem = SelectedItems[0];
			Focus();
			m_bHandleListViewItem = true;
		}
	}

	private void DirectoryEditEditOver(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			e.Handled = true;
			if (m_bHandleListViewItem)
			{
				m_bHandleListViewItem = false;
				m_ListItem.SubItems[m_nSubItemSelected].Text = m_DirectoryEditBox.Text;
				if (OnListViewItemChanged != null)
				{
					OnListViewItemChanged(m_ListItem.Index);
				}
				m_DirectoryEditBox.Hide();
				m_DirectoryOpenButton.Hide();
				m_bHandleListViewItem = true;
			}
		}
		else if (e.KeyChar == '\t' && m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_DirectoryEditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_DirectoryEditBox.Hide();
			m_DirectoryOpenButton.Hide();
			m_bHandleListViewItem = true;
		}
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_DirectoryEditBox.Hide();
		m_DirectoryOpenButton.Hide();
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void DirectoryEditFocusOver(object sender, EventArgs e)
	{
		if (m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			m_ListItem.SubItems[m_nSubItemSelected].Text = m_DirectoryEditBox.Text;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_DirectoryEditBox.Hide();
			m_DirectoryOpenButton.Hide();
			Refresh();
			FocusedItem = SelectedItems[0];
			Focus();
			m_bHandleListViewItem = true;
		}
	}

	private void ColorFocusOver(object sender, EventArgs e)
	{
		if (m_bHandleListViewItem)
		{
			m_bHandleListViewItem = false;
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
			m_ColorOpenButton.Hide();
			Refresh();
			FocusedItem = SelectedItems[0];
			Focus();
			m_bHandleListViewItem = true;
		}
	}

	private void HideSelectedControls()
	{
		if (m_FileEditBox.Visible)
		{
			m_FileEditBox.Hide();
		}
		if (m_FileOpenButton.Visible)
		{
			m_FileOpenButton.Hide();
		}
		if (m_DirectoryEditBox.Visible)
		{
			m_DirectoryEditBox.Hide();
		}
		if (m_DirectoryOpenButton.Visible)
		{
			m_DirectoryOpenButton.Hide();
		}
		if (m_OptionsCmbBox.Visible)
		{
			m_OptionsCmbBox.Hide();
		}
		if (m_CmbBox.Visible)
		{
			m_CmbBox.Hide();
		}
		if (m_ColorOpenButton.Visible)
		{
			m_ColorOpenButton.Hide();
		}
		if (m_EditBox.Visible)
		{
			m_EditBox.Hide();
		}
	}

	private new void ColumnWidthChanged(object sender, ColumnWidthChangingEventArgs e)
	{
		Draw();
	}

	public void ListViewSingleClick(object sender, EventArgs e)
	{
		Draw();
	}

	private void Draw()
	{
		int num = 0;
		int num2 = Columns[0].Width;
		if (!m_bKeyBoard)
		{
			m_nSubItemSelected = 0;
			int nX = m_nX;
			for (int i = 1; i < Columns.Count; i++)
			{
				num = num2;
				num2 += Columns[i].Width;
				if (nX > num && nX < num2)
				{
					m_nSubItemSelected = i;
					break;
				}
			}
			if (m_nSubItemSelected == 0)
			{
				HideSelectedControls();
				return;
			}
		}
		else
		{
			num = num2;
			num2 += Columns[1].Width;
			m_nSubItemSelected = 1;
		}
		if (string.Compare(EPShared.LocalRM.GetString("PlantSpacingPlantMixFamilyParameter"), m_ListItem.SubItems[0].Text, ignoreCase: true) == 0 && string.Compare(Convert.ToString(m_OptionsCmbBox.SelectedItem), EPShared.LocalRM.GetString("AreaPlantMixFamilyParameter"), ignoreCase: true) == 0)
		{
			RevitMessageBox.Show(EPShared.LocalRM.GetString("NoAreaSpacingMethod"), EPShared.LocalRM.GetString("Error"), MessageBoxIcon.Hand);
			return;
		}
		m_strSubItemText = m_ListItem.SubItems[m_nSubItemSelected].Text;
		string text = Columns[m_nSubItemSelected].Text;
		string strB = m_ListItem.SubItems[0].Text;
		Rectangle rectangle;
		if (string.Compare(EPShared.LocalRM.GetString("SpacingMethodPlantMixFamilyParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("EdgePathWalkFamilyParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("MowingStripPathWalkFamilyParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SideWalkLocationLineParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetCurbDisplayParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("MassCreationOnlyMode"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SiteComponentPromptForStartingSide"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("UseSharedElevation"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("RetainingWallSlopeProjectionParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("CheckForUpdateNotification"), strB, ignoreCase: true) == 0)
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_OptionsCmbBox.Width = Columns[m_nSubItemSelected].Width - 10;
			m_OptionsCmbBox.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			m_OptionsCmbBox.Show();
			m_OptionsCmbBox.SelectAll();
			int num3 = m_OptionsCmbBox.FindString(m_strSubItemText);
			if (num3 >= 0)
			{
				m_OptionsCmbBox.SelectedIndex = num3;
			}
			else if (m_OptionsCmbBox.SelectedIndex < 0 && m_OptionsCmbBox.Items.Count > 0)
			{
				m_OptionsCmbBox.SelectedIndex = 0;
			}
			SiteDesigner.UI.WpfFocus.Focus(m_OptionsCmbBox);
		}
		else if (string.Compare(EPShared.LocalRM.GetString("StreetPavementSurfaceParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetCurbMaterialParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StepsMaterialParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ParkingLotParkingPavementSurfaceParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("RetainingWallMaterialParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SideWalkPavementSurfaceParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ProposedToposurfaceMaterialParameter"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ParkingLotIslandToposurfaceMaterialParameter"), strB, ignoreCase: true) == 0)
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_CmbBox.Width = Columns[m_nSubItemSelected].Width - 10;
			m_CmbBox.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			m_CmbBox.Show();
			m_CmbBox.SelectAll();
			int num4 = m_CmbBox.FindString(m_strSubItemText);
			if (num4 >= 0)
			{
				m_CmbBox.SelectedIndex = num4;
			}
			else if (m_CmbBox.SelectedIndex < 0 && m_CmbBox.Items.Count > 0)
			{
				m_CmbBox.SelectedIndex = 0;
			}
			SiteDesigner.UI.WpfFocus.Focus(m_CmbBox);
		}
		else if (string.Compare(EPShared.LocalRM.GetString("MassTemplateFileNameString"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetMassTemplateFileNameString"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("CurbMassTemplateFileNameString"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SidewalkMassTemplateFileNameString"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("RetainingWallMassTemplateFileNameString"), strB, ignoreCase: true) == 0)
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_FileEditBox.Width = Columns[m_nSubItemSelected].Width - 35;
			m_FileEditBox.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			m_FileOpenButton.Size = new Size(25, m_FileEditBox.Height);
			m_FileOpenButton.Location = new Point(num2 - 35, m_ListItem.Bounds.Y - 4);
			m_FileOpenButton.Show();
			m_FileEditBox.Show();
			m_FileEditBox.Text = m_strSubItemText;
			m_strPreviousFilePath = m_strSubItemText;
		}
		else if (string.Compare(EPShared.LocalRM.GetString("SoftTerrainLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SidewalkLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("RetainingWallLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("StreetLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("IntersectionLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("CurbLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("ParkingLotLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("FeatureLineLineSubCategoryColor"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("PointWipeoutLineSubCategoryColor"), strB, ignoreCase: true) == 0)
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_ColorOpenButton.Width = Columns[m_nSubItemSelected].Width - 10;
			m_ColorOpenButton.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			Color _FindColor = default;
			bool flag = RevitUtility.FindColorFromString(m_ListItem.SubItems[m_nSubItemSelected].Text, ref _FindColor);
			Bitmap bitmap = new Bitmap(15, 8);
			if (flag)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					for (int k = 0; k < bitmap.Height; k++)
					{
						bitmap.SetPixel(j, k, _FindColor);
					}
				}
			}
			m_ColorOpenButton.Image = bitmap;
			m_ColorOpenButton.Show();
			m_ColorOpenButton.Text = m_strSubItemText;
			SiteDesigner.UI.WpfFocus.Focus(m_ColorOpenButton);
		}
		else if (string.Compare(EPShared.LocalRM.GetString("LibraryPath"), strB, ignoreCase: true) == 0 || string.Compare(EPShared.LocalRM.GetString("SamplePath"), strB, ignoreCase: true) == 0)
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_DirectoryEditBox.Width = Columns[m_nSubItemSelected].Width - 35;
			m_DirectoryEditBox.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			m_DirectoryOpenButton.Size = new Size(25, m_DirectoryEditBox.Height);
			m_DirectoryOpenButton.Location = new Point(num2 - 35, m_ListItem.Bounds.Y - 4);
			m_DirectoryOpenButton.Show();
			m_DirectoryEditBox.Show();
			m_DirectoryEditBox.Text = m_strSubItemText;
			m_strPreviousDirectoryPath = m_strSubItemText;
		}
		else
		{
			rectangle = new Rectangle(num, m_ListItem.Bounds.Y, num2, m_ListItem.Bounds.Bottom);
			m_EditBox.Width = Columns[m_nSubItemSelected].Width;
			m_EditBox.Location = new Point(num, m_ListItem.Bounds.Y - 4);
			m_EditBox.Show();
			m_EditBox.Text = m_strSubItemText;
			SiteDesigner.UI.WpfFocus.SelectAll(m_EditBox);
			SiteDesigner.UI.WpfFocus.Focus(m_EditBox);
		}
	}

	public void SetFocus(int _nIndex)
	{
		Refresh();
		Rectangle rectangle = new Rectangle(m_ListItem.SubItems[1].Bounds.X, m_ListItem.SubItems[1].Bounds.Y, Columns[0].Width + Columns[m_nSubItemSelected].Width, m_ListItem.SubItems[1].Bounds.Bottom);
		m_EditBox.Width = Columns[m_nSubItemSelected].Width;
		m_EditBox.Location = new Point(m_ListItem.SubItems[1].Bounds.X, m_ListItem.SubItems[1].Bounds.Y - 4);
		m_EditBox.Text = string.Empty;
		m_EditBox.Show();
		SiteDesigner.UI.WpfFocus.SelectAll(m_EditBox);
		SiteDesigner.UI.WpfFocus.Focus(m_EditBox);
	}

	public void SetFilePickerFocus(int _nIndex)
	{
		m_ListItem.SubItems[1].Text = m_strPreviousFilePath;
	}

	public void SetDirectoryPickerFocus(int _nIndex)
	{
		m_ListItem.SubItems[1].Text = m_strPreviousDirectoryPath;
	}

	public void ListViewMouseDown(object sender, MouseEventArgs e)
	{
		m_ListItem = GetItemAt(e.X, e.Y);
		m_nX = e.X;
		m_nY = e.Y;
		m_bKeyBoard = false;
	}

	private void ListViewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			m_bKeyBoard = true;
			Rectangle itemRect = GetItemRect(SelectedIndices[0]);
			m_nX = itemRect.X + Columns[1].Width;
			m_nY = itemRect.Y;
			m_ListItem = FindItemWithText(SelectedItems[0].Text);
			Draw();
		}
		else
		{
			if (e.KeyCode != Keys.Escape || m_nSubItemSelected <= 0)
			{
				return;
			}
			m_nSubItemSelected = 0;
			HideSelectedControls();
			EPForm ePForm = null;
			for (Control control = Parent; control != null; control = control.Parent)
			{
				if (control is EPForm ePForm2)
				{
					ePForm2.OverrideKeyUp();
					break;
				}
			}
		}
	}

	private void CmbKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r' && e.KeyChar != '\u001b')
		{
			return;
		}
		m_OptionsCmbBox.Hide();
		m_CmbBox.Hide();
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void ColorKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_ColorOpenButton.Hide();
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void CmbSelected(object sender, EventArgs e)
	{
		int selectedIndex = m_OptionsCmbBox.SelectedIndex;
		if (selectedIndex >= 0)
		{
			string text = m_OptionsCmbBox.Items[selectedIndex].ToString();
			m_ListItem.SubItems[m_nSubItemSelected].Text = text;
			m_OptionsCmbBox.Hide();
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
		}
	}

	private void CmbFocusOver(object sender, EventArgs e)
	{
		m_OptionsCmbBox.Hide();
		m_CmbBox.Hide();
		Focus();
	}

	private void MaterialCmbKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r' && e.KeyChar != '\u001b')
		{
			return;
		}
		m_CmbBox.Hide();
		if (e.KeyChar != '\u001b')
		{
			return;
		}
		m_nSubItemSelected = 0;
		EPForm ePForm = null;
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control is EPForm ePForm2)
			{
				ePForm2.OverrideKeyUp();
				break;
			}
		}
	}

	private void MaterialCmbSelected(object sender, EventArgs e)
	{
		int selectedIndex = m_CmbBox.SelectedIndex;
		if (selectedIndex >= 0)
		{
			string text = m_CmbBox.Items[selectedIndex].ToString();
			m_ListItem.SubItems[m_nSubItemSelected].Text = text;
			m_CmbBox.Hide();
			if (OnListViewItemChanged != null)
			{
				OnListViewItemChanged(m_ListItem.Index);
			}
		}
	}

	private void MaterialCmbFocusOver(object sender, EventArgs e)
	{
		m_CmbBox.Hide();
		Focus();
	}

	public void SetAnchor()
	{
		Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
	}
}
