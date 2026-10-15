using System;

namespace EPCommon;

public class UCS
{
	private Vector m_origin = new Vector(0.0, 0.0, 0.0);

	private Vector m_xAxis = new Vector(1.0, 0.0, 0.0);

	private Vector m_yAxis = new Vector(0.0, 1.0, 0.0);

	private Vector m_zAxis = new Vector(0.0, 0.0, 1.0);

	public Vector Origin
	{
		get
		{
			return m_origin;
		}
		set
		{
			m_origin = value;
		}
	}

	public Vector this[int index] => index switch
	{
		0 => m_xAxis, 
		1 => m_yAxis, 
		2 => m_zAxis, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public Vector XAxis => m_xAxis;

	public Vector YAxis => m_yAxis;

	public Vector ZAxis => m_zAxis;

	public UCS()
	{
	}

	public UCS(Vector origin, Vector xAxis, Vector yAxis)
		: this(origin, xAxis, yAxis, flag: true)
	{
	}

	public UCS(Vector origin, Vector xAxis, Vector yAxis, bool flag)
	{
		Vector vector = xAxis / ~xAxis;
		Vector vector2 = yAxis / ~yAxis;
		Vector vector3 = vector & vector2;
		if (~vector3 < double.Epsilon)
		{
			throw new InvalidOperationException();
		}
		if (!flag)
		{
			vector3 = -vector3;
		}
		m_origin = origin;
		m_xAxis = vector;
		m_yAxis = vector2;
		m_zAxis = vector3;
	}

	public Vector LC2GC(Vector arg)
	{
		return new Vector
		{
			X = arg.X * m_xAxis.X + arg.Y * m_yAxis.X + arg.Z * m_zAxis.X + m_origin.X,
			Y = arg.X * m_xAxis.Y + arg.Y * m_yAxis.Y + arg.Z * m_zAxis.Y + m_origin.Y,
			Z = arg.X * m_xAxis.Z + arg.Y * m_yAxis.Z + arg.Z * m_zAxis.Z + m_origin.Z
		};
	}

	public Vector GC2LC(Vector arg)
	{
		Vector result = default;
		arg -= m_origin;
		result.X = m_xAxis * arg;
		result.Y = m_yAxis * arg;
		result.Z = m_zAxis * arg;
		return result;
	}

	public static UCS operator +(UCS lhs, UCS rhs)
	{
		Vector origin = lhs.Origin + rhs.Origin;
		Vector[] array = new Vector[3];
		Vector[] array2 = new Vector[3];
		for (int i = 0; i < 3; i++)
		{
			array[i] = lhs[i];
			array2[i] = rhs[i];
		}
		Vector[] array3 = Vector.MultiCross3X3Matrix(array, array2);
		return new UCS(origin, array3[0], array3[1]);
	}
}
