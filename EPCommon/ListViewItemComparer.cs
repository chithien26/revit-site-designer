using System.Collections;
using System.Windows.Forms;

namespace EPCommon;

public class ListViewItemComparer : IComparer
{
	private int m_nColumn;

	private SortOrder m_Order;

	public ListViewItemComparer()
	{
		m_nColumn = 0;
		m_Order = SortOrder.Ascending;
	}

	public ListViewItemComparer(int _Column, SortOrder _Order)
	{
		m_nColumn = _Column;
		m_Order = _Order;
	}

	public int Compare(object First, object Second)
	{
		int num = -1;
		num = string.Compare(((ListViewItem)First).SubItems[m_nColumn].Text, ((ListViewItem)Second).SubItems[m_nColumn].Text);
		if (m_Order == SortOrder.Descending)
		{
			num *= -1;
		}
		return num;
	}
}
