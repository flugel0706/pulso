using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Pulso;

public sealed class MiniBar : Control
{
	private enum Slot
	{
		None,
		Record,
		Play,
		Pause,
		Stop,
		Minus,
		Plus,
		Loop,
		Settings,
		More,
		Expand,
		Close
	}

	public const int BaseWidth = 720;

	public const int BaseHeight = 136;

	private readonly Rectangle[] _rects = new Rectangle[12];

	private Slot _hot;

	private Slot _pressed;

	private Point _down;

	private bool _armed;

	private bool _dragging;

	private string _callsign = "macro sem nome";

	private string _duration = "0,0 s";

	private string _state = "Pronto";

	private int _events;

	private bool _recording;

	private bool _playing;

	private bool _paused;

	private int _speed = 100;

	private int _loops = 1;

	private bool _continuous;

	private double _progress = -1.0;

	private static Bitmap? _art;

	private static bool _artTried;

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

	public string StateText
	{
		get
		{
			return _state;
		}
		set
		{
			string text = (value ?? "").ToUpperInvariant();
			if (!(text == _state))
			{
				_state = text;
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

	public bool Recording
	{
		get
		{
			return _recording;
		}
		set
		{
			if (value != _recording)
			{
				_recording = value;
				Invalidate();
			}
		}
	}

	public bool Playing
	{
		get
		{
			return _playing;
		}
		set
		{
			if (value != _playing)
			{
				_playing = value;
				Invalidate();
			}
		}
	}

	public bool Paused
	{
		get
		{
			return _paused;
		}
		set
		{
			if (value != _paused)
			{
				_paused = value;
				Invalidate();
			}
		}
	}

	public int SpeedPercent
	{
		get
		{
			return _speed;
		}
		set
		{
			if (value != _speed)
			{
				_speed = value;
				Invalidate();
			}
		}
	}

	public int Loops
	{
		get
		{
			return _loops;
		}
		set
		{
			if (value != _loops)
			{
				_loops = value;
				Invalidate();
			}
		}
	}

	public bool Continuous
	{
		get
		{
			return _continuous;
		}
		set
		{
			if (value != _continuous)
			{
				_continuous = value;
				Invalidate();
			}
		}
	}

	public double Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			double num = Math.Max(-1.0, Math.Min(1.0, value));
			if (Math.Abs(num - _progress) > 0.004)
			{
				_progress = num;
				Invalidate();
			}
		}
	}

	public event Action? RecordClick;

	public event Action? PlayClick;

	public event Action? PauseClick;

	public event Action? StopClick;

	public event Action? SpeedDownClick;

	public event Action? SpeedUpClick;

	public event Action? LoopClick;

	public event Action? SettingsClick;

	public event Action? MoreClick;

	public event Action? ExpandClick;

	public event Action? CloseClick;

	public MiniBar()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Palette.Bg;
		Cursor = Cursors.SizeAll;
		base.TabStop = false;
	}

	private int S(int value)
	{
		return (int)Math.Round((double)value * (double)base.DeviceDpi / 96.0);
	}

	private void LayoutButtons()
	{
		int num = S(42);
		int num2 = S(6);
		int num3 = base.Width - S(16);
		int num4 = num3 - (num * 4 + num2 * 3);
		int y = S(20);
		_rects[1] = new Rectangle(num4, y, num, num);
		_rects[2] = new Rectangle(num4 + num + num2, y, num, num);
		_rects[3] = new Rectangle(num4 + (num + num2) * 2, y, num, num);
		_rects[4] = new Rectangle(num4 + (num + num2) * 3, y, num, num);
		int num5 = S(26);
		int num6 = S(5);
		int num7 = num3 - (num5 * 7 + num6 * 6);
		int y2 = S(76);
		for (int i = 5; i <= 11; i++)
		{
			_rects[i] = new Rectangle(num7 + (num5 + num6) * (i - 5), y2, num5, num5);
		}
	}

	private Slot HitTest(Point p)
	{
		for (int i = 1; i <= 11; i++)
		{
			if (_rects[i].Contains(p))
			{
				return (Slot)i;
			}
		}
		return Slot.None;
	}

	private bool IsEnabled(Slot slot)
	{
		bool flag = _recording || _playing;
		switch (slot)
		{
		case Slot.Play:
			return !_recording && !_playing && _events > 0;
		case Slot.Pause:
			return _playing;
		case Slot.Stop:
			return flag;
		default:
			return true;
		}
	}

