using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using SITES.Properties;

namespace EPCommon;

public class AboutForm : EPForm
{
	private IContainer components;

	private Label m_ProductForRevitLabel;

	private Label m_VersionLabel;

	private Label m_CopyrightLabel;

	private PictureBox pictureBox1;

	private RichTextBox richTextBox1;

	public AboutForm()
	{
		InitializeComponent();
		ContextualHelpID = "HID_SDabout";
		Text = EPShared.LocalRM.GetString("AboutSiteDesignerForRevitDialog");
		m_ProductForRevitLabel.Text = GetAssemblyDescription();
		object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			m_CopyrightLabel.Text = ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
		}
		else
		{
			m_CopyrightLabel.Text = string.Format(EPShared.LocalRM.GetString("CopyrightDialog"), DateTime.Now.Year);
		}
		string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		m_VersionLabel.Text = string.IsNullOrWhiteSpace(version)
			? EPShared.LocalRM.GetString("VersionDialog") + " (dev)"
			: EPShared.LocalRM.GetString("VersionDialog") + " " + version;
		GetDialogPosition();
	}

	private void OnCloseButtton(object sender, EventArgs e)
	{
		SetDialogPosition();
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCheckForUpdatesLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		ApplicationUtility.VerifyProductVersion(_bDailyVersionCheck: false);
	}

	private string GetAssemblyDescription()
	{
		Assembly assembly = GetType().Assembly;
		Attribute[] customAttributes = Attribute.GetCustomAttributes(assembly);
		foreach (Attribute attribute in customAttributes)
		{
			if (attribute.GetType() == typeof(AssemblyDescriptionAttribute))
			{
				return (attribute as AssemblyDescriptionAttribute).Description;
			}
		}
		return string.Empty;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EPCommon.AboutForm));
		this.m_ProductForRevitLabel = new System.Windows.Forms.Label();
		this.m_VersionLabel = new System.Windows.Forms.Label();
		this.m_CopyrightLabel = new System.Windows.Forms.Label();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.richTextBox1 = new System.Windows.Forms.RichTextBox();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.m_ProductForRevitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_ProductForRevitLabel.Location = new System.Drawing.Point(17, 12);
		this.m_ProductForRevitLabel.Name = "m_ProductForRevitLabel";
		this.m_ProductForRevitLabel.Size = new System.Drawing.Size(240, 60);
		this.m_ProductForRevitLabel.TabIndex = 0;
		this.m_ProductForRevitLabel.Text = "Product Label";
		this.m_VersionLabel.AutoSize = true;
		this.m_VersionLabel.Location = new System.Drawing.Point(14, 96);
		this.m_VersionLabel.Name = "m_VersionLabel";
		this.m_VersionLabel.Size = new System.Drawing.Size(42, 13);
		this.m_VersionLabel.TabIndex = 1;
		this.m_VersionLabel.Text = "Version";
		this.m_CopyrightLabel.AutoSize = true;
		this.m_CopyrightLabel.Location = new System.Drawing.Point(14, 119);
		this.m_CopyrightLabel.Name = "m_CopyrightLabel";
		this.m_CopyrightLabel.Size = new System.Drawing.Size(51, 13);
		this.m_CopyrightLabel.TabIndex = 2;
		this.m_CopyrightLabel.Text = "Copyright";
		this.pictureBox1.Image = SITES.Properties.Resources.AutodeskLogo;
		this.pictureBox1.Location = new System.Drawing.Point(291, 94);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(119, 20);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 3;
		this.pictureBox1.TabStop = false;
		this.richTextBox1.Location = new System.Drawing.Point(12, 139);
		this.richTextBox1.Name = "richTextBox1";
		this.richTextBox1.Size = new System.Drawing.Size(394, 175);
		this.richTextBox1.TabIndex = 5;
		this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(419, 326);
		base.Controls.Add(this.richTextBox1);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.m_CopyrightLabel);
		base.Controls.Add(this.m_VersionLabel);
		base.Controls.Add(this.m_ProductForRevitLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.HelpButton = false;
		base.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		base.Name = "AboutForm";
		this.Text = "About LANDCADD for Revit";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
