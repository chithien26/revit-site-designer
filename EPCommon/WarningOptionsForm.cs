using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SITES.Sites_Core;

namespace EPCommon;

public class WarningOptionsForm : SiteDesigner.UI.WpfForm
{
	private string m_strSettingAppName;

	private string m_strSettingName;

	private List<RadioButton> m_WarningOptions;

	private int m_nDefaultOption;

	private bool m_bDisplayDoNotShow;

	private int m_nDoNotShowOptionIndex;

	private bool m_bUseSettingAppName;

	private string m_contextualHelpID = "HID_SDaaa";

	private IContainer components;

	private Button m_OKButton;

	private Button m_CancelButton;

	private CheckBox m_DoNotShowMessageAgainCheckBox;

	private TextBox m_WarningMessageTextBox;

	public string WarningOption
	{
		get
		{
			for (int i = 0; i < m_WarningOptions.Count; i++)
			{
				if (m_WarningOptions[i].Checked)
				{
					return m_WarningOptions[i].Text;
				}
			}
			return string.Empty;
		}
	}

	public WarningOptionsForm(string _strSettingName, string _strWarningText, List<string> _WarningOptions)
	{
		SetupForm(EPShared.LocalRM.GetString("WarningDialog"), string.Empty, _strSettingName, _strWarningText, _WarningOptions, -1, _bDisplayOKCancel: true, _bDisplayDoNotShow: true, -1, _bUseSettingAppName: false);
	}

	public WarningOptionsForm(string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions)
	{
		SetupForm(EPShared.LocalRM.GetString("WarningDialog"), _strSettingAppName, _strSettingName, _strWarningText, _WarningOptions, -1, _bDisplayOKCancel: true, _bDisplayDoNotShow: true, -1, _bUseSettingAppName: true);
	}

	public WarningOptionsForm(string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption)
	{
		SetupForm(EPShared.LocalRM.GetString("WarningDialog"), _strSettingAppName, _strSettingName, _strWarningText, _WarningOptions, _nDefaultOption, _bDisplayOKCancel: true, _bDisplayDoNotShow: true, -1, _bUseSettingAppName: true);
	}

	public WarningOptionsForm(string _strFormTitle, string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption, bool _bDisplayOKCancel)
	{
		SetupForm(_strFormTitle, _strSettingAppName, _strSettingName, _strWarningText, _WarningOptions, _nDefaultOption, _bDisplayOKCancel, _bDisplayDoNotShow: true, -1, _bUseSettingAppName: true);
	}

	public WarningOptionsForm(string _strFormTitle, string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption, bool _bDisplayOKCancel, bool _bDisplayDoNotShow)
	{
		SetupForm(_strFormTitle, _strSettingAppName, _strSettingName, _strWarningText, _WarningOptions, _nDefaultOption, _bDisplayOKCancel, _bDisplayDoNotShow, -1, _bUseSettingAppName: true);
	}

	public WarningOptionsForm(string _strFormTitle, string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption, bool _bDisplayOKCancel, bool _bDisplayDoNotShow, int _nDoNotShowOptionIndex)
	{
		SetupForm(_strFormTitle, _strSettingAppName, _strSettingName, _strWarningText, _WarningOptions, _nDefaultOption, _bDisplayOKCancel, _bDisplayDoNotShow, _nDoNotShowOptionIndex, _bUseSettingAppName: true);
	}

	public WarningOptionsForm(string _strFormTitle, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption, bool _bDisplayOKCancel, bool _bDisplayDoNotShow, int _nDoNotShowOptionIndex)
	{
		SetupForm(_strFormTitle, string.Empty, _strSettingName, _strWarningText, _WarningOptions, _nDefaultOption, _bDisplayOKCancel, _bDisplayDoNotShow, _nDoNotShowOptionIndex, _bUseSettingAppName: false);
	}

