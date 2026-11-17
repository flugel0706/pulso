using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public sealed class HudGrid : DataGridView
{
	public string EmptyTitle { get; set; } = "Nenhuma automação por aqui";


	public string EmptyHint { get; set; } = "Grave sua primeira macro ou abra um arquivo .gmt";


	public HudGrid()
	{
		DoubleBuffered = true;
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (base.RowCount > 0 || base.DesignMode)
		{
			return;
		}
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle clientRectangle = base.ClientRectangle;
		float num = (float)base.DeviceDpi / 96f;
		int num2 = clientRectangle.Width / 2;
		int num3 = clientRectangle.Height / 2 - (int)(22f * num);
		Rectangle rectangle = new Rectangle(num2 - (int)(23f * num), num3 - (int)(44f * num), (int)(46f * num), (int)(46f * num));
		using (GraphicsPath path = Hud.Round(rectangle, (int)(14f * num)))
		{
			using SolidBrush brush = new SolidBrush(Palette.Alpha(Palette.Accent, 26));
			graphics.FillPath(brush, path);
			using Pen pen = new Pen(Palette.Alpha(Palette.Accent, 120), 1f);
			graphics.DrawPath(pen, path);
		}
		VectorIcon.Draw(graphics, ButtonIcon.Wave, rectangle, Palette.AccentSoft);
		TextRenderer.DrawText(graphics, EmptyTitle, UiFont.Title, new Rectangle(clientRectangle.X, num3 + (int)(14f * num), clientRectangle.Width, (int)(26f * num)), Palette.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		TextRenderer.DrawText(graphics, EmptyHint, UiFont.Body, new Rectangle(clientRectangle.X, num3 + (int)(40f * num), clientRectangle.Width, (int)(24f * num)), Palette.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		string text = "Dica: arraste um arquivo .gmt, .rec ou .json para esta janela";
		TextRenderer.DrawText(graphics, text, UiFont.Tiny, new Rectangle(clientRectangle.X, num3 + (int)(72f * num), clientRectangle.Width, (int)(18f * num)), Palette.Alpha(Palette.Muted, 170), TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
	}
}
