using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SITES.Sites_Core;

namespace EPCommon;

public class ErrorTreeListForm : SiteDesigner.UI.WpfForm
{
	private Document m_CurDocument;

	private IContainer components;

	private GroupBox m_MessagesGroupBox;

	private TreeView m_MessagesTreeView;

	private Button m_CloseButton;

	private Button m_ShowButton;

	public ErrorTreeListForm(Document _CurDocument, string _strFormTitle, TreeNode _CurrentNode)
	{
		InitializeComponent();
		Text = _strFormTitle;
		m_CloseButton.Text = EPShared.LocalRM.GetString("AMP_CloseDialog");
		m_ShowButton.Text = EPShared.LocalRM.GetString("AMP_ShowDialog");
		m_MessagesGroupBox.Text = EPShared.LocalRM.GetString("MessagesDialog");
		m_CurDocument = _CurDocument;
		m_MessagesTreeView.Nodes.Add(_CurrentNode);
		m_MessagesTreeView.ExpandAll();
	}

	private void OnCloseButtonClicked(object sender, EventArgs e)
	{
		DialogResult = DialogResult.OK;
	}

	private void OnMessagesTreeViewAfterCheck(object sender, TreeViewEventArgs e)
	{
		if (e.Node != null)
		{
			m_MessagesTreeView.AfterCheck -= OnMessagesTreeViewAfterCheck;
			SetChildTreeNodeSelectionState(e.Node);
			UnselectParentTreeNode(e.Node);
			if (m_MessagesTreeView.Nodes.Count > 0)
			{
				m_ShowButton.Enabled = AreAnyTreeNodesChecked(m_MessagesTreeView.Nodes[0]);
			}
			m_MessagesTreeView.AfterCheck += OnMessagesTreeViewAfterCheck;
		}
	}

	private void SetChildTreeNodeSelectionState(TreeNode _ParentTreeNode)
	{
		if (_ParentTreeNode.Nodes == null || _ParentTreeNode.Nodes.Count <= 0)
		{
			return;
		}
		foreach (TreeNode node in _ParentTreeNode.Nodes)
		{
			node.Checked = _ParentTreeNode.Checked;
			SetChildTreeNodeSelectionState(node);
		}
	}

	private void UnselectParentTreeNode(TreeNode _ChildTreeNode)
	{
		if (!_ChildTreeNode.Checked && _ChildTreeNode.Parent != null)
		{
			_ChildTreeNode.Parent.Checked = false;
			UnselectParentTreeNode(_ChildTreeNode.Parent);
		}
	}

	private List<Element> FindAllChildTreeNodeElements(TreeNode _ParentTreeNode)
	{
		List<Element> list = new List<Element>();
		if (_ParentTreeNode.Nodes != null && _ParentTreeNode.Nodes.Count > 0)
		{
			Element element = null;
			foreach (TreeNode node in _ParentTreeNode.Nodes)
			{
				if (node.Checked && node.Tag != null && node.Tag is Element item)
				{
					list.Add(item);
				}
				list.AddRange(FindAllChildTreeNodeElements(node));
			}
		}
		return list;
	}

	private bool AreAnyTreeNodesChecked(TreeNode _TreeNode)
	{
		if (_TreeNode.Nodes != null && _TreeNode.Nodes.Count > 0)
		{
			foreach (TreeNode node in _TreeNode.Nodes)
			{
				if (node.Checked || AreAnyTreeNodesChecked(node))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnShowButtonClick(object sender, EventArgs e)
	{
		List<Element> list = null;
		if (m_MessagesTreeView.Nodes.Count > 0)
		{
			list = FindAllChildTreeNodeElements(m_MessagesTreeView.Nodes[0]);
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		UIDocument uIDocument = new UIDocument(m_CurDocument);
		ElementSet elementSet = new ElementSet();
		foreach (Element item in list)
		{
			elementSet.Insert(item);
		}
		if (elementSet.Size > 0)
		{
			uIDocument.ShowElements(elementSet);
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
		this.m_MessagesGroupBox = new System.Windows.Forms.GroupBox();
		this.m_MessagesTreeView = new System.Windows.Forms.TreeView();
		this.m_CloseButton = new System.Windows.Forms.Button();
		this.m_ShowButton = new System.Windows.Forms.Button();
		this.m_MessagesGroupBox.SuspendLayout();
		base.SuspendLayout();
		this.m_MessagesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_MessagesGroupBox.Controls.Add(this.m_MessagesTreeView);
		this.m_MessagesGroupBox.Location = new System.Drawing.Point(12, 12);
		this.m_MessagesGroupBox.Name = "m_MessagesGroupBox";
		this.m_MessagesGroupBox.Size = new System.Drawing.Size(628, 316);
		this.m_MessagesGroupBox.TabIndex = 0;
		this.m_MessagesGroupBox.TabStop = false;
		this.m_MessagesGroupBox.Text = "Messages";
		this.m_MessagesTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_MessagesTreeView.CheckBoxes = true;
		this.m_MessagesTreeView.Location = new System.Drawing.Point(6, 19);
		this.m_MessagesTreeView.Name = "m_MessagesTreeView";
		this.m_MessagesTreeView.Size = new System.Drawing.Size(616, 291);
		this.m_MessagesTreeView.TabIndex = 0;
		this.m_MessagesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OnMessagesTreeViewAfterCheck);
		this.m_CloseButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_CloseButton.Location = new System.Drawing.Point(565, 334);
		this.m_CloseButton.Name = "m_CloseButton";
		this.m_CloseButton.Size = new System.Drawing.Size(75, 23);
		this.m_CloseButton.TabIndex = 2;
		this.m_CloseButton.Text = "&Close";
		this.m_CloseButton.UseVisualStyleBackColor = true;
		this.m_CloseButton.Click += new System.EventHandler(this.OnCloseButtonClicked);
		this.m_ShowButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_ShowButton.Enabled = false;
		this.m_ShowButton.Location = new System.Drawing.Point(484, 334);
		this.m_ShowButton.Name = "m_ShowButton";
		this.m_ShowButton.Size = new System.Drawing.Size(75, 23);
		this.m_ShowButton.TabIndex = 1;
		this.m_ShowButton.Text = "&Show";
		this.m_ShowButton.UseVisualStyleBackColor = true;
		this.m_ShowButton.Click += new System.EventHandler(this.OnShowButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(652, 369);
		base.Controls.Add(this.m_ShowButton);
		base.Controls.Add(this.m_CloseButton);
		base.Controls.Add(this.m_MessagesGroupBox);
		base.Icon = SITES.Sites_Core.Resources.Siteworks;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(260, 200);
		base.Name = "ErrorTreeListForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "ErrorTreeListForm";
		this.m_MessagesGroupBox.ResumeLayout(false);
		base.ResumeLayout(false);
		base.AcceptButton = this.m_ShowButton;
		base.CancelButton = this.m_CloseButton;
	}
}
