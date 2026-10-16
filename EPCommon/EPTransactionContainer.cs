using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace EPCommon;

public class EPTransactionContainer : System.IDisposable
{
	private Document m_Document;

	private Transaction m_Transaction;

	private SubTransaction m_SubTransaction;

	private FailurePreprocessor m_FailurePreprocessor;

	private TopographyEditScope m_TopographyEditScope;

	private TransactionGroup m_TransactionGroup;
	private bool m_Disposed;

	public EPTransactionContainer(Document _Document)
	{
		m_Document = _Document;
		m_Transaction = new Transaction(_Document);
		m_SubTransaction = new SubTransaction(_Document);
	}

	public void Dispose()
	{
		if (m_Disposed) return;
		if (m_SubTransaction?.GetStatus() == TransactionStatus.Started) m_SubTransaction.RollBack();
		if (m_Transaction?.GetStatus() == TransactionStatus.Started) m_Transaction.RollBack();
		m_TopographyEditScope?.Dispose();
		m_TopographyEditScope = null;
		if (m_TransactionGroup?.GetStatus() == TransactionStatus.Started) m_TransactionGroup.RollBack();
		m_SubTransaction?.Dispose();
		m_Transaction?.Dispose();
		m_TransactionGroup?.Dispose();
		m_Disposed = true;
	}

	public bool Start()
	{
		return Start(string.Empty);
	}

	public bool Start(string _strTransactionName)
	{
		if (m_Transaction != null && !m_Transaction.HasStarted())
		{
			TransactionStatus transactionStatus = ((!(_strTransactionName != string.Empty)) ? m_Transaction.Start() : m_Transaction.Start(_strTransactionName));
			if (transactionStatus == TransactionStatus.Started)
			{
				return true;
			}
		}
		return false;
	}

