using Autodesk.Revit.DB;

namespace EPCommon;

public static class MathUtil
{
	public static XYZ[] MultiCross(XYZ[] m1, XYZ[] m2)
	{
		XYZ[] array = new XYZ[3];
		for (int i = 0; i < 3; i++)
		{
			array[i] = new XYZ();
		}
		for (int j = 0; j < 3; j++)
		{
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 3; l++)
				{
					switch (k)
					{
					case 0:
						array[j] = new XYZ(array[j].X + m1[j][l] * m2[l][k], array[j].Y, array[j].Z);
						break;
					case 1:
						array[j] = new XYZ(array[j].X, array[j].Y + m1[j][l] * m2[l][k], array[j].Z);
						break;
					case 2:
						array[j] = new XYZ(array[j].X, array[j].Y, array[j].Z + m1[j][l] * m2[l][k]);
						break;
					}
				}
			}
		}
		return array;
	}

	public static XYZ SubXYZ(XYZ lhs, XYZ rhs)
	{
		double x = lhs.X - rhs.X;
		double y = lhs.Y - rhs.Y;
		double z = lhs.Z - rhs.Z;
		return new XYZ(x, y, z);
	}

	public static XYZ AddXYZ(XYZ lhs, XYZ rhs)
	{
		double x = lhs.X + rhs.X;
		double y = lhs.Y + rhs.Y;
		double z = lhs.Z + rhs.Z;
		return new XYZ(x, y, z);
	}

	public static XYZ DivideXYZ(XYZ lhs, double rhs)
	{
		return new XYZ(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
	}

	public static XYZ GetBasis(XYZ point, Transform transform)
	{
		double x = point.X;
		double y = point.Y;
		double z = point.Z;
		XYZ xYZ = transform.get_Basis(0);
		XYZ xYZ2 = transform.get_Basis(1);
		XYZ xYZ3 = transform.get_Basis(2);
		XYZ origin = transform.Origin;
		double x2 = x * xYZ.X + y * xYZ2.X + z * xYZ3.X + origin.X;
		double y2 = x * xYZ.Y + y * xYZ2.Y + z * xYZ3.Y + origin.Y;
		double z2 = x * xYZ.Z + y * xYZ2.Z + z * xYZ3.Z + origin.Z;
		return new XYZ(x2, y2, z2);
	}

	public static XYZ Vector2XYZ(Vector v)
	{
		return new XYZ(v.X, v.Y, v.Z);
	}

	public static Vector XYZ2Vector(XYZ pnt)
	{
		return new Vector(pnt.X, pnt.Y, pnt.Z);
	}
}
