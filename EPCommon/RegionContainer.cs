using Autodesk.Revit.DB;

namespace EPCommon;

public class RegionContainer
{
	private string m_strRegionName;

	private Element m_RegionElement;

	private MeshTriangleList m_MeshTriangleList;

	public string RegionName
	{
		get
		{
			return m_strRegionName;
		}
		set
		{
			m_strRegionName = value;
		}
	}

	public Element RegionElement
	{
		get
		{
			return m_RegionElement;
		}
		set
		{
			m_RegionElement = value;
		}
	}

	public MeshTriangleList MeshTriangleList
	{
		get
		{
			return m_MeshTriangleList;
		}
		set
		{
			m_MeshTriangleList = value;
		}
	}

	public RegionContainer(string _strRegionName, Element _RegionElement)
	{
		m_strRegionName = _strRegionName;
		m_RegionElement = _RegionElement;
		m_MeshTriangleList = null;
	}
}
