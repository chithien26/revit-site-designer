using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace EPCommon;

public class ImportData
{
	protected Document m_CurDocument;

	protected Document m_ActiveDoc;

	protected EPTransactionContainer m_TransactionContainer;

	protected bool m_bAbortTransaction;

	protected string m_strImportFolder;

	protected string m_strImportFileFullName;

	protected string m_strSaveAsFileName;

	public Document CurDocument => m_CurDocument;

	public string ImportFileFullName
	{
		get
		{
			return m_strImportFileFullName;
		}
		set
		{
			m_strImportFileFullName = value;
		}
	}

	public string SaveAsFileName
	{
		get
		{
			return m_strSaveAsFileName;
		}
		set
		{
			m_strSaveAsFileName = value;
		}
	}

	public string ImportFolder
	{
		get
		{
			return m_strImportFolder;
		}
		set
		{
			m_strImportFolder = value;
		}
	}

	public ImportData(Document _CurDocument)
	{
		m_bAbortTransaction = true;
		m_CurDocument = _CurDocument;
		string _strTemplateFileName = string.Empty;
		if (ManagePlants.FindTemplateFileName(m_CurDocument, ref _strTemplateFileName, _bIssueWarning: true))
		{
			m_ActiveDoc = m_CurDocument.Application.NewFamilyDocument(_strTemplateFileName);
			m_TransactionContainer = new EPTransactionContainer(m_ActiveDoc);
			Initialize();
		}
	}

	private void Initialize()
	{
		m_strImportFolder = RuntimePaths.AssemblyDirectory;
		m_strImportFileFullName = string.Empty;
	}

	public virtual bool Import()
	{
		if (m_strImportFileFullName == null)
		{
			throw new NullReferenceException();
		}
		return true;
	}

	public DialogResult CompleteTransaction()
	{
		DialogResult result = DialogResult.Cancel;
		m_ActiveDoc.Close(saveModified: false);
		if (!m_bAbortTransaction)
		{
			result = DialogResult.OK;
		}
		return result;
	}
}
