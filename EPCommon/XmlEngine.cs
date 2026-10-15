using System;
using System.Xml;

namespace EPCommon;

public class XmlEngine
{
	private XmlTextReader m_XMLReader;

	private XmlElementInfo m_XMLData;

	private string m_strXMLFile = "";

	public string FileName => m_strXMLFile;

	public void Read(string _strXMLFile)
	{
		m_strXMLFile = _strXMLFile;
		try
		{
			m_XMLReader = new XmlTextReader(m_strXMLFile);
			m_XMLData = new XmlElementInfo();
			while (m_XMLReader.Read())
			{
				if (m_XMLReader.NodeType == XmlNodeType.Element)
				{
					ReadElement(ref m_XMLData);
				}
			}
			m_XMLReader.Close();
		}
		catch (Exception)
		{
		}
	}

	private void ReadElement(ref XmlElementInfo _ElementInfo)
	{
		_ElementInfo.Name = m_XMLReader.Name;
		if (m_XMLReader.HasAttributes)
		{
			for (int i = 0; i < m_XMLReader.AttributeCount; i++)
			{
				m_XMLReader.MoveToAttribute(i);
				XmlAttributeInfo xmlAttributeInfo = new XmlAttributeInfo(m_XMLReader.Name, m_XMLReader.Value);
				if (xmlAttributeInfo != null)
				{
					_ElementInfo.AddAttribute(xmlAttributeInfo);
				}
			}
			m_XMLReader.MoveToElement();
		}
		if (m_XMLReader.IsEmptyElement)
		{
			return;
		}
		while (m_XMLReader.Read() && m_XMLReader.NodeType != XmlNodeType.EndElement)
		{
			if (m_XMLReader.NodeType == XmlNodeType.Element)
			{
				XmlElementInfo _ElementInfo2 = new XmlElementInfo();
				ReadElement(ref _ElementInfo2);
				_ElementInfo.AddElement(_ElementInfo2);
			}
			else if (m_XMLReader.NodeType == XmlNodeType.Comment)
			{
				XmlElementInfo _CommentInfo = new XmlElementInfo();
				ReadComment(ref _CommentInfo);
				_ElementInfo.AddElement(_CommentInfo);
			}
		}
	}

	private void ReadComment(ref XmlElementInfo _CommentInfo)
	{
		_CommentInfo.IsComment = true;
		_CommentInfo.Name = m_XMLReader.Value;
	}

	public XmlElementInfo GetElement(string _strElement)
	{
		return GetElement(m_XMLData, _strElement);
	}

	public XmlElementInfo GetElement(XmlElementInfo _ElementInfo, string _strElement)
	{
		if (_ElementInfo != null)
		{
			if (string.Compare(_ElementInfo.Name, _strElement, ignoreCase: true) == 0)
			{
				return _ElementInfo;
			}
			for (int i = 0; i < _ElementInfo.ElementCount; i++)
			{
				if (string.Compare(((XmlElementInfo)_ElementInfo.Children[i]).Name, _strElement, ignoreCase: true) == 0)
				{
					return (XmlElementInfo)_ElementInfo.Children[i];
				}
			}
		}
		return null;
	}
}
