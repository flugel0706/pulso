using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class PulseCheck : CheckBox
{
	private bool _hover;

	public PulseCheck()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Color.Transparent;
		ForeColor = Palette.TextSoft;
		base.FlatStyle = FlatStyle.Flat;
		AutoSize = true;
		base.Height = 24;
		Cursor = Cursors.Hand;
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size size = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPrefix);
		return new Size(size.Width + 32, Math.Max(24, size.Height + 4));
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		_hover = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		_hover = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnCheckedChanged(EventArgs e)
	{
		Invalidate();
		base.OnCheckedChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			graphics.FillRectangle(brush, base.ClientRectangle);
		}
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		int num = 18;
		int num2 = (base.Height - num) / 2;
		using (GraphicsPath path = Hud.Round(new RectangleF(0.5f, (float)num2 + 0.5f, num, num), 5))
		{
			using SolidBrush brush2 = new SolidBrush(base.Checked ? Palette.Accent : (_hover ? Palette.PanelHover : Palette.PanelAlt));
			graphics.FillPath(brush2, path);
			using Pen pen = new Pen(base.Checked ? Palette.Accent : (_hover ? Palette.Border : Palette.BorderSoft), 1.3f);
			graphics.DrawPath(pen, path);
		}
		if (base.Checked)
		{
			using Pen pen2 = new Pen(Color.White, 2f)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round,
				LineJoin = LineJoin.Round
			};
			float num3 = 4.5f;
			float num4 = (float)num2 + 9f;
			graphics.DrawLines(pen2, new PointF[3]
			{
				new PointF(num3, num4),
				new PointF(num3 + 4.5f, num4 + 4.5f),
				new PointF(num3 + 9.5f, num4 - 5.5f)
			});
		}
		TextRenderer.DrawText(graphics, Text, Font, new Rectangle(28, 0, Math.Max(1, base.Width - 28), base.Height), base.Enabled ? Palette.TextSoft : Palette.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
	}
}
