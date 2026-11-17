using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class HudButton : Control
{
	private bool _hover;

	private bool _pressed;

	private Color _back;

	private Color _border;

	private Color _text;

	public HudKind Kind { get; set; }

	public bool Active { get; set; }

	public Color Accent { get; set; } = Palette.Accent;


	public int Radius { get; set; } = 9;


	public ButtonIcon Icon { get; set; }

	public bool LeftAlign { get; set; }

	public bool TextVisible { get; set; } = true;


	public HudButton()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		Font = UiFont.BodyBold;
		ForeColor = Palette.Text;
		Cursor = Cursors.Hand;
		base.Size = new Size(120, 38);
		base.TabStop = false;
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
		_pressed = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		_pressed = true;
		Invalidate();
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		_pressed = false;
		Invalidate();
		base.OnMouseUp(e);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		Invalidate();
		base.OnEnabledChanged(e);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		Invalidate();
		base.OnTextChanged(e);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Radius = Math.Min(Radius, Math.Min(base.Width, base.Height) / 2);
	}

	private void Compute()
	{
		Color color = (base.Enabled ? Accent : Palette.PanelAlt);
		switch (Kind)
		{
		case HudKind.Primary:
			_back = ((!base.Enabled) ? Palette.PanelAlt : (_pressed ? Palette.Blend(color, Color.Black, 0.22) : (_hover ? Palette.Blend(color, Color.White, 0.13) : color)));
			_border = ((!base.Enabled) ? Palette.BorderSoft : Palette.Blend(color, Color.White, 0.18));
			_text = (base.Enabled ? Color.White : Palette.Muted);
			break;
		case HudKind.Danger:
			_back = ((!base.Enabled) ? Palette.PanelAlt : (_pressed ? Palette.Blend(color, Color.Black, 0.22) : (_hover ? Palette.Blend(color, Color.White, 0.12) : color)));
			_border = ((!base.Enabled) ? Palette.BorderSoft : Palette.Blend(color, Color.White, 0.12));
			_text = (base.Enabled ? Color.White : Palette.Muted);
			break;
		case HudKind.Bare:
			_back = (_pressed ? Palette.PanelHover : (_hover ? Palette.Alpha(Palette.Text, 14) : Color.Transparent));
			_border = Color.Transparent;
			_text = ((!base.Enabled) ? Palette.Muted : (_hover ? Palette.Text : Palette.TextSoft));
			break;
		case HudKind.Outline:
			_back = (_pressed ? Palette.PanelHover : (_hover ? Palette.Alpha(color, 22) : Color.Transparent));
			_border = ((_hover || _pressed) ? Palette.Blend(color, Color.White, 0.12) : Palette.Alpha(color, 120));
			_text = (base.Enabled ? Palette.Text : Palette.Muted);
			break;
		default:
			_back = ((!base.Enabled) ? Palette.Alpha(Palette.PanelAlt, 130) : (_pressed ? Palette.PanelHover : (_hover ? Palette.Blend(Palette.PanelAlt, Palette.Text, 0.045) : Palette.PanelAlt)));
			_border = (Active ? Palette.Alpha(color, 160) : (_hover ? Palette.Border : Palette.BorderSoft));
			_text = ((!base.Enabled) ? Palette.Muted : (Active ? color : Palette.Text));
			break;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Compute();
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		using (GraphicsPath path = Hud.Round(new RectangleF(0.5f, 0.5f, (float)base.Width - 1f, (float)base.Height - 1f), Radius))
		{
			if (_back.A > 0)
			{
				using SolidBrush brush = new SolidBrush(_back);
				graphics.FillPath(brush, path);
			}
			if (_border.A > 0)
			{
				using Pen pen = new Pen(_border, (Active || _hover) ? 1.2f : 1f);
				graphics.DrawPath(pen, path);
			}
		}
		bool flag = Active;
		if (flag)
		{
			HudKind kind = Kind;
			bool flag2 = ((kind == HudKind.Ghost || kind == HudKind.Outline) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			using SolidBrush brush2 = new SolidBrush(Palette.Alpha(Accent, 90));
			graphics.FillEllipse(brush2, 10, base.Height / 2 - 2, 4, 4);
		}
		bool flag3 = TextVisible && !string.IsNullOrEmpty(Text);
		int num = ((Icon != 0) ? Math.Max(14, Math.Min(18, base.Height - 18)) : 0);
		int num2 = ((num > 0 && flag3) ? ((int)(7f * Math.Max(1f, (float)base.DeviceDpi / 96f))) : 0);
		Font font = Font;
		Size size = (flag3 ? TextRenderer.MeasureText(Text, font, Size.Empty, TextFormatFlags.NoPrefix) : Size.Empty);
		int num3 = num + num2 + (flag3 ? size.Width : 0);
		int num4;
		int num5 = (num4 = (LeftAlign ? 15 : ((base.Width - num3) / 2))) + num + num2;
		if (num > 0)
		{
			VectorIcon.Draw(r: new RectangleF(num4, (float)(base.Height - num) / 2f, num, num), g: graphics, icon: Icon, color: _text);
		}
		if (flag3)
		{
			TextRenderer.DrawText(bounds: new Rectangle(num5, 0, Math.Max(1, base.Width - num5 - 6), base.Height), dc: graphics, text: Text, font: font, foreColor: _text, flags: TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		}
		else if (num == 0)
		{
			TextRenderer.DrawText(graphics, Text, font, base.ClientRectangle, _text, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		}
	}
}