	private string Hint(Slot slot)
	{
		switch (slot)
		{
		case Slot.Record:
			return _recording ? "PARAR GRAVAÇÃO" : "GRAVAR";
		case Slot.Play:
			return "REPRODUZIR  ·  F5";
		case Slot.Pause:
			return (_paused ? "CONTINUAR" : "PAUSAR") + "  ·  Ctrl+Alt+Espaço";
		case Slot.Stop:
			return "PARAR TUDO  ·  F12";
		case Slot.Minus:
			return "MENOS VELOCIDADE";
		case Slot.Plus:
			return "MAIS VELOCIDADE";
		case Slot.Loop:
			return _continuous ? "REPETIÇÃO CONTÍNUA ATIVA" : "REPETIR CONTINUAMENTE";
		case Slot.Settings:
			return "CONFIGURAÇÕES";
		case Slot.More:
			return "MAIS OPÇÕES";
		case Slot.Expand:
			return "VOLTAR À JANELA  ·  duplo clique";
		case Slot.Close:
			return "FECHAR O PULSO";
		default:
			return "";
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_armed && !_dragging && e.Button == MouseButtons.Left)
		{
			Size size = SystemInformation.DragSize;
			if (Math.Abs(e.X - _down.X) > size.Width || Math.Abs(e.Y - _down.Y) > size.Height)
			{
				_dragging = true;
				Form? form = FindForm();
				if (form != null)
				{
					Native.ReleaseCapture();
					Native.SendMessage(form.Handle, 161, 2, IntPtr.Zero);
				}
			}
		}
		Slot slot = HitTest(e.Location);
		if (slot != _hot)
		{
			_hot = slot;
			Cursor = ((slot == Slot.None) ? Cursors.SizeAll : Cursors.Hand);
			Invalidate();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		Slot slot = HitTest(e.Location);
		_pressed = slot;
		_down = e.Location;
		_armed = slot == Slot.None;
		_dragging = false;
		if (slot != Slot.None)
		{
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button == MouseButtons.Right)
		{
			MoreClick?.Invoke();
			return;
		}
		Slot slot = HitTest(e.Location);
		Slot pressed = _pressed;
		_pressed = Slot.None;
		_armed = false;
		_dragging = false;
		if (pressed != Slot.None && pressed == slot && IsEnabled(slot))
		{
			switch (slot)
			{
			case Slot.Record:
				RecordClick?.Invoke();
				break;
			case Slot.Play:
				PlayClick?.Invoke();
				break;
			case Slot.Pause:
				PauseClick?.Invoke();
				break;
			case Slot.Stop:
				StopClick?.Invoke();
				break;
			case Slot.Minus:
				SpeedDownClick?.Invoke();
				break;
			case Slot.Plus:
				SpeedUpClick?.Invoke();
				break;
			case Slot.Loop:
				LoopClick?.Invoke();
				break;
			case Slot.Settings:
				SettingsClick?.Invoke();
				break;
			case Slot.More:
				MoreClick?.Invoke();
				break;
			case Slot.Expand:
				ExpandClick?.Invoke();
				break;
			case Slot.Close:
				CloseClick?.Invoke();
				break;
			}
		}
		Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hot != Slot.None)
		{
			_hot = Slot.None;
			Cursor = Cursors.SizeAll;
			Invalidate();
		}
	}

	protected override void OnDoubleClick(EventArgs e)
	{
		base.OnDoubleClick(e);
		if (HitTest(PointToClient(Cursor.Position)) == Slot.None)
		{
			ExpandClick?.Invoke();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
		LayoutButtons();
		PaintBackdrop(graphics);
		PaintFrame(graphics);
		PaintIdentity(graphics);
		PaintStats(graphics);
		PaintButtons(graphics);
		PaintFooter(graphics);
	}

	private static Bitmap? Art()
	{
		if (_artTried)
		{
			return _art;
		}
		_artTried = true;
		Bitmap? source = Backdrop.Source;
		if (source == null)
		{
			return null;
		}
		Rectangle crop = Crop(source.Size);
		try
		{
			Bitmap bitmap = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				using ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetGamma(1.3f);
				imageAttributes.SetColorMatrix(Grade());
				graphics.DrawImage(source, new Rectangle(0, 0, crop.Width, crop.Height), crop.X, crop.Y, crop.Width, crop.Height, GraphicsUnit.Pixel, imageAttributes);
			}
			_art = bitmap;
		}
		catch
		{
			_art = null;
		}
		return _art;
	}

