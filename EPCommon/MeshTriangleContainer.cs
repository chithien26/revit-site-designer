using System.Collections.Generic;

namespace EPCommon;

public class MeshTriangleContainer
{
	private List<MeshTriangleLine> m_TriangleLines;

	public List<MeshTriangleLine> TriangleLines => m_TriangleLines;

	public MeshTriangleContainer()
	{
		m_TriangleLines = new List<MeshTriangleLine>();
	}
}
