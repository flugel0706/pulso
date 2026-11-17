using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Pulso;

internal static class VectorIcon
{
	public static void Draw(Graphics g, ButtonIcon icon, RectangleF r, Color color)
	{
		if (icon == ButtonIcon.None)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float num = r.X + r.Width / 2f;
		float num2 = r.Y + r.Height / 2f;
		float num3 = Math.Min(r.Width, r.Height);
		using Pen pen = new Pen(color, Math.Max(1.6f, num3 / 9f))
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round,
			LineJoin = LineJoin.Round
		};
		using SolidBrush brush = new SolidBrush(color);
		switch (icon)
		{
		case ButtonIcon.Record:
			g.FillEllipse(brush, num - num3 * 0.24f, num2 - num3 * 0.24f, num3 * 0.48f, num3 * 0.48f);
			break;
		case ButtonIcon.Play:
			g.FillPolygon(brush, new PointF[3]
			{
				new PointF(num - num3 * 0.19f, num2 - num3 * 0.27f),
				new PointF(num + num3 * 0.27f, num2),
				new PointF(num - num3 * 0.19f, num2 + num3 * 0.27f)
			});
			break;
		case ButtonIcon.Pause:
		{
			using (GraphicsPath path2 = Hud.Round(new RectangleF(num - num3 * 0.24f, num2 - num3 * 0.25f, num3 * 0.16f, num3 * 0.5f), (int)(num3 * 0.07f)))
			{
				g.FillPath(brush, path2);
			}
			using GraphicsPath path3 = Hud.Round(new RectangleF(num + num3 * 0.08f, num2 - num3 * 0.25f, num3 * 0.16f, num3 * 0.5f), (int)(num3 * 0.07f));
			g.FillPath(brush, path3);
			break;
		}
		case ButtonIcon.Stop:
		{
			using GraphicsPath path = Hud.Round(new RectangleF(num - num3 * 0.23f, num2 - num3 * 0.23f, num3 * 0.46f, num3 * 0.46f), (int)(num3 * 0.08f));
			g.FillPath(brush, path);
			break;
		}
		case ButtonIcon.Folder:
			g.DrawLines(pen, new PointF[8]
			{
				new PointF(num - num3 * 0.3f, num2 - num3 * 0.14f),
				new PointF(num - num3 * 0.3f, num2 + num3 * 0.22f),
				new PointF(num + num3 * 0.3f, num2 + num3 * 0.22f),
				new PointF(num + num3 * 0.3f, num2 - num3 * 0.08f),
				new PointF(num - num3 * 0.06f, num2 - num3 * 0.08f),
				new PointF(num - num3 * 0.14f, num2 - num3 * 0.22f),
				new PointF(num - num3 * 0.3f, num2 - num3 * 0.22f),
				new PointF(num - num3 * 0.3f, num2 - num3 * 0.14f)
			});
			break;
		case ButtonIcon.Save:
			g.DrawRectangle(pen, num - num3 * 0.27f, num2 - num3 * 0.27f, num3 * 0.54f, num3 * 0.54f);
			g.FillRectangle(brush, num - num3 * 0.15f, num2 - num3 * 0.27f, num3 * 0.3f, num3 * 0.17f);
			g.DrawRectangle(pen, num - num3 * 0.14f, num2 + num3 * 0.05f, num3 * 0.28f, num3 * 0.22f);
			break;
		case ButtonIcon.Export:
			g.DrawLine(pen, num - num3 * 0.05f, num2 + num3 * 0.26f, num - num3 * 0.05f, num2 + num3 * 0.04f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 + num3 * 0.04f, num - num3 * 0.22f, num2 + num3 * 0.04f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 + num3 * 0.04f, num + num3 * 0.12f, num2 + num3 * 0.04f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 + num3 * 0.04f, num - num3 * 0.05f, num2 - num3 * 0.26f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 - num3 * 0.26f, num - num3 * 0.16f, num2 - num3 * 0.15f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 - num3 * 0.26f, num + num3 * 0.06f, num2 - num3 * 0.15f);
			g.DrawLine(pen, num - num3 * 0.28f, num2 + num3 * 0.18f, num - num3 * 0.28f, num2 + num3 * 0.28f);
			g.DrawLine(pen, num - num3 * 0.28f, num2 + num3 * 0.28f, num + num3 * 0.18f, num2 + num3 * 0.28f);
			g.DrawLine(pen, num + num3 * 0.18f, num2 + num3 * 0.28f, num + num3 * 0.18f, num2 + num3 * 0.18f);
			break;
		case ButtonIcon.Settings:
		{
			for (int i = 0; i < 8; i++)
			{
				double num4 = (double)i * Math.PI / 4.0;
				float x = num + (float)Math.Cos(num4) * num3 * 0.18f;
				float y = num2 + (float)Math.Sin(num4) * num3 * 0.18f;
				float x2 = num + (float)Math.Cos(num4) * num3 * 0.32f;
				float y2 = num2 + (float)Math.Sin(num4) * num3 * 0.32f;
				g.DrawLine(pen, x, y, x2, y2);
			}
			g.DrawEllipse(pen, num - num3 * 0.16f, num2 - num3 * 0.16f, num3 * 0.32f, num3 * 0.32f);
			g.FillEllipse(brush, num - num3 * 0.05f, num2 - num3 * 0.05f, num3 * 0.1f, num3 * 0.1f);
			break;
		}
		case ButtonIcon.Expand:
			g.DrawLine(pen, num - num3 * 0.24f, num2 - num3 * 0.1f, num - num3 * 0.24f, num2 - num3 * 0.24f);
			g.DrawLine(pen, num - num3 * 0.24f, num2 - num3 * 0.24f, num - num3 * 0.1f, num2 - num3 * 0.24f);
			g.DrawLine(pen, num + num3 * 0.1f, num2 - num3 * 0.24f, num + num3 * 0.24f, num2 - num3 * 0.24f);
			g.DrawLine(pen, num + num3 * 0.24f, num2 - num3 * 0.24f, num + num3 * 0.24f, num2 - num3 * 0.1f);
			g.DrawLine(pen, num - num3 * 0.24f, num2 + num3 * 0.24f, num - num3 * 0.1f, num2 + num3 * 0.24f);
			g.DrawLine(pen, num - num3 * 0.24f, num2 + num3 * 0.24f, num - num3 * 0.24f, num2 + num3 * 0.1f);
			g.DrawLine(pen, num + num3 * 0.1f, num2 + num3 * 0.24f, num + num3 * 0.24f, num2 + num3 * 0.24f);
			g.DrawLine(pen, num + num3 * 0.24f, num2 + num3 * 0.24f, num + num3 * 0.24f, num2 + num3 * 0.1f);
			break;
		case ButtonIcon.Minimize:
			g.DrawLine(pen, num - num3 * 0.22f, num2, num + num3 * 0.22f, num2);
			break;
		case ButtonIcon.Maximize:
			g.DrawRectangle(pen, num - num3 * 0.19f, num2 - num3 * 0.19f, num3 * 0.38f, num3 * 0.38f);
			break;
		case ButtonIcon.Restore:
			g.DrawRectangle(pen, num - num3 * 0.22f, num2 - num3 * 0.14f, num3 * 0.3f, num3 * 0.3f);
			g.DrawRectangle(pen, num - num3 * 0.08f, num2 - num3 * 0.24f, num3 * 0.3f, num3 * 0.3f);
			break;
		case ButtonIcon.Close:
			g.DrawLine(pen, num - num3 * 0.17f, num2 - num3 * 0.17f, num + num3 * 0.17f, num2 + num3 * 0.17f);
			g.DrawLine(pen, num + num3 * 0.17f, num2 - num3 * 0.17f, num - num3 * 0.17f, num2 + num3 * 0.17f);
			break;
		case ButtonIcon.Infinity:
			g.DrawEllipse(pen, num - num3 * 0.32f, num2 - num3 * 0.16f, num3 * 0.3f, num3 * 0.32f);
			g.DrawEllipse(pen, num + num3 * 0.02f, num2 - num3 * 0.16f, num3 * 0.3f, num3 * 0.32f);
			break;
		case ButtonIcon.Minus:
			g.DrawLine(pen, num - num3 * 0.2f, num2, num + num3 * 0.2f, num2);
			break;
		case ButtonIcon.Plus:
			g.DrawLine(pen, num - num3 * 0.2f, num2, num + num3 * 0.2f, num2);
			g.DrawLine(pen, num, num2 - num3 * 0.2f, num, num2 + num3 * 0.2f);
			break;
		case ButtonIcon.Wave:
			g.DrawLines(pen, new PointF[6]
			{
				new PointF(num - num3 * 0.3f, num2),
				new PointF(num - num3 * 0.15f, num2),
				new PointF(num - num3 * 0.05f, num2 - num3 * 0.2f),
				new PointF(num + num3 * 0.07f, num2 + num3 * 0.2f),
				new PointF(num + num3 * 0.17f, num2),
				new PointF(num + num3 * 0.3f, num2)
			});
			break;
		case ButtonIcon.Trash:
			g.DrawLine(pen, num - num3 * 0.24f, num2 - num3 * 0.2f, num + num3 * 0.24f, num2 - num3 * 0.2f);
			g.DrawLine(pen, num - num3 * 0.08f, num2 - num3 * 0.28f, num + num3 * 0.08f, num2 - num3 * 0.28f);
			g.DrawLine(pen, num - num3 * 0.17f, num2 - num3 * 0.2f, num - num3 * 0.14f, num2 + num3 * 0.26f);
			g.DrawLine(pen, num + num3 * 0.17f, num2 - num3 * 0.2f, num + num3 * 0.14f, num2 + num3 * 0.26f);
			g.DrawLine(pen, num - num3 * 0.14f, num2 + num3 * 0.26f, num + num3 * 0.14f, num2 + num3 * 0.26f);
			g.DrawLine(pen, num - num3 * 0.05f, num2 - num3 * 0.1f, num - num3 * 0.05f, num2 + num3 * 0.17f);
			g.DrawLine(pen, num + num3 * 0.05f, num2 - num3 * 0.1f, num + num3 * 0.05f, num2 + num3 * 0.17f);
			break;
		case ButtonIcon.Chevron:
			g.DrawLine(pen, num - num3 * 0.14f, num2 - num3 * 0.05f, num, num2 + num3 * 0.1f);
			g.DrawLine(pen, num, num2 + num3 * 0.1f, num + num3 * 0.14f, num2 - num3 * 0.05f);
			break;
		case ButtonIcon.More:
			g.FillEllipse(brush, num - num3 * 0.22f, num2 - num3 * 0.045f, num3 * 0.09f, num3 * 0.09f);
			g.FillEllipse(brush, num - num3 * 0.045f, num2 - num3 * 0.045f, num3 * 0.09f, num3 * 0.09f);
			g.FillEllipse(brush, num + num3 * 0.13f, num2 - num3 * 0.045f, num3 * 0.09f, num3 * 0.09f);
			break;
		}
	}
}
