using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public static class HudChrome
{
	public static void Attach(Form f, string title, bool shiftAbsolute = false)
	{
		Form f2 = f;
		string title2 = title;
		int num = (int)Math.Round(46f * Math.Max(1f, (float)f2.DeviceDpi / 96f));
		f2.FormBorderStyle = FormBorderStyle.None;
		f2.BackColor = Palette.Bg;
		f2.Padding = new Padding(1);
		Panel bar = new Panel
		{
			Dock = DockStyle.Top,
			Height = num,
			BackColor = Palette.Bg
		};
		HudButton close = new HudButton
		{
			Icon = ButtonIcon.Close,
			Kind = HudKind.Bare,
			Width = (int)(40f * Math.Max(1f, (float)f2.DeviceDpi / 96f)),
			Height = num - (int)(12f * Math.Max(1f, (float)f2.DeviceDpi / 96f)),
			Radius = 7,
			LeftAlign = false
		};
		close.Click += delegate
		{
			f2.DialogResult = DialogResult.Cancel;
			f2.Close();
		};
		bar.Controls.Add(close);
		bar.Resize += delegate
		{
			close.Location = new Point(bar.Width - close.Width - 4, (bar.Height - close.Height) / 2);
			bar.Invalidate();
		};
		bar.Paint += delegate(object? _, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (SolidBrush brush = new SolidBrush(Palette.Bg))
			{
				graphics.FillRectangle(brush, bar.ClientRectangle);
			}
			Rectangle rectangle = new Rectangle((int)(16f * ((float)f2.DeviceDpi / 96f)), (bar.Height - (int)(22f * ((float)f2.DeviceDpi / 96f))) / 2, (int)(22f * ((float)f2.DeviceDpi / 96f)), (int)(22f * ((float)f2.DeviceDpi / 96f)));
			using (GraphicsPath path = Hud.Round(rectangle, 7))
			{
				using LinearGradientBrush brush2 = new LinearGradientBrush(rectangle, Palette.AccentSoft, Palette.AccentDeep, 55f);
				graphics.FillPath(brush2, path);
			}
			using (Pen pen2 = new Pen(Color.White, 1.6f)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			})
			{
				float num2 = (float)rectangle.X + (float)rectangle.Width / 2f;
				float num3 = (float)rectangle.Y + (float)rectangle.Height / 2f;
				float num4 = rectangle.Width;
				graphics.DrawLines(pen2, new PointF[6]
				{
					new PointF(num2 - num4 * 0.3f, num3),
					new PointF(num2 - num4 * 0.1f, num3),
					new PointF(num2, num3 - num4 * 0.2f),
					new PointF(num2 + num4 * 0.1f, num3 + num4 * 0.2f),
					new PointF(num2 + num4 * 0.2f, num3),
					new PointF(num2 + num4 * 0.32f, num3)
				});
			}
			TextRenderer.DrawText(bounds: new Rectangle(rectangle.Right + (int)(10f * ((float)f2.DeviceDpi / 96f)), 0, Math.Max(40, bar.Width - rectangle.Right - 70), bar.Height), dc: graphics, text: title2, font: UiFont.Head, foreColor: Palette.Text, flags: TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
			using Pen pen3 = new Pen(Palette.BorderSoft);
			graphics.DrawLine(pen3, 0, bar.Height - 1, bar.Width, bar.Height - 1);
		};
		bar.MouseDown += delegate(object? _, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Native.ReleaseCapture();
				Native.SendMessage(f2.Handle, 161, 2, IntPtr.Zero);
			}
		};
		if (shiftAbsolute)
		{
			f2.ClientSize = new Size(f2.ClientSize.Width, f2.ClientSize.Height + num);
			foreach (Control control in f2.Controls)
			{
				control.Top += num;
			}
			f2.Controls.Add(bar);
		}
		else
		{
			f2.ClientSize = new Size(f2.ClientSize.Width, f2.ClientSize.Height + num);
			f2.Controls.Add(bar);
		}
		f2.Paint += delegate(object? _, PaintEventArgs e)
		{
			using Pen pen = new Pen(Palette.Border);
			e.Graphics.DrawRectangle(pen, 0, 0, f2.Width - 1, f2.Height - 1);
		};
		f2.Shown += delegate
		{
			ModernWindow.Apply(f2, rounded: true);
		};
	}
}
