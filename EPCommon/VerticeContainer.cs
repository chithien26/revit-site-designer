namespace EPCommon;

public class VerticeContainer
{
	private uint m_nVerticeNumber;

	private Eps_Point m_VerticePoint;

	public uint VerticeNumber => m_nVerticeNumber;

	public Eps_Point VerticePoint => m_VerticePoint;

	public VerticeContainer(uint _nVerticeNumber, Eps_Point _VerticePoint)
	{
		m_nVerticeNumber = _nVerticeNumber;
		m_VerticePoint = _VerticePoint;
	}
}
