using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class HeroStrip : Control
{
	private string _callsign = "macro sem nome";

	private string _duration = "0,0 s";

	private string _mode = "Pronto";

	private int _events;

	private bool _live;

	public string Callsign
	{
		get
		{
			return _callsign;
		}
		set
		{
			string text = value ?? "";
			if (!(text == _callsign))
			{
				_callsign = text;
				Invalidate();
			}
		}
	}

	public string Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			string text = value ?? "";
			if (!(text == _duration))
			{
				_duration = text;
				Invalidate();
			}
		}
	}

	public string Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			string text = value ?? "";
			if (!(text == _mode))
			{
				_mode = text;
				Invalidate();
			}
		}
	}

	public int EventCount
	{
		get
		{
			return _events;
		}
		set
		{
			if (value != _events)
			{
				_events = value;
				Invalidate();
			}
		}
	}

	public bool Live
	{
		get
		{
			return _live;
		}
		set
		{
			if (value != _live)
			{
				_live = value;
				Invalidate();
			}
		}
	}

	public HeroStrip()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Palette.Bg;
		base.Height = 86;
	}

	public void SetStats(int events, string duration, string mode, bool live)
	{
		_events = events;
		_duration = duration;
		_mode = mode;
		_live = live;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
		Size size = base.ClientSize;
		Size rootSize = size;
		Point rootOffset = Point.Empty;
		Form? form = FindForm();
		if (form != null)
		{
			rootSize = form.ClientSize;
			if (form.Handle != IntPtr.Zero && IsHandleCreated)
			{
				try
				{
					Point point = PointToScreen(Point.Empty);
					Point point2 = form.PointToScreen(Point.Empty);
					rootOffset = new Point(point.X - point2.X, point.Y - point2.Y);
				}
				catch
				{
				}
			}
		}
		Backdrop.Paint(graphics, new Rectangle(0, 0, size.Width, size.Height), rootSize, rootOffset, 0.02f);
		int num = (int)((float)size.Width * 0.56f);
		if (num > size.Width - 300)
		{
			num = Math.Max(0, size.Width - 300);
		}
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, 8, 11, 8)))
		{
			graphics.FillPolygon(brush, new Point[4]
			{
				new Point(0, 0),
				new Point(num, 0),
				new Point(Math.Max(0, num - 46), size.Height),
				new Point(0, size.Height)
			});
		}
		using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, size.Height - 26, size.Width, 26), Color.FromArgb(0, Palette.Bg), Palette.Bg, 90f))
		{
			graphics.FillRectangle(brush2, 0, size.Height - 26, size.Width, 26);
		}
		using (SolidBrush brush3 = new SolidBrush(Palette.Accent))
		{
			graphics.FillRectangle(brush3, 0, 0, 3, size.Height);
		}
		using (SolidBrush brush4 = new SolidBrush(Palette.Alpha(Palette.Hazard, 190)))
		{
			graphics.FillRectangle(brush4, 3, 0, 1, size.Height);
		}
		DrawTracked(graphics, "// OPERAÇÃO ATIVA", UiFont.TinyBold, Palette.Accent, 22, 10, 1.6f);
		string text = _callsign.ToUpperInvariant();
		float num2 = (float)(num - 60);
		using (Font font = new Font(UiFont.Hero.FontFamily, UiFont.Hero.Size, FontStyle.Regular))
		{
			while (text.Length > 3 && MeasureTracked(graphics, text, font, 2.2f) > num2)
			{
				text = text.Substring(0, text.Length - 2).TrimEnd() + "…";
			}
			DrawTracked(graphics, text, font, Palette.Text, 20, 26, 2.2f);
		}
		DrawTracked(graphics, "ESTÚDIO DE AUTOMAÇÃO TÁTICA", UiFont.TinyBold, Palette.Muted, 22, 62, 1.1f);
		int num3 = 24;
		int num4 = 112;
		int num5 = 14;
		int num6 = size.Width - num3 - (num4 * 3 + num5 * 2);
		if (num6 > num + 20)
		{
			DrawStat(graphics, num6, "EVENTOS", _events.ToString("N0"), Palette.Text, warning: false);
			DrawStat(graphics, num6 + num4 + num5, "DURAÇÃO", _duration, Palette.Text, warning: false);
			DrawStat(graphics, num6 + (num4 + num5) * 2, "STATUS", _mode.ToUpperInvariant(), _live ? Palette.Danger : Palette.Success, warning: _live);
		}
		using Pen pen = new Pen(Palette.Alpha(Palette.Accent, 110));
		graphics.DrawLine(pen, 0, size.Height - 1, size.Width, size.Height - 1);
	}

	private void DrawStat(Graphics g, int x, string label, string value, Color tint, bool warning)
	{
		int num = base.ClientSize.Height;
		using (Pen pen = new Pen(Palette.Alpha(Palette.BorderSoft, 220)))
		{
			g.DrawLine(pen, x - 12, 18, x - 12, num - 20);
		}
		using (SolidBrush brush = new SolidBrush(warning ? Palette.Danger : Palette.Accent))
		{
			g.FillRectangle(brush, x, 18, 16, 2);
		}
		DrawTracked(g, label, UiFont.TinyBold, Palette.Muted, x, 24, 1.2f);
		DrawTracked(g, value, UiFont.HeroSmall, tint, x, 38, 1.1f);
	}

	private static float MeasureTracked(Graphics g, string text, Font font, float track)
	{
		float num = 0f;
		foreach (char c in text)
		{
			num += g.MeasureString(c.ToString(), font, PointF.Empty, StringFormat.GenericTypographic).Width + track;
		}
		return Math.Max(0f, num - track);
	}

	private static void DrawTracked(Graphics g, string text, Font font, Color color, int x, int y, float track)
	{
		using SolidBrush brush = new SolidBrush(color);
		float num = x;
		foreach (char c in text)
		{
			string text2 = c.ToString();
			g.DrawString(text2, font, brush, num, y, StringFormat.GenericTypographic);
			num += g.MeasureString(text2, font, PointF.Empty, StringFormat.GenericTypographic).Width + track;
		}
	}
}
