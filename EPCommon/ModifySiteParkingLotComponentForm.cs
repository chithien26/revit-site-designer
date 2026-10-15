using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EPCommon;

public class ModifySiteParkingLotComponentForm : EPForm
{
	private SiteAlignmentAction m_PerformAction;

	private IContainer components;

	private Button m_MoveButton;

	private Button m_RotateButton;

	private Button m_CancelButton;

	private Button m_ApplyButton;

	private Button m_ChangeElevationButton;

	private Button m_AddVertexButton;

	private Button m_DeleteVertexButton;

	private Button m_DeleteButton;

	private Button m_ChangeTypeButton;

	private TextBox m_MessageTextBox;

	private Button m_DeleteBreaklineButton;

	private Button m_AddBreaklineButton;

	private Button m_ChangeViewScaleButton;

	public SiteAlignmentAction PerformAction => m_PerformAction;

	public ModifySiteParkingLotComponentForm()
	{
		InitializeComponent();
		Text = EPShared.LocalRM.GetString("ModifyParkingLotDialog");
		m_MessageTextBox.Text = EPShared.LocalRM.GetString("ModifySiteComponentInformationDialog");
		m_MoveButton.Text = EPShared.LocalRM.GetString("AMP_MoveDialog");
		m_RotateButton.Text = EPShared.LocalRM.GetString("AMP_RotateDialog");
		m_AddVertexButton.Text = EPShared.LocalRM.GetString("Add_AMP_VertexDialog");
		m_DeleteVertexButton.Text = EPShared.LocalRM.GetString("D_AMP_eleteVertexDialog");
		m_ChangeElevationButton.Text = EPShared.LocalRM.GetString("AMP_ChangeElevationDialog");
		m_ChangeTypeButton.Text = EPShared.LocalRM.GetString("C_AMP_hangeTypeDialog");
		m_AddBreaklineButton.Text = EPShared.LocalRM.GetString("Add_AMP_BreaklineDialog");
		m_DeleteBreaklineButton.Text = EPShared.LocalRM.GetString("De_AMP_leteBreaklineDialog");
		m_DeleteButton.Text = EPShared.LocalRM.GetString("AMP_DeleteDialog");
		m_ChangeViewScaleButton.Text = EPShared.LocalRM.GetString("ChangeView_AMP_ScaleDialog");
		m_ApplyButton.Text = EPShared.LocalRM.GetString("AMP_ApplyDialog");
		m_CancelButton.Text = EPShared.LocalRM.GetString("AMP_CancelDialog");
		ContextualHelpID = "HID_SDparkinglotModify";
		InitializeControls();
	}

	private void InitializeControls()
	{
		GetDialogPosition();
	}