	private void SetupForm(string _strFormTitle, string _strSettingAppName, string _strSettingName, string _strWarningText, List<string> _WarningOptions, int _nDefaultOption, bool _bDisplayOKCancel, bool _bDisplayDoNotShow, int _nDoNotShowOptionIndex, bool _bUseSettingAppName)
	{
		InitializeComponent();
		if (string.Compare(_strWarningText, EPShared.LocalRM.GetString("PickViewOptionForMassMessage"), ignoreCase: true) == 0)
		{
			m_contextualHelpID = "HID_SDshowMassMode";
		}
		if (string.Compare(_strWarningText, EPShared.LocalRM.GetString("SoftTerrainFamilyNotLoadedMessage"), ignoreCase: true) == 0 || string.Compare(_strWarningText, EPShared.LocalRM.GetString("SideWalkFamilyNotLoadedMessage"), ignoreCase: true) == 0 || string.Compare(_strWarningText, EPShared.LocalRM.GetString("RetainingWallFamilyNotLoadedMessage"), ignoreCase: true) == 0 || string.Compare(_strWarningText, EPShared.LocalRM.GetString("StreetFamilyNotLoadedMessage"), ignoreCase: true) == 0 || string.Compare(_strWarningText, EPShared.LocalRM.GetString("CurbFamilyNotLoadedMessage"), ignoreCase: true) == 0 || string.Compare(_strWarningText, EPShared.LocalRM.GetString("ParkingLotFamilyNotLoadedMessage"), ignoreCase: true) == 0)
		{
			m_contextualHelpID = "HID_SDnoFamilyLoaded";
		}
		Text = _strFormTitle;
		if (ApplicationUtility.StringTable != null)
		{
			m_DoNotShowMessageAgainCheckBox.Text = EPShared.LocalRM.GetString("AMP_DoNotShowMessageDialog");
		}
		if (_bDisplayOKCancel)
		{
			if (ApplicationUtility.StringTable != null)
			{
				m_OKButton.Text = EPShared.LocalRM.GetString("AMP_OKDialog");
				m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
			}
		}
		else
		{
			if (ApplicationUtility.StringTable != null)
			{
				m_OKButton.Text = EPShared.LocalRM.GetString("AMP_CloseDialog");
			}
			m_CancelButton.Visible = false;
		}
		m_WarningMessageTextBox.Text = _strWarningText;
		m_strSettingAppName = _strSettingAppName;
		m_strSettingName = _strSettingName;
		m_nDefaultOption = _nDefaultOption;
		m_bDisplayDoNotShow = _bDisplayDoNotShow;
		m_nDoNotShowOptionIndex = _nDoNotShowOptionIndex;
		m_bUseSettingAppName = _bUseSettingAppName;
		m_WarningOptions = new List<RadioButton>();
		Point location = m_DoNotShowMessageAgainCheckBox.Location;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		RadioButton radioButton = null;
		for (num3 = 0; num3 < _WarningOptions.Count; num3++)
		{
			radioButton = new RadioButton();
			radioButton.AutoSize = true;
			radioButton.Location = location;
			radioButton.Name = "m_WarningOptions" + num3;
			radioButton.Size = new Size(60, 17);
			radioButton.TabIndex = m_DoNotShowMessageAgainCheckBox.TabIndex + num3;
			radioButton.TabStop = true;
			radioButton.Text = _WarningOptions[num3];
			radioButton.UseVisualStyleBackColor = true;
			Controls.Add(radioButton);
			m_WarningOptions.Add(radioButton);
			num2 += 23;
			location.Y += 23;
			if (radioButton.Size.Width > num)
			{
				num = radioButton.Size.Width;
			}
		}
		if (num > Size.Width + 24)
		{
			Size = new Size(num + 24, Size.Height);
			m_WarningMessageTextBox.Size = new Size(num, m_WarningMessageTextBox.Size.Height);
		}
		if (m_nDefaultOption >= 0 && m_nDefaultOption < m_WarningOptions.Count)
		{
			m_WarningOptions[m_nDefaultOption].Checked = true;
		}
		if (_bDisplayDoNotShow)
		{
			m_DoNotShowMessageAgainCheckBox.TabIndex += num3;
			m_DoNotShowMessageAgainCheckBox.Location = location;
		}
		else
		{
			m_DoNotShowMessageAgainCheckBox.Visible = false;
			num3--;
		}
		location = ((!_bDisplayOKCancel) ? m_CancelButton.Location : m_OKButton.Location);
		location.Y += num2;
		m_OKButton.TabIndex += num3;
		m_OKButton.Location = location;
		location = m_CancelButton.Location;
		location.Y += num2;
		m_CancelButton.TabIndex += num3;
		m_CancelButton.Location = location;
		Size size = Size;
		size.Height += num2;
		Size = size;
		InitializeControls();
	}