	private static Rectangle Crop(Size size)
	{
		int width = Math.Min(size.Width, Math.Max(320, size.Width * 3 / 4));
		int height = Math.Max(1, (int)Math.Round((double)width * BaseHeight / BaseWidth));
		if (height > size.Height)
		{
			height = size.Height;
			width = Math.Max(1, (int)Math.Round((double)height * BaseWidth / BaseHeight));
		}
		int x = Math.Max(0, Math.Min(size.Width - width, (int)Math.Round((double)size.Width * 0.25)));
		int y = Math.Max(0, Math.Min(size.Height - height, (int)Math.Round((double)size.Height * 0.32)));
		return new Rectangle(x, y, Math.Max(1, width), Math.Max(1, height));
	}

	private static ColorMatrix Grade()
	{
		float s = 0.72f;
		float exposure = 1.45f;
		float r = 0.3086f;
		float g = 0.6094f;
		float b = 0.082f;
		float nr = (1f - s) * r;
		float ng = (1f - s) * g;
		float nb = (1f - s) * b;
		return new ColorMatrix(new float[5][]
		{
			new float[5] { (nr + s) * exposure, nr * exposure, nr * exposure, 0f, 0f },
			new float[5] { ng * exposure, (ng + s) * exposure, ng * exposure, 0f, 0f },
			new float[5] { nb * exposure, nb * exposure, (nb + s) * exposure, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { 0.012f, 0.008f, 0.004f, 0f, 1f }
		});
	}

	private void PaintBackdrop(Graphics g)
	{
		Rectangle area = base.ClientRectangle;
		Bitmap? bitmap = Art();
		if (bitmap == null)
		{
			using SolidBrush brush = new SolidBrush(Palette.Bg);
			g.FillRectangle(brush, area);
			return;
		}
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		g.DrawImage(bitmap, area);
		int left = Math.Max(2, (int)((float)base.Width * 0.62f));
		using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, 0, left, Math.Max(2, base.Height)), Color.FromArgb(228, 6, 9, 6), Color.FromArgb(0, 6, 9, 6), 0f))
		{
			g.FillRectangle(brush2, area);
		}
		using (LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(0, 0, Math.Max(2, base.Width), Math.Max(2, base.Height)), Color.FromArgb(104, 4, 7, 4), Color.FromArgb(26, 4, 7, 4), 90f))
		{
			g.FillRectangle(brush3, area);
		}
		using (Pen pen = new Pen(Color.FromArgb(22, 0, 0, 0)))
		{
			for (int i = 0; i < base.Height; i += 3)
			{
				g.DrawLine(pen, 0, i, base.Width, i);
			}
		}
	}

	private void PaintFrame(Graphics g)
	{
		int width = base.Width;
		int height = base.Height;
		using (GraphicsPath path = new GraphicsPath())
		{
			path.AddPolygon(new Point[4]
			{
				new Point(0, 0),
				new Point((int)((float)width * 0.44f), 0),
				new Point((int)((float)width * 0.38f), height),
				new Point(0, height)
			});
			using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, Math.Max(1, (int)((float)width * 0.44f)), height), Color.FromArgb(198, 8, 11, 8), Color.FromArgb(48, 8, 11, 8), 0f);
			g.FillPath(brush, path);
		}
		using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, height - S(34), width, S(34)), Color.FromArgb(0, Palette.Bg), Color.FromArgb(210, Palette.Bg), 90f))
		{
			g.FillRectangle(brush2, 0, height - S(34), width, S(34));
		}
		using (SolidBrush brush3 = new SolidBrush(Palette.Accent))
		{
			g.FillRectangle(brush3, 0, 0, S(3), height);
		}
		using (SolidBrush brush4 = new SolidBrush(Palette.Alpha(Palette.Hazard, 190)))
		{
			g.FillRectangle(brush4, S(3), 0, 1, height);
		}
		using Pen pen = new Pen(Palette.Alpha(Palette.Accent, 120));
		g.DrawRectangle(pen, 0, 0, width - 1, height - 1);
	}

	private void PaintIdentity(Graphics g)
	{
		int num = S(16);
		BrandMark.Draw(g, new RectangleF(num, S(10), S(24), S(24)));
		DrawTracked(g, "PULSO", UiFont.Title, Palette.Text, num + S(32), S(6), 1.6f);
		DrawTracked(g, "CAMPO  //  " + Program.ShortVersion, UiFont.TinyBold, Palette.Muted, num + S(33), S(25), 0.9f);
		int num2 = S(236);
		Rectangle bounds = new Rectangle(num, S(42), num2, S(24));
		TextRenderer.DrawText(g, _callsign.ToUpperInvariant(), UiFont.HeroSmall, bounds, Palette.Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
		int num3 = S(88);
		Color color = (_recording ? Palette.Danger : (_playing ? Palette.Accent : Palette.Success));
		using (SolidBrush brush = new SolidBrush(Palette.Alpha(color, _recording || _playing ? 255 : 200)))
		{
			g.FillEllipse(brush, num + S(1), num3 + S(5), S(7), S(7));
		}
		using (Pen pen = new Pen(Palette.Alpha(color, 90)))
		{
			g.DrawEllipse(pen, num + S(1), num3 + S(5), S(7), S(7));
		}
		TextRenderer.DrawText(g, _state, UiFont.TinyBold, new Rectangle(num + S(14), num3, num2 - S(14), S(18)), color, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
	}

	private void PaintStats(Graphics g)
	{
		int num = S(300);
		int num2 = S(38);
		int num3 = S(96);
		Rectangle rect = new Rectangle(S(272), S(24), S(248), S(88));
		using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.FromArgb(132, 6, 9, 6), Color.FromArgb(0, 6, 9, 6), 0f))
		{
			g.FillRectangle(brush, rect);
		}
		DrawStat(g, num, num2, "EVENTOS", _events.ToString("N0"));
		DrawStat(g, num + num3, num2, "DURAÇÃO", _duration);
		string text = $"VEL {_speed}%  ·  REP {(_continuous ? "∞" : _loops + "×")}";
		TextRenderer.DrawText(g, text, UiFont.MonoSmall, new Rectangle(num, S(82), S(200), S(18)), Palette.Muted, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
	}

	private void DrawStat(Graphics g, int x, int y, string label, string value)
	{
		using (Pen pen = new Pen(Palette.Alpha(Palette.BorderSoft, 210)))
		{
			g.DrawLine(pen, x - S(10), y, x - S(10), y + S(56));
		}
		using (SolidBrush brush = new SolidBrush(Palette.Accent))
		{
			g.FillRectangle(brush, x, y, S(14), 2);
		}
		TextRenderer.DrawText(g, label, UiFont.TinyBold, new Rectangle(x, y + S(5), S(90), S(16)), Palette.Muted, TextFormatFlags.NoPrefix);
		TextRenderer.DrawText(g, value, UiFont.Head, new Rectangle(x, y + S(20), S(90), S(24)), Palette.Text, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
	}

	private void PaintButtons(Graphics g)
	{
		DrawPrimary(g, Slot.Record, ButtonIcon.Record, Palette.Danger);
		DrawPrimary(g, Slot.Play, ButtonIcon.Play, Palette.Accent);
		DrawSecondary(g, Slot.Pause, ButtonIcon.Pause, _paused);
		DrawSecondary(g, Slot.Stop, ButtonIcon.Stop, active: false);
		DrawSmall(g, Slot.Minus, ButtonIcon.Minus, _hot == Slot.Minus);
		DrawSmall(g, Slot.Plus, ButtonIcon.Plus, _hot == Slot.Plus);
		DrawSmall(g, Slot.Loop, ButtonIcon.Infinity, _continuous);
		DrawSmall(g, Slot.Settings, ButtonIcon.Settings, _hot == Slot.Settings);
		DrawSmall(g, Slot.More, ButtonIcon.More, _hot == Slot.More);
		DrawSmall(g, Slot.Expand, ButtonIcon.Expand, _hot == Slot.Expand);
		DrawSmall(g, Slot.Close, ButtonIcon.Close, _hot == Slot.Close);
	}

	private void DrawPrimary(Graphics g, Slot slot, ButtonIcon icon, Color accent)
	{
		Rectangle rect = _rects[(int)slot];
		bool flag = IsEnabled(slot);
		bool flag2 = _hot == slot && flag;
		bool flag3 = _pressed == slot && flag2;
		bool active = ((slot == Slot.Record) ? _recording : (slot == Slot.Play && _playing && !_paused));
		Color color = (flag ? (flag3 ? Palette.Blend(accent, Color.Black, 0.22) : (flag2 ? Palette.Blend(accent, Color.White, 0.14) : accent)) : Palette.PanelAlt);
		Color border = (flag ? Palette.Blend(accent, Color.White, 0.18) : Palette.BorderSoft);
		Color color2 = (flag ? ((Luma(accent) > 0.55f) ? Palette.Ink : Color.White) : Palette.Muted);
		using (GraphicsPath path = Hud.Round(rect, S(9)))
		{
			using (SolidBrush brush = new SolidBrush(color))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(border, 1f);
			g.DrawPath(pen, path);
			if (active)
			{
				using Pen pen2 = new Pen(Palette.Alpha(Palette.Text, 160), 1.4f);
				g.DrawPath(pen2, path);
			}
		}
		int num = rect.Width - S(20);
		VectorIcon.Draw(g, icon, new RectangleF(rect.X + (rect.Width - num) / 2f, rect.Y + (rect.Height - num) / 2f, num, num), color2);
	}

	private void DrawSecondary(Graphics g, Slot slot, ButtonIcon icon, bool active)
	{
		Rectangle rect = _rects[(int)slot];
		bool flag = IsEnabled(slot);
		bool flag2 = _hot == slot && flag;
		Color color = (flag2 ? Palette.Alpha(Palette.TextSoft, 30) : Palette.Alpha(Palette.PanelAlt, 205));
		Color border = (active ? Palette.Alpha(Palette.Accent, 190) : Palette.Alpha(Palette.Border, 190));
		Color color2 = ((!flag) ? Palette.Alpha(Palette.Muted, 110) : (active ? Palette.Accent : Palette.Text));
		using (GraphicsPath path = Hud.Round(rect, S(8)))
		{
			using (SolidBrush brush = new SolidBrush(color))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(border, 1f);
			g.DrawPath(pen, path);
		}
		int num = rect.Width - S(18);
		VectorIcon.Draw(g, icon, new RectangleF(rect.X + (rect.Width - num) / 2f, rect.Y + (rect.Height - num) / 2f, num, num), color2);
	}

	private void DrawSmall(Graphics g, Slot slot, ButtonIcon icon, bool active)
	{
		Rectangle rect = _rects[(int)slot];
		bool flag = _hot == slot;
		Color color = (flag ? Palette.Alpha(Palette.TextSoft, 30) : Palette.Alpha(Palette.PanelAlt, 165));
		Color border = (active ? Palette.Alpha(Palette.Accent, 200) : (flag ? Palette.Alpha(Palette.Border, 220) : Palette.Alpha(Palette.BorderSoft, 210)));
		Color color2 = (active ? Palette.Accent : (flag ? Palette.Text : Palette.TextSoft));
		using (GraphicsPath path = Hud.Round(rect, S(5)))
		{
			using (SolidBrush brush = new SolidBrush(color))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(border, 1f);
			g.DrawPath(pen, path);
		}
		int num = rect.Width - S(8);
		VectorIcon.Draw(g, icon, new RectangleF(rect.X + (rect.Width - num) / 2f, rect.Y + (rect.Height - num) / 2f, num, num), color2);
	}

	private void PaintFooter(Graphics g)
	{
		int y = base.Height - S(24);
		string text = "ARRASTE PARA MOVER  ·  DUPLO CLIQUE EXPANDE";
		TextRenderer.DrawText(g, text, UiFont.TinyBold, new Rectangle(S(16), y, base.Width / 2, S(16)), Palette.Alpha(Palette.Muted, 190), TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		Slot hot = _hot;
		if (hot != Slot.None)
		{
			string text2 = Hint(hot);
			TextRenderer.DrawText(g, text2, UiFont.TinyBold, new Rectangle(base.Width / 2, y, base.Width / 2 - S(16), S(16)), Palette.Accent, TextFormatFlags.NoPrefix | TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
		}
		int num = base.Height - S(5);
		int num2 = base.Width - S(16);
		using (SolidBrush brush = new SolidBrush(Palette.Alpha(Palette.Border, 120)))
		{
			g.FillRectangle(brush, S(16), num, num2 - S(16), S(2));
		}
		if (_recording)
		{
			int num3 = S(72);
			int num4 = (int)(Environment.TickCount64 % 1400L);
			int num5 = num4 * num2 / 1400 - num3;
			int num6 = Math.Max(S(16), num5);
			int num7 = Math.Min(num2, num5 + num3) - num6;
			if (num7 > 0)
			{
				using SolidBrush brush2 = new SolidBrush(Palette.Danger);
				g.FillRectangle(brush2, num6, num, num7, S(2));
			}
		}
		else if (_playing && _progress >= 0.0)
		{
			int num8 = (int)((double)(num2 - S(16)) * _progress);
			using (SolidBrush brush3 = new SolidBrush(_paused ? Palette.AccentDeep : Palette.Accent))
			{
				g.FillRectangle(brush3, S(16), num, Math.Max(0, num8), S(2));
			}
		}
	}

	private static float Luma(Color c)
	{
		return (0.299f * (float)c.R + 0.587f * (float)c.G + 0.114f * (float)c.B) / 255f;
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
