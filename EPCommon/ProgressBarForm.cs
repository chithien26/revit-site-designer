using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SITES.Sites_Core;

namespace EPCommon;

public class ProgressBarForm : SiteDesigner.UI.WpfForm
{
	private ProgressBar m_ProgressBar;

	private string m_strMessage = string.Empty;

	private IContainer components;

	private Label ProgMessageBox;

	private Panel m_Panel;

	public string Message
	{
		get
		{
			return m_strMessage;
		}
		set
		{
			if (string.Compare(value, m_strMessage) != 0)
			{
				m_strMessage = value;
				ProgMessageBox.Text = m_strMessage;
				Height = 10 + ProgMessageBox.Height + 20 + m_ProgressBar.Height + 20;
				Focus();
			}
		}
	}

	public int MinValue
	{
		set
		{
			m_ProgressBar.MinValue = value;
			m_ProgressBar.Invalidate();
			Invalidate();
			Refresh();
		}
	}

	public int MaxValue
	{
		set
		{
			m_ProgressBar.MaxValue = value;
			m_ProgressBar.Invalidate();
			Invalidate();
			Refresh();
		}
	}

	public ProgressBarForm()
	{
		InitializeComponent();
		Text = EPShared.LocalRM.GetString("ProgressDialog");
		SetupProgressBar();
		StartPosition = FormStartPosition.CenterScreen;
		m_ProgressBar.Location = new Point(12, ProgMessageBox.Height + 20);
		m_ProgressBar.Width = ProgMessageBox.Width;
		Height = 10 + ProgMessageBox.Height + 20 + m_ProgressBar.Height + 20;
		m_ProgressBar.Value = 0;
	}

	public void ShowForm()
	{
		if (ApplicationUtility.RevitWindowWrapper != null)
		{
			Show(ApplicationUtility.RevitWindowWrapper);
		}
		else
		{
			Show();
		}
	}

	private void SetupProgressBar()
	{
		m_ProgressBar.BackColor = Color.Transparent;
		m_ProgressBar.BackgroundColor = Color.FromArgb(100, 201, 201, 201);
		m_ProgressBar.HighlightColor = Color.Orange;
		m_ProgressBar.StartColor = Color.FromArgb(0, 211, 40);
		m_ProgressBar.EndColor = Color.FromArgb(0, 211, 40);
		m_ProgressBar.GlowColor = Color.White;
		Invalidate();
		Refresh();
	}

	public void UpdateProgressBar(int _nValue)
	{
		m_ProgressBar.Value += _nValue;
		Invalidate();
		Refresh();
	}

	public void SetMaxValue()
	{
		m_ProgressBar.Value = m_ProgressBar.MaxValue - 1;
		Invalidate();
		Refresh();
	}

	public void ResetValue()
	{
		m_ProgressBar.Value = 0;
		Invalidate();
		Refresh();
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
		this.ProgMessageBox = new System.Windows.Forms.Label();
		this.m_ProgressBar = new EPCommon.ProgressBar();
		this.m_Panel = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.m_Panel.SuspendLayout();
		this.ProgMessageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ProgMessageBox.Location = new System.Drawing.Point(12, 6);
		this.ProgMessageBox.Name = "ProgMessageBox";
		this.ProgMessageBox.Size = new System.Drawing.Size(324, 20);
		this.ProgMessageBox.TabIndex = 1;
		this.m_ProgressBar.BackColor = System.Drawing.Color.Transparent;
		this.m_ProgressBar.Location = new System.Drawing.Point(0, 0);
		this.m_ProgressBar.Name = "m_ProgressBar";
		this.m_ProgressBar.Size = new System.Drawing.Size(264, 32);
		this.m_ProgressBar.TabIndex = 2;
		this.m_Panel.AutoSize = true;
		this.m_Panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.m_Panel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_Panel.Location = new System.Drawing.Point(0, 0);
		this.m_Panel.Name = "m_Panel";
		this.m_Panel.Size = new System.Drawing.Size(348, 90);
		this.m_Panel.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(348, 90);
		this.m_Panel.Controls.Add(this.ProgMessageBox);
		this.m_Panel.Controls.Add(this.m_ProgressBar);
		base.Controls.Add(this.m_Panel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = SITES.Sites_Core.Resources.Siteworks;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(354, 140);
		base.Name = "ProgressBarForm";
		base.ShowInTaskbar = false;
		this.Text = "Progress";
		this.m_Panel.ResumeLayout(false);
		this.m_Panel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
