using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Autodesk.Revit.DB;

namespace EPCommon;

public class Sketch3D
{
	private const float MarginRatio = 0.1f;

	private const float MoveDistance = 2f;

	private const float ZoomRatio = 0.05f;

	private List<PointF[]> m_curve3Ds = new List<PointF[]>();

	private Graphics3DData m_data3D;

	private Matrix m_transform3D;

	private RectangleF m_displayBBox = new RectangleF(0f, 0f, 10f, 10f);

	public RectangleF DisplayBBox
	{
		get
		{
			return m_displayBBox;
		}
		set
		{
			m_displayBBox = value;
			Calculate3DTransform();
			UpdateDataAndEvent();
			if (UpdateViewEvent != null)
			{
				UpdateViewEvent();
			}
		}
	}

	public Graphics3DData Data3D
	{
		get
		{
			return m_data3D;
		}
		set
		{
			m_data3D = value;
			UpdateDataAndEvent();
			if (UpdateViewEvent != null)
			{
				UpdateViewEvent();
			}
		}
	}

	public event UpdateViewDelegate UpdateViewEvent;

	public Sketch3D(Graphics3DData data3D)
	{
		m_data3D = data3D;
		Calculate3DTransform();
		UpdateDataAndEvent();
	}

	public void Draw(Graphics graphics)
	{
		if (m_data3D.NumPoints == 0)
		{
			graphics.Clear(System.Drawing.Color.LightGray);
			return;
		}
		graphics.Clear(System.Drawing.Color.White);
		DrawCurves(graphics, m_curve3Ds, m_transform3D, new Pen(System.Drawing.Color.DarkGreen));
	}

	public void Zoom(bool zoomIn)
	{
		if (zoomIn)
		{
			m_transform3D.Scale(0.95f, 0.95f, MatrixOrder.Append);
		}
		else
		{
			m_transform3D.Scale(1.05f, 1.05f, MatrixOrder.Append);
		}
		if (UpdateViewEvent != null)
		{
			UpdateViewEvent();
		}
	}

	public void MoveX(bool left)
	{
		if (left)
		{
			m_transform3D.Translate(-2f, 0f, MatrixOrder.Append);
		}
		else
		{
			m_transform3D.Translate(2f, 0f, MatrixOrder.Append);
		}
		if (UpdateViewEvent != null)
		{
			UpdateViewEvent();
		}
	}

	public void MoveY(bool up)
	{
		if (up)
		{
			m_transform3D.Translate(0f, 2f, MatrixOrder.Append);
		}
		else
		{
			m_transform3D.Translate(0f, -2f, MatrixOrder.Append);
		}
		if (UpdateViewEvent != null)
		{
			UpdateViewEvent();
		}
	}

	private void UpdateDataAndEvent()
	{
		Initialize3DData();
		m_data3D.UpdateViewEvent += DataUpdateViewEvent;
	}

	private void Initialize3DData()
	{
		m_curve3Ds.Clear();
		foreach (List<Vector> constCurf in m_data3D.ConstCurves)
		{
			PointF[] array = new PointF[constCurf.Count];
			m_curve3Ds.Add(array);
			for (int i = 0; i < constCurf.Count; i++)
			{
				array[i] = new PointF((float)constCurf[i].X, (float)constCurf[i].Y);
			}
		}
	}

	private void DrawCurves(Graphics graphics, List<PointF[]> curves, Matrix transform, Pen pen)
	{
		foreach (PointF[] curf in curves)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			if (curf.Length == 0)
			{
				break;
			}
			if (curf.Length == 1)
			{
				graphicsPath.AddArc(new RectangleF(curf[0], new SizeF(0.5f, 0.5f)), 0f, (float)Math.PI);
			}
			else
			{
				graphicsPath.AddLines(curf);
			}
			graphicsPath.Transform(transform);
			graphics.DrawPath(pen, graphicsPath);
		}
	}

	private void DataUpdateViewEvent()
	{
		Initialize3DData();
		if (UpdateViewEvent != null)
		{
			UpdateViewEvent();
		}
	}

	private void Calculate3DTransform()
	{
		float width = m_displayBBox.Width;
		float height = m_displayBBox.Height;
		RectangleF bbox = ToBoundingBox2D(m_data3D.BoundingBox);
		PointF[] plgpts = CalculateCanvasRegion(width, height, bbox);
		m_transform3D = new Matrix(PreTreatBBox(bbox), plgpts);
	}

	private RectangleF PreTreatBBox(RectangleF bbox)
	{
		if (bbox.Height < 0.001f)
		{
			bbox.Height = 0.001f;
		}
		if (bbox.Width < 0.001f)
		{
			bbox.Width = 0.001f;
		}
		return bbox;
	}

	private PointF[] CalculateCanvasRegion(float previewWidth, float previewHeigh, RectangleF bbox)
	{
		float num = previewWidth * 0.8f;
		float num2 = previewHeigh * 0.8f;
		float num3 = previewWidth * 0.1f;
		float num4 = previewHeigh * 0.1f;
		float num5 = bbox.Width / bbox.Height;
		float num6 = num / num2;
		if (num5 > num6)
		{
			float num7 = num / num5;
			num4 += (num2 - num7) / 2f;
			num2 = num7;
		}
		else
		{
			float num8 = num2 * num5;
			num3 += (num - num8) / 2f;
			num = num8;
		}
		return new PointF[3]
		{
			new PointF(num3, num2 + num4),
			new PointF(num + num3, num2 + num4),
			new PointF(num3, num4)
		};
	}

	private RectangleF ToBoundingBox2D(BoundingBoxXYZ bbox3D)
	{
		float num = (float)bbox3D.Min.X;
		float num2 = (float)bbox3D.Min.Y;
		float num3 = (float)bbox3D.Max.X;
		float num4 = (float)bbox3D.Max.Y;
		return new RectangleF(num, num2, num3 - num, num4 - num2);
	}
}
