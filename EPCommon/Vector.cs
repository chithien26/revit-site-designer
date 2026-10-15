using System;

namespace EPCommon;

public struct Vector
{
	private double m_x;

	private double m_y;

	private double m_z;

	public double X
	{
		get
		{
			return m_x;
		}
		set
		{
			m_x = value;
		}
	}

	public double Y
	{
		get
		{
			return m_y;
		}
		set
		{
			m_y = value;
		}
	}

	public double Z
	{
		get
		{
			return m_z;
		}
		set
		{
			m_z = value;
		}
	}

	public double this[int index]
	{
		get
		{
			return index switch
			{
				0 => m_x, 
				1 => m_y, 
				2 => m_z, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				m_x = value;
				break;
			case 1:
				m_y = value;
				break;
			case 2:
				m_z = value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vector(Vector rhs)
	{
		m_x = rhs.X;
		m_y = rhs.Y;
		m_z = rhs.Z;
	}

	public Vector(double x, double y, double z)
	{
		m_x = x;
		m_y = y;
		m_z = z;
	}

	public Vector GetNormal()
	{
		Vector result = default;
		double length = GetLength();
		if (length < double.Epsilon)
		{
			return default;
		}
		result.X = m_x / length;
		result.Y = m_y / length;
		result.Z = m_z / length;
		return result;
	}

	public static Vector operator +(Vector lhs, Vector rhs)
	{
		Vector result = new Vector(lhs);
		result.X += rhs.X;
		result.Y += rhs.Y;
		result.Z += rhs.Z;
		return result;
	}

	public static Vector operator -(Vector lhs, Vector rhs)
	{
		Vector result = new Vector(lhs);
		result.X -= rhs.X;
		result.Y -= rhs.Y;
		result.Z -= rhs.Z;
		return result;
	}

	public static Vector operator -(Vector lhs)
	{
		Vector result = new Vector(lhs);
		result.X = 0.0 - lhs.X;
		result.Y = 0.0 - lhs.Y;
		result.Z = 0.0 - lhs.Z;
		return result;
	}

	public static Vector operator &(Vector lhs, Vector rhs)
	{
		double x = lhs.X;
		double y = lhs.Y;
		double z = lhs.Z;
		double x2 = rhs.X;
		double y2 = rhs.Y;
		double z2 = rhs.Z;
		double x3 = y * z2 - z * y2;
		double y3 = z * x2 - x * z2;
		double z3 = x * y2 - y * x2;
		return new Vector(x3, y3, z3);
	}

	public static double operator *(Vector lhs, Vector rhs)
	{
		return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
	}

	public static Vector operator *(Vector lhs, double rhs)
	{
		return new Vector(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
	}

	public static bool operator !=(Vector lhs, Vector rhs)
	{
		return !IsEqual(lhs, rhs);
	}

	public static bool operator ==(Vector lhs, Vector rhs)
	{
		return IsEqual(lhs, rhs);
	}

	public static double operator ~(Vector lhs)
	{
		return lhs.GetLength();
	}

	public static Vector operator /(Vector lhs, double rhs)
	{
		return new Vector(lhs.m_x / rhs, lhs.m_y / rhs, lhs.m_z / rhs);
	}

	public static double GetAngleOf2Vectors(Vector lhs, Vector rhs, bool acuteAngleDesired)
	{
		double num = Math.Acos(lhs.GetNormal() * rhs.GetNormal());
		if (acuteAngleDesired && num > Math.PI / 2.0)
		{
			num = Math.PI - num;
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		try
		{
			Vector rhs = (Vector)obj;
			return IsEqual(this, rhs);
		}
		catch
		{
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode();
	}

	public double GetLength()
	{
		return Math.Sqrt(m_x * m_x + m_y * m_y + m_z * m_z);
	}

	private static bool IsEqual(Vector lhs, Vector rhs)
	{
		if (lhs.X == rhs.X && lhs.X == rhs.X && lhs.X == rhs.X)
		{
			return true;
		}
		return false;
	}

	public static Vector[] MultiCross3X3Matrix(Vector[] m1, Vector[] m2)
	{
		Vector[] array = new Vector[3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					switch (j)
					{
					case 0:
						array[i].X += m1[i][k] * m2[k][j];
						break;
					case 1:
						array[i].Y += m1[i][k] * m2[k][j];
						break;
					case 2:
						array[i].Z += m1[i][k] * m2[k][j];
						break;
					}
				}
			}
		}
		return array;
	}
}