	private void InitializeControls()
	{
		int _nReturnValue = 0;
		bool flag = false;
		if (((m_bUseSettingAppName && ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingAppName, m_strSettingName + "WarningOption", ref _nReturnValue)) || (!m_bUseSettingAppName && ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingName + "WarningOption", ref _nReturnValue))) && _nReturnValue >= 0 && _nReturnValue < m_WarningOptions.Count)
		{
			m_WarningOptions[_nReturnValue].Checked = true;
			if (m_nDoNotShowOptionIndex == _nReturnValue)
			{
				flag = true;
			}
		}
		if (m_bDisplayDoNotShow || flag)
		{
			m_DoNotShowMessageAgainCheckBox.Checked = false;
			if ((m_bUseSettingAppName && ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingAppName, m_strSettingName + "DoNotShowAgainOption", ref _nReturnValue)) || (!m_bUseSettingAppName && ApplicationUtility.Settings.eplSetE_GetSetting(m_strSettingName + "DoNotShowAgainOption", ref _nReturnValue)))
			{
				m_DoNotShowMessageAgainCheckBox.Checked = Convert.ToBoolean(_nReturnValue);
			}
		}
	}

	private void SaveSettings()
	{
		bool flag = false;
		for (int i = 0; i < m_WarningOptions.Count; i++)
		{
			if (m_WarningOptions[i].Checked)
			{
				if (m_nDoNotShowOptionIndex != -1 || m_bUseSettingAppName)
				{
					ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingAppName, m_strSettingName + "WarningOption", i);
				}
				else
				{
					ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingName + "WarningOption", i);
				}
				if (m_nDoNotShowOptionIndex == i)
				{
					flag = true;
				}
				break;
			}
		}
		if (m_bDisplayDoNotShow)
		{
			if (m_bUseSettingAppName)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingAppName, m_strSettingName + "DoNotShowAgainOption", Convert.ToInt32(m_DoNotShowMessageAgainCheckBox.Checked));
			}
			else
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingName + "DoNotShowAgainOption", Convert.ToInt32(m_DoNotShowMessageAgainCheckBox.Checked));
			}
		}
		else if (flag)
		{
			if (m_bUseSettingAppName)
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingAppName, m_strSettingName + "DoNotShowAgainOption", 1);
			}
			else
			{
				ApplicationUtility.Settings.eplSetE_SetSetting(m_strSettingName + "DoNotShowAgainOption", 1);
			}
		}
	}

	private void ivokeHelp()
	{
		RevitUtility.ShowHelpInRevit(m_contextualHelpID);
	}

	private void OnHelpButtonClicked(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		ivokeHelp();
	}

	private void WarningOptionsForm_HelpRequested(object sender, HelpEventArgs hlpevent)
	{
		ivokeHelp();
		hlpevent.Handled = true;
	}

	private void OnOKButtonClick(object sender, EventArgs e)
	{
		SaveSettings();
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnWarningOptionsFormLoad(object sender, EventArgs e)
	{
		SiteDesigner.UI.WpfFocus.Focus(m_OKButton);
		if (m_DoNotShowMessageAgainCheckBox.Checked)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	public void SetDoNotShowOptionIndex(int _nIndex)
	{
		m_nDoNotShowOptionIndex = _nIndex;
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
		this.m_OKButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_DoNotShowMessageAgainCheckBox = new System.Windows.Forms.CheckBox();
		this.m_WarningMessageTextBox = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.m_OKButton.Location = new System.Drawing.Point(180, 91);
		this.m_OKButton.Name = "m_OKButton";
		this.m_OKButton.Size = new System.Drawing.Size(75, 23);
		this.m_OKButton.TabIndex = 2;
		this.m_OKButton.Text = "&OK";
		this.m_OKButton.UseVisualStyleBackColor = true;
		this.m_OKButton.Click += new System.EventHandler(this.OnOKButtonClick);
		this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CancelButton.Location = new System.Drawing.Point(261, 91);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(75, 23);
		this.m_CancelButton.TabIndex = 3;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_DoNotShowMessageAgainCheckBox.AutoSize = true;
		this.m_DoNotShowMessageAgainCheckBox.Location = new System.Drawing.Point(12, 69);
		this.m_DoNotShowMessageAgainCheckBox.Name = "m_DoNotShowMessageAgainCheckBox";
		this.m_DoNotShowMessageAgainCheckBox.Size = new System.Drawing.Size(160, 17);
		this.m_DoNotShowMessageAgainCheckBox.TabIndex = 1;
		this.m_DoNotShowMessageAgainCheckBox.Text = "&Do not show message again";
		this.m_DoNotShowMessageAgainCheckBox.UseVisualStyleBackColor = true;
		this.m_WarningMessageTextBox.BackColor = System.Drawing.SystemColors.Control;
		this.m_WarningMessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_WarningMessageTextBox.Location = new System.Drawing.Point(12, 12);
		this.m_WarningMessageTextBox.Multiline = true;
		this.m_WarningMessageTextBox.Name = "m_WarningMessageTextBox";
		this.m_WarningMessageTextBox.Size = new System.Drawing.Size(324, 51);
		this.m_WarningMessageTextBox.TabIndex = 0;
		this.m_WarningMessageTextBox.TabStop = false;
		base.AcceptButton = this.m_OKButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_CancelButton;
		base.ClientSize = new System.Drawing.Size(349, 127);
		base.Controls.Add(this.m_WarningMessageTextBox);
		base.Controls.Add(this.m_OKButton);
		base.Controls.Add(this.m_CancelButton);
		base.Controls.Add(this.m_DoNotShowMessageAgainCheckBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = SITES.Sites_Core.Resources.Siteworks;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "WarningOptionsForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "WarningOptionsForm";
		base.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnHelpButtonClicked);
		base.Load += new System.EventHandler(this.OnWarningOptionsFormLoad);
		base.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.WarningOptionsForm_HelpRequested);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
