namespace EPCommon;

public class MeshTriangleLine
{
	private int m_nTriangleNumber;

	private uint m_nLineStartNumber;

	private uint m_nLineEndNumber;

	public int TriangleNumber => m_nTriangleNumber;

	public uint LineStartNumber => m_nLineStartNumber;

	public uint LineEndNumber => m_nLineEndNumber;

	public MeshTriangleLine(int _nTriangleNumber, uint _nLineStartNumber, uint _nLineEndNumber)
	{
		m_nTriangleNumber = _nTriangleNumber;
		m_nLineStartNumber = _nLineStartNumber;
		m_nLineEndNumber = _nLineEndNumber;
	}

	public bool IsEqual(MeshTriangleLine _CompareLine)
	{
		bool result = false;
		if ((m_nLineStartNumber == _CompareLine.LineStartNumber && m_nLineEndNumber == _CompareLine.LineEndNumber) || (m_nLineEndNumber == _CompareLine.LineStartNumber && m_nLineStartNumber == _CompareLine.LineEndNumber))
		{
			result = true;
		}
		return result;
	}
}
