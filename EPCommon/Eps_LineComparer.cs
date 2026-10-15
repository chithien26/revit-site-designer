using System.Collections.Generic;

namespace EPCommon;

public class Eps_LineComparer : IComparer<Eps_Line>
{
	int IComparer<Eps_Line>.Compare(Eps_Line first, Eps_Line second)
	{
		Eps_Line eps_Line = null;
		Eps_Line eps_Line2 = null;
		int num = 0;
		if (first != null)
		{
			if (second != null)
			{
				if (first.StartPnt.x < first.EndPnt.x)
				{
					eps_Line = first;
				}
				else if (first.StartPnt.x > first.EndPnt.x)
				{
					eps_Line = first.Reverse();
				}
				else
				{
					eps_Line = ((!(first.StartPnt.y < first.EndPnt.y)) ? first.Reverse() : first);
				}
				if (second.StartPnt.x < second.EndPnt.x)
				{
					eps_Line2 = second;
				}
				else if (second.StartPnt.x > second.EndPnt.x)
				{
					eps_Line2 = second.Reverse();
				}
				else
				{
					eps_Line2 = ((!(second.StartPnt.y < second.EndPnt.y)) ? second.Reverse() : second);
				}
				num = eps_Line.StartPnt.x.CompareTo(eps_Line2.StartPnt.x);
				if (num == 0)
				{
					num = eps_Line.StartPnt.y.CompareTo(eps_Line2.StartPnt.y);
					if (num == 0)
					{
						num = eps_Line.EndPnt.x.CompareTo(eps_Line2.EndPnt.x);
						if (num == 0)
						{
							num = eps_Line.EndPnt.y.CompareTo(eps_Line2.EndPnt.y);
						}
					}
				}
			}
			else
			{
				num = -1;
			}
		}
		else if (second != null)
		{
			num = 1;
		}
		return num;
	}
}