	private void OnMoveButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.Move;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnRotateButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.Rotate;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnChangeElevationButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.ChangeElevationGrade;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnAddVertexButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.AddVertex;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnDeleteVertexButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.DeleteVertex;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnDeleteButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.Delete;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnChangeTypeButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.ChangeType;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnAddBreaklineButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.AddBreakline;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnDeleteBreaklineButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.DeleteBreakline;
		DialogResult = DialogResult.Retry;
		Close();
	}

	private void OnApplyButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}

	private void OnCancelButtonClick(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnModifySiteComponentClosing(object sender, FormClosingEventArgs e)
	{
		SetDialogPosition();
	}

	private void OnChangeViewScaleButtonClick(object sender, EventArgs e)
	{
		m_PerformAction = SiteAlignmentAction.ChangeViewScale;
		DialogResult = DialogResult.Retry;
		Close();
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
		this.m_MoveButton = new System.Windows.Forms.Button();
		this.m_RotateButton = new System.Windows.Forms.Button();
		this.m_CancelButton = new System.Windows.Forms.Button();
		this.m_ApplyButton = new System.Windows.Forms.Button();
		this.m_ChangeElevationButton = new System.Windows.Forms.Button();
		this.m_AddVertexButton = new System.Windows.Forms.Button();
		this.m_DeleteVertexButton = new System.Windows.Forms.Button();
		this.m_DeleteButton = new System.Windows.Forms.Button();
		this.m_ChangeTypeButton = new System.Windows.Forms.Button();
		this.m_MessageTextBox = new System.Windows.Forms.TextBox();
		this.m_DeleteBreaklineButton = new System.Windows.Forms.Button();
		this.m_AddBreaklineButton = new System.Windows.Forms.Button();
		this.m_ChangeViewScaleButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_MoveButton.Location = new System.Drawing.Point(12, 59);
		this.m_MoveButton.Name = "m_MoveButton";
		this.m_MoveButton.Size = new System.Drawing.Size(132, 23);
		this.m_MoveButton.TabIndex = 1;
		this.m_MoveButton.Text = "&Move";
		this.m_MoveButton.UseVisualStyleBackColor = true;
		this.m_MoveButton.Click += new System.EventHandler(this.OnMoveButtonClick);
		this.m_RotateButton.Location = new System.Drawing.Point(150, 59);
		this.m_RotateButton.Name = "m_RotateButton";
		this.m_RotateButton.Size = new System.Drawing.Size(132, 23);
		this.m_RotateButton.TabIndex = 2;
		this.m_RotateButton.Text = "&Rotate";
		this.m_RotateButton.UseVisualStyleBackColor = true;
		this.m_RotateButton.Click += new System.EventHandler(this.OnRotateButtonClick);
		this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_CancelButton.Location = new System.Drawing.Point(206, 204);
		this.m_CancelButton.Name = "m_CancelButton";
		this.m_CancelButton.Size = new System.Drawing.Size(76, 23);
		this.m_CancelButton.TabIndex = 12;
		this.m_CancelButton.Text = "&Cancel";
		this.m_CancelButton.UseVisualStyleBackColor = true;
		this.m_CancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
		this.m_ApplyButton.Location = new System.Drawing.Point(125, 204);
		this.m_ApplyButton.Name = "m_ApplyButton";
		this.m_ApplyButton.Size = new System.Drawing.Size(75, 23);
		this.m_ApplyButton.TabIndex = 11;
		this.m_ApplyButton.Text = "&Apply";
		this.m_ApplyButton.UseVisualStyleBackColor = true;
		this.m_ApplyButton.Click += new System.EventHandler(this.OnApplyButtonClick);
		this.m_ChangeElevationButton.Location = new System.Drawing.Point(12, 117);
		this.m_ChangeElevationButton.Name = "m_ChangeElevationButton";
		this.m_ChangeElevationButton.Size = new System.Drawing.Size(132, 23);
		this.m_ChangeElevationButton.TabIndex = 5;
		this.m_ChangeElevationButton.Text = "Change Elevation";
		this.m_ChangeElevationButton.UseVisualStyleBackColor = true;
		this.m_ChangeElevationButton.Click += new System.EventHandler(this.OnChangeElevationButtonClick);
		this.m_AddVertexButton.Location = new System.Drawing.Point(13, 88);
		this.m_AddVertexButton.Name = "m_AddVertexButton";
		this.m_AddVertexButton.Size = new System.Drawing.Size(131, 23);
		this.m_AddVertexButton.TabIndex = 3;
		this.m_AddVertexButton.Text = "Add Vertex";
		this.m_AddVertexButton.UseVisualStyleBackColor = true;
		this.m_AddVertexButton.Click += new System.EventHandler(this.OnAddVertexButtonClick);
		this.m_DeleteVertexButton.Location = new System.Drawing.Point(150, 88);
		this.m_DeleteVertexButton.Name = "m_DeleteVertexButton";
		this.m_DeleteVertexButton.Size = new System.Drawing.Size(132, 23);
		this.m_DeleteVertexButton.TabIndex = 4;
		this.m_DeleteVertexButton.Text = "Delete Vertex";
		this.m_DeleteVertexButton.UseVisualStyleBackColor = true;
		this.m_DeleteVertexButton.Click += new System.EventHandler(this.OnDeleteVertexButtonClick);
		this.m_DeleteButton.Location = new System.Drawing.Point(13, 175);
		this.m_DeleteButton.Name = "m_DeleteButton";
		this.m_DeleteButton.Size = new System.Drawing.Size(131, 23);
		this.m_DeleteButton.TabIndex = 9;
		this.m_DeleteButton.Text = "Delete";
		this.m_DeleteButton.UseVisualStyleBackColor = true;
		this.m_DeleteButton.Click += new System.EventHandler(this.OnDeleteButtonClick);
		this.m_ChangeTypeButton.Location = new System.Drawing.Point(150, 117);
		this.m_ChangeTypeButton.Name = "m_ChangeTypeButton";
		this.m_ChangeTypeButton.Size = new System.Drawing.Size(132, 23);
		this.m_ChangeTypeButton.TabIndex = 6;
		this.m_ChangeTypeButton.Text = "Change Type";
		this.m_ChangeTypeButton.UseVisualStyleBackColor = true;
		this.m_ChangeTypeButton.Click += new System.EventHandler(this.OnChangeTypeButtonClick);
		this.m_MessageTextBox.BackColor = System.Drawing.SystemColors.Control;
		this.m_MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_MessageTextBox.Location = new System.Drawing.Point(13, 11);
		this.m_MessageTextBox.Multiline = true;
		this.m_MessageTextBox.Name = "m_MessageTextBox";
		this.m_MessageTextBox.Size = new System.Drawing.Size(269, 42);
		this.m_MessageTextBox.TabIndex = 0;
		this.m_MessageTextBox.TabStop = false;
		this.m_MessageTextBox.Text = "Please select the edit type you want to do. Press ESC or right-click and Cancel to complete the action.";
		this.m_DeleteBreaklineButton.Location = new System.Drawing.Point(150, 146);
		this.m_DeleteBreaklineButton.Name = "m_DeleteBreaklineButton";
		this.m_DeleteBreaklineButton.Size = new System.Drawing.Size(132, 23);
		this.m_DeleteBreaklineButton.TabIndex = 8;
		this.m_DeleteBreaklineButton.Text = "Delete Breakline";
		this.m_DeleteBreaklineButton.UseVisualStyleBackColor = true;
		this.m_DeleteBreaklineButton.Click += new System.EventHandler(this.OnDeleteBreaklineButtonClick);
		this.m_AddBreaklineButton.Location = new System.Drawing.Point(12, 146);
		this.m_AddBreaklineButton.Name = "m_AddBreaklineButton";
		this.m_AddBreaklineButton.Size = new System.Drawing.Size(132, 23);
		this.m_AddBreaklineButton.TabIndex = 7;
		this.m_AddBreaklineButton.Text = "Add Breakline";
		this.m_AddBreaklineButton.UseVisualStyleBackColor = true;
		this.m_AddBreaklineButton.Click += new System.EventHandler(this.OnAddBreaklineButtonClick);
		this.m_ChangeViewScaleButton.Location = new System.Drawing.Point(150, 175);
		this.m_ChangeViewScaleButton.Name = "m_ChangeViewScaleButton";
		this.m_ChangeViewScaleButton.Size = new System.Drawing.Size(132, 23);
		this.m_ChangeViewScaleButton.TabIndex = 10;
		this.m_ChangeViewScaleButton.Text = "Change View Scale";
		this.m_ChangeViewScaleButton.UseVisualStyleBackColor = true;
		this.m_ChangeViewScaleButton.Click += new System.EventHandler(this.OnChangeViewScaleButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(294, 239);
		base.Controls.Add(this.m_ChangeViewScaleButton);
		base.Controls.Add(this.m_DeleteBreaklineButton);
		base.Controls.Add(this.m_AddBreaklineButton);
		base.Controls.Add(this.m_MessageTextBox);
		base.Controls.Add(this.m_ChangeTypeButton);
		base.Controls.Add(this.m_DeleteButton);
		base.Controls.Add(this.m_DeleteVertexButton);
		base.Controls.Add(this.m_AddVertexButton);
		base.Controls.Add(this.m_ChangeElevationButton);
		base.Controls.Add(this.m_CancelButton);
		base.Controls.Add(this.m_ApplyButton);
		base.Controls.Add(this.m_RotateButton);
		base.Controls.Add(this.m_MoveButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "ModifySiteParkingLotComponentForm";
		this.Text = "Modify Site Component";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnModifySiteComponentClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
		base.AcceptButton = this.m_ApplyButton;
		base.CancelButton = this.m_CancelButton;
	}
}
