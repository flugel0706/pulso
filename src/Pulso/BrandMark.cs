using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class BrandMark : Control
{
	public BrandMark()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Color.Transparent;
		base.Size = new Size(26, 26);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Draw(graphics, new RectangleF(0f, 0f, Math.Max(1f, (float)(base.Width - 1)), Math.Max(1f, (float)(base.Height - 1))));
	}

	public static void Draw(Graphics graphics, RectangleF r)
	{
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		float num = r.Width;
		float num2 = r.Height;
		PointF[] points = new PointF[6]
		{
			new PointF(r.X + num * 0.5f, r.Y),
			new PointF(r.X + num, r.Y + num2 * 0.25f),
			new PointF(r.X + num, r.Y + num2 * 0.75f),
			new PointF(r.X + num * 0.5f, r.Y + num2),
			new PointF(r.X, r.Y + num2 * 0.75f),
			new PointF(r.X, r.Y + num2 * 0.25f)
		};
		using (GraphicsPath path = new GraphicsPath())
		{
			path.AddPolygon(points);
			using LinearGradientBrush brush = new LinearGradientBrush(r, Palette.AccentSoft, Palette.AccentDeep, 60f);
			graphics.FillPath(brush, path);
			using Pen pen = new Pen(Color.FromArgb(150, Palette.Ink), Math.Max(1f, num / 19f));
			graphics.DrawPath(pen, path);
		}
		float num3 = r.X + num / 2f;
		float num4 = r.Y + num2 / 2f;
		using Pen pen2 = new Pen(Palette.Ink, Math.Max(1.6f, num / 14f))
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round,
			LineJoin = LineJoin.Round
		};
		graphics.DrawLines(pen2, new PointF[5]
		{
			new PointF(num3 - num * 0.3f, num4),
			new PointF(num3 - num * 0.13f, num4),
			new PointF(num3, num4 - num2 * 0.2f),
			new PointF(num3 + num * 0.14f, num4 + num2 * 0.14f),
			new PointF(num3 + num * 0.3f, num4)
		});
	}
}