	public bool Commit()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted())
		{
			try { TerrainApi.FlushEdits(m_Document); }
			catch
			{
				m_Transaction.RollBack();
				DisposeTopographyEditScope();
				throw;
			}
			FailureHandlingOptions(BuiltInFailures.OverlapFailures.DuplicateInstances);
			TransactionStatus transactionStatus = m_Transaction.Commit();
			if (transactionStatus == TransactionStatus.Committed)
			{
				return true;
			}
#if !REVIT2022 && !REVIT2023
			if (m_TopographyEditScope != null)
			{
				DisposeTopographyEditScope();
				throw new System.InvalidOperationException("Revit rolled back the terrain changes.");
			}
#endif
		}
		return false;
	}

	public bool Rollback()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted())
		{
			TransactionStatus transactionStatus = m_Transaction.RollBack();
			if (transactionStatus == TransactionStatus.RolledBack)
			{
				return true;
			}
		}
		return false;
	}

	public bool StartTopographyEditScope(string transName, ElementId topoID)
	{
#if !REVIT2022 && !REVIT2023
		if (m_TopographyEditScope?.IsActive == true || m_Document.IsModifiable)
			throw new System.InvalidOperationException("Commit the current transaction before starting a Toposolid edit scope.");
#endif
		try
		{
			if (m_TopographyEditScope == null)
			{
				m_TopographyEditScope = new TopographyEditScope(m_Document, transName);
			}
			if (!m_TopographyEditScope.IsActive && m_TopographyEditScope.IsPermitted)
			{
				m_TopographyEditScope.Start(topoID);
			}
			return true;
		}
		catch
		{
#if !REVIT2022 && !REVIT2023
			m_TopographyEditScope?.Dispose();
			m_TopographyEditScope = null;
			throw;
#else
			return false;
#endif
		}
	}

	public bool CommitTopographyEditScope(ref TopographyEditFailuresPreprocessor failuresPreprocessor)
	{
		try
		{
			if (m_TopographyEditScope != null && m_TopographyEditScope.IsActive)
			{
				m_TopographyEditScope.Commit(failuresPreprocessor);
				m_TopographyEditScope.Dispose();
				m_TopographyEditScope = null;
			}
			return true;
		}
		catch
		{
#if !REVIT2022 && !REVIT2023
			m_TopographyEditScope?.Dispose();
			m_TopographyEditScope = null;
			throw;
#else
			return false;
#endif
		}
	}

	public bool DisposeTopographyEditScope()
	{
		try
		{
			if (m_TopographyEditScope != null
#if REVIT2022 || REVIT2023
				&& !m_TopographyEditScope.IsActive
#endif
				)
			{
				m_TopographyEditScope.Dispose();
				m_TopographyEditScope = null;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool StartTransactionGroup(string transName)
	{
		try
		{
			if (m_TransactionGroup == null)
			{
				m_TransactionGroup = new TransactionGroup(m_Document, transName);
			}
			if (!m_TransactionGroup.HasStarted())
			{
				string text = m_TransactionGroup.Start().ToString();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool CommitTransactionGroup()
	{
		try
		{
			if (m_TransactionGroup != null && m_TransactionGroup.HasStarted())
			{
				m_TransactionGroup.Commit();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool AssimilateTransactionGroup()
	{
		try
		{
			if (m_TransactionGroup != null && m_TransactionGroup.HasStarted())
			{
				m_TransactionGroup.Assimilate();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool RollbackTransactionGroup()
	{
		try
		{
			if (m_TransactionGroup != null && m_TransactionGroup.HasStarted())
			{
				m_TransactionGroup.RollBack();
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool BeginTransaction()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted() && m_SubTransaction != null && !m_SubTransaction.HasStarted())
		{
			TransactionStatus transactionStatus = m_SubTransaction.Start();
			if (transactionStatus == TransactionStatus.Started)
			{
				return true;
			}
		}
		return false;
	}

	public bool EndTransaction()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted() && m_SubTransaction != null && !m_SubTransaction.HasEnded())
		{
			TransactionStatus transactionStatus = m_SubTransaction.Commit();
			if (transactionStatus == TransactionStatus.Committed)
			{
				return true;
			}
		}
		return false;
	}

	public void AbortTransaction()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted() && m_SubTransaction != null && !m_SubTransaction.HasEnded())
		{
			m_SubTransaction.RollBack();
		}
	}

	public string GetName()
	{
		if (m_Transaction != null)
		{
			return m_Transaction.GetName();
		}
		return string.Empty;
	}

	public void SetName(string _strTransactionName)
	{
		if (m_Transaction != null && _strTransactionName != string.Empty && string.Compare(_strTransactionName, m_Transaction.GetName(), ignoreCase: true) != 0)
		{
			m_Transaction.SetName(_strTransactionName);
		}
	}

	public bool HasStarted()
	{
		if (m_Transaction != null && !m_Transaction.HasEnded() && m_Transaction.HasStarted())
		{
			return true;
		}
		return false;
	}

	public void FailureHandlingOptions(List<FailureDefinitionId> FailureDefinitionIdList)
	{
		if (m_Transaction == null || m_Transaction.HasEnded() || !m_Transaction.HasStarted())
		{
			return;
		}
		if (m_FailurePreprocessor == null)
		{
			m_FailurePreprocessor = new FailurePreprocessor(FailureDefinitionIdList);
		}
		else
		{
			if (m_FailurePreprocessor.FailureDefinitionIdList == null)
			{
				m_FailurePreprocessor.FailureDefinitionIdList = new List<FailureDefinitionId>();
			}
			foreach (FailureDefinitionId FailureDefinitionId in FailureDefinitionIdList)
			{
				if (!CheckDuplicateId(FailureDefinitionId))
				{
					m_FailurePreprocessor.FailureDefinitionIdList.Add(FailureDefinitionId);
				}
			}
		}
		FailureHandlingOptions failureHandlingOptions = m_Transaction.GetFailureHandlingOptions();
		failureHandlingOptions.SetFailuresPreprocessor(m_FailurePreprocessor);
		m_Transaction.SetFailureHandlingOptions(failureHandlingOptions);
	}

	public void FailureHandlingOptions(FailureDefinitionId FailureDefinitionId)
	{
		if (m_Transaction == null || m_Transaction.HasEnded() || !m_Transaction.HasStarted())
		{
			return;
		}
		if (m_FailurePreprocessor == null)
		{
			m_FailurePreprocessor = new FailurePreprocessor(FailureDefinitionId);
		}
		else
		{
			if (m_FailurePreprocessor.FailureDefinitionIdList == null)
			{
				m_FailurePreprocessor.FailureDefinitionIdList = new List<FailureDefinitionId>();
			}
			if (!CheckDuplicateId(FailureDefinitionId))
			{
				m_FailurePreprocessor.FailureDefinitionIdList.Add(FailureDefinitionId);
			}
		}
		FailureHandlingOptions failureHandlingOptions = m_Transaction.GetFailureHandlingOptions();
		failureHandlingOptions.SetFailuresPreprocessor(m_FailurePreprocessor);
		m_Transaction.SetFailureHandlingOptions(failureHandlingOptions);
	}

	private bool CheckDuplicateId(FailureDefinitionId _Id)
	{
		foreach (FailureDefinitionId failureDefinitionId in m_FailurePreprocessor.FailureDefinitionIdList)
		{
			if (_Id == failureDefinitionId)
			{
				return true;
			}
		}
		return false;
	}
}
