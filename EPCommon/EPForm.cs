using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using SITES.Sites_Core;

namespace EPCommon;

public class EPForm : SiteDesigner.UI.WpfForm
{
	private FormBorderStyle m_FormBorderStyle;

	private ToolTip m_EPToolTip = new ToolTip();

	private bool m_bOverrideKeyUp;

	private string m_contextualHelpID = "HID_SDaaa";

	private IContainer components;

	private HelpProvider m_HelpProvider;

	public string ContextualHelpID
	{
		set
		{
			m_contextualHelpID = value;
		}
	}

	public ToolTip EPToolTip
	{
		get
		{
			return m_EPToolTip;
		}
		set
		{
			m_EPToolTip = value;
		}
	}

	public EPForm()
	{
		InitializeComponent();
		m_EPToolTip.AutoPopDelay = 5000;
		m_EPToolTip.InitialDelay = 1000;
		m_EPToolTip.ReshowDelay = 500;
		m_EPToolTip.ShowAlways = true;
		ShowInTaskbar = false;
	}

	private void OnFormLoad(object sender, EventArgs e)
	{
		m_FormBorderStyle = FormBorderStyle;
		MinimizeBox = false;
		MaximizeBox = false;
		HelpButton = true;
	}

	private void OnHelpButtonClicked(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		RevitUtility.ShowHelpInRevit(m_contextualHelpID);
	}

	private void EPForm_HelpRequested(object sender, HelpEventArgs hlpevent)
	{
		RevitUtility.ShowHelpInRevit(m_contextualHelpID);
	}

	public void GetDialogPosition()
	{
		m_FormBorderStyle = FormBorderStyle;
		Point location = Screen.PrimaryScreen.Bounds.Location;
		Location = new Point(location.X + 30, location.Y + 30);
		StartPosition = FormStartPosition.Manual;
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(ApplicationUtility.CurrentUserRegistryKey + "\\" + Text);
		if (registryKey == null)
		{
			return;
		}
		if (registryKey.GetValue("X") != null && registryKey.GetValue("Y") != null && (int)registryKey.GetValue("X") >= 0 && (int)registryKey.GetValue("Y") >= 0)
		{
			Location = new Point((int)registryKey.GetValue("X"), (int)registryKey.GetValue("Y"));
			bool flag = false;
			Screen[] allScreens = Screen.AllScreens;
			for (int i = 0; i < allScreens.Length; i++)
			{
				if (Location.X >= allScreens[i].Bounds.Left && Location.X <= allScreens[i].Bounds.Right && Location.Y >= allScreens[i].Bounds.Top && Location.Y <= allScreens[i].Bounds.Bottom)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Location = new Point(location.X + 30, location.Y + 30);
				SetDialogPosition();
			}
		}
		if ((m_FormBorderStyle == FormBorderStyle.Sizable || m_FormBorderStyle == FormBorderStyle.SizableToolWindow) && registryKey.GetValue("Height") != null && registryKey.GetValue("Width") != null && (int)registryKey.GetValue("Width") >= MinimumSize.Width && (int)registryKey.GetValue("Height") >= MinimumSize.Height)
		{
			Size = new Size((int)registryKey.GetValue("Width"), (int)registryKey.GetValue("Height"));
		}
		registryKey.Close();
	}

	public void SetDialogPosition()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(ApplicationUtility.CurrentUserRegistryKey + Text, writable: true);
		if (registryKey == null)
		{
			registryKey = Registry.CurrentUser.CreateSubKey(ApplicationUtility.CurrentUserRegistryKey + "\\" + Text);
		}
		if (registryKey != null)
		{
			registryKey.SetValue("X", Location.X);
			registryKey.SetValue("Y", Location.Y);
			if (m_FormBorderStyle == FormBorderStyle.Sizable || m_FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				registryKey.SetValue("Width", Size.Width);
				registryKey.SetValue("Height", Size.Height);
			}
			registryKey.Close();
		}
	}

	private void OnFormKeyUp(object sender, KeyEventArgs e)
	{
		if (!m_bOverrideKeyUp)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
		else
		{
			m_bOverrideKeyUp = false;
		}
	}

	public void OverrideKeyUp()
	{
		m_bOverrideKeyUp = true;
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
		this.m_HelpProvider = new System.Windows.Forms.HelpProvider();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(292, 266);
		base.HelpButton = true;
		this.m_HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.KeywordIndex);
		base.Icon = SITES.Sites_Core.Resources.Siteworks;
		base.KeyPreview = true;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "EPForm";
		this.m_HelpProvider.SetShowHelp(this, false);
		base.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnHelpButtonClicked);
		base.Load += new System.EventHandler(this.OnFormLoad);
		base.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.EPForm_HelpRequested);
		base.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFormKeyUp);
		base.ResumeLayout(false);
	}
}
