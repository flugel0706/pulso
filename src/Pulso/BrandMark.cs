using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class BrandMark : Control
{
	public BrandMark()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		base.Size = new Size(26, 26);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle rectangle = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		using (GraphicsPath path = Hud.Round(rectangle, Math.Max(6, base.Width / 5)))
		{
			using LinearGradientBrush brush = new LinearGradientBrush(rectangle, Palette.AccentSoft, Palette.AccentDeep, 55f);
			graphics.FillPath(brush, path);
			using Pen pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1f);
			graphics.DrawPath(pen, path);
		}
		float num = (float)base.Width / 2f;
		float num2 = (float)base.Height / 2f;
		float num3 = base.Width;
		using Pen pen2 = new Pen(Color.White, Math.Max(1.8f, num3 / 12f))
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round,
			LineJoin = LineJoin.Round
		};
		graphics.DrawLines(pen2, new PointF[6]
		{
			new PointF(num - num3 * 0.3f, num2),
			new PointF(num - num3 * 0.14f, num2),
			new PointF(num - num3 * 0.04f, num2 - num3 * 0.2f),
			new PointF(num + num3 * 0.08f, num2 + num3 * 0.2f),
			new PointF(num + num3 * 0.18f, num2),
			new PointF(num + num3 * 0.3f, num2)
		});
	}
}
