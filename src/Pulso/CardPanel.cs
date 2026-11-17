using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public class CardPanel : Panel
{
	public int Radius { get; set; } = 14;


	public Color Fill { get; set; } = Palette.Panel;


	public Color BorderColor { get; set; } = Palette.BorderSoft;


	public float BorderWidth { get; set; } = 1f;


	public CardPanel()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Palette.Panel;
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		using GraphicsPath path = Hud.Round(new Rectangle(0, 0, Math.Max(0, base.Width - 1), Math.Max(0, base.Height - 1)), Radius);
		base.Region = new Region(path);
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using GraphicsPath path = Hud.Round(new RectangleF(0.5f, 0.5f, Math.Max(0f, (float)base.Width - 1f), Math.Max(0f, (float)base.Height - 1f)), Radius);
		using (SolidBrush brush = new SolidBrush(Fill))
		{
			graphics.FillPath(brush, path);
		}
		if (BorderWidth > 0f)
		{
			using (Pen pen = new Pen(BorderColor, BorderWidth))
			{
				graphics.DrawPath(pen, path);
				return;
			}
		}
	}
}
