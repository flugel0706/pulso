using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class StatusDot : Control
{
	private bool _on;

	public bool On
	{
		get
		{
			return _on;
		}
		set
		{
			_on = value;
			Invalidate();
		}
	}

	public Color Tint { get; set; } = Palette.Success;


	public StatusDot()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Color.Transparent;
		base.Size = new Size(12, 12);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Color tint = Tint;
		using SolidBrush brush = new SolidBrush(Color.FromArgb(On ? 55 : 40, tint));
		graphics.FillEllipse(brush, 0, 0, base.Width - 1, base.Height - 1);
		using SolidBrush brush2 = new SolidBrush(tint);
		graphics.FillEllipse(brush2, 3, 3, base.Width - 7, base.Height - 7);
	}
}
