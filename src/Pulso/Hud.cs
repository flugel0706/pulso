using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Pulso;

public static class Hud
{
	public static GraphicsPath Round(Rectangle r, int radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		int num = Math.Max(1, radius * 2);
		if (r.Width <= num || r.Height <= num)
		{
			graphicsPath.AddRectangle(r);
			return graphicsPath;
		}
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath Round(RectangleF r, int radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = Math.Max(1, radius * 2);
		if (r.Width <= num || r.Height <= num)
		{
			graphicsPath.AddRectangle(r);
			return graphicsPath;
		}
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static void Corners(Graphics g, Rectangle r, Color color, int len = 10)
	{
		using Pen pen = new Pen(color, 1.4f);
		g.DrawLine(pen, r.Left, r.Top, r.Left + len, r.Top);
		g.DrawLine(pen, r.Left, r.Top, r.Left, r.Top + len);
		g.DrawLine(pen, r.Right, r.Top, r.Right - len, r.Top);
		g.DrawLine(pen, r.Right, r.Top, r.Right, r.Top + len);
		g.DrawLine(pen, r.Left, r.Bottom, r.Left + len, r.Bottom);
		g.DrawLine(pen, r.Left, r.Bottom, r.Left, r.Bottom - len);
		g.DrawLine(pen, r.Right, r.Bottom, r.Right - len, r.Bottom);
		g.DrawLine(pen, r.Right, r.Bottom, r.Right, r.Bottom - len);
	}
}
