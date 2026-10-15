using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace SITES;

public class DocumentViewContainer
{
	private Document m_Document;

	private List<View> m_ViewList;

	public Document Document => m_Document;

	public List<View> ViewList => m_ViewList;

	public DocumentViewContainer(Document _Document, List<View> _ViewList)
	{
		m_Document = _Document;
		m_ViewList = _ViewList;
	}
}
