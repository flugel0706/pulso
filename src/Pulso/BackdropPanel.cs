using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public class BackdropPanel : Panel
{
	public float Dim { get; set; } = 0.5f;

	public bool Photo { get; set; } = true;

	public BackdropPanel()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Palette.Bg;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Photo)
		{
			Backdrop.Paint(e.Graphics, ClientRectangle, RootSize(), RootOffset(), Dim);
			return;
		}
		using SolidBrush brush = new SolidBrush(BackColor);
		e.Graphics.FillRectangle(brush, ClientRectangle);
	}

	public Size RootSize()
	{
		Form? form = FindForm();
		if (form != null)
		{
			return form.ClientSize;
		}
		return base.ClientSize;
	}

	public Point RootOffset()
	{
		Form? form = FindForm();
		if (form == null || form.Handle == IntPtr.Zero || !IsHandleCreated)
		{
			return Point.Empty;
		}
		try
		{
			Point point = PointToScreen(Point.Empty);
			Point point2 = form.PointToScreen(Point.Empty);
			return new Point(point.X - point2.X, point.Y - point2.Y);
		}
		catch
		{
			return Point.Empty;
		}
	}
}
