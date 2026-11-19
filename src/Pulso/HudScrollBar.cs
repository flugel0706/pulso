using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class HudScrollBar : Control
{
	private bool _dragging;

	private int _dragOffset;

	private Rectangle _thumb;

	public DataGridView? Grid { get; set; }

	public HudScrollBar()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, value: true);
		BackColor = Color.Transparent;
		Cursor = Cursors.Hand;
		TabStop = false;
	}

	private int Total => Grid?.RowCount ?? 0;

	private int VisibleRows => Grid?.DisplayedRowCount(includePartialRow: false) ?? 0;

	private int First
	{
		get
		{
			if (Grid == null || Grid.RowCount == 0)
			{
				return 0;
			}
			return Math.Max(0, Grid.FirstDisplayedScrollingRowIndex);
		}
	}

	public void Sync()
	{
		_thumb = ThumbRect();
		Invalidate();
	}

	private Rectangle TrackRect()
	{
		return new Rectangle(base.Width / 2 - 2, 2, 4, Math.Max(1, base.Height - 4));
	}

	private Rectangle ThumbRect()
	{
		int total = Total;
		int visible = VisibleRows;
		Rectangle track = TrackRect();
		if (total <= 0 || visible <= 0 || total <= visible)
		{
			return Rectangle.Empty;
		}
		int num = Math.Max(28, (int)((long)track.Height * visible / total));
		int num2 = Math.Max(0, total - visible);
		int num3 = Math.Min(track.Height - num, (int)((long)(track.Height - num) * Math.Min(First, num2) / num2));
		return new Rectangle(base.Width / 2 - 3, track.Y + num3, 7, num);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle rectangle = TrackRect();
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, Palette.Text)))
		{
			graphics.FillRectangle(brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
		_thumb = ThumbRect();
		if (_thumb.IsEmpty)
		{
			return;
		}
		Color color = (base.Enabled ? Palette.Accent : Palette.Muted);
		if (_dragging)
		{
			color = Palette.Blend(color, Color.White, 0.2f);
		}
		using GraphicsPath path = Hud.Round(_thumb, 3);
		using SolidBrush brush2 = new SolidBrush(Palette.Alpha(color, 215));
		graphics.FillPath(brush2, path);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left || Grid == null)
		{
			return;
		}
		_thumb = ThumbRect();
		if (_thumb.Contains(e.Location))
		{
			_dragging = true;
			_dragOffset = e.Y - _thumb.Top;
			Capture = true;
			Invalidate();
			return;
		}
		SetFirst(First + ((e.Y < _thumb.Top) ? (-VisibleRows) : VisibleRows));
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (!_dragging || Grid == null)
		{
			return;
		}
		int total = Total;
		int num = Math.Max(1, total - VisibleRows);
		Rectangle track = TrackRect();
		int num2 = Math.Max(1, track.Height - _thumb.Height);
		int num3 = Math.Max(0, Math.Min(num2, e.Y - _dragOffset - track.Top));
		SetFirst((int)Math.Round((double)num3 * (double)num / (double)num2));
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (_dragging)
		{
			_dragging = false;
			Capture = false;
			Invalidate();
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (Grid != null)
		{
			SetFirst(First - Math.Sign(e.Delta) * 3);
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		Invalidate();
	}

	private void SetFirst(int row)
	{
		if (Grid == null || Grid.RowCount == 0)
		{
			return;
		}
		int num = Math.Max(0, Grid.RowCount - VisibleRows);
		int firstDisplayedScrollingRowIndex = Math.Max(0, Math.Min(num, row));
		if (Grid.FirstDisplayedScrollingRowIndex != firstDisplayedScrollingRowIndex)
		{
			Grid.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
		}
		Sync();
	}
}
