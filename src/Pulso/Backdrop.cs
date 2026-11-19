using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace Pulso;

public static class Backdrop
{
	private static Bitmap? _source;

	private static bool _sourceTried;

	private static Bitmap? _graded;

	private static readonly object _sync = new object();

	public static bool Available => Source != null;

	public static Bitmap? Source
	{
		get
		{
			if (!_sourceTried)
			{
				_sourceTried = true;
				_source = LoadSource();
			}
			return _source;
		}
	}

	private static Bitmap? LoadSource()
	{
		try
		{
			string path = Path.Combine(Paths.BaseDir, "backdrop.jpg");
			if (File.Exists(path))
			{
				using FileStream stream = File.OpenRead(path);
				using Bitmap bitmap = new Bitmap(stream);
				return new Bitmap(bitmap);
			}
		}
		catch
		{
		}
		try
		{
			using Stream? stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Pulso.backdrop.jpg");
			if (stream2 != null)
			{
				using Bitmap bitmap2 = new Bitmap(stream2);
				return new Bitmap(bitmap2);
			}
		}
		catch
		{
		}
		return null;
	}

	private static Bitmap? Graded()
	{
		lock (_sync)
		{
			if (_graded != null)
			{
				return _graded;
			}
			Bitmap? source = Source;
			if (source == null)
			{
				return null;
			}
			Bitmap bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				using ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetGamma(1.6f);
				imageAttributes.SetColorMatrix(Saturation(0.5f, 0.03f, 2.1f));
				graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttributes);
			}
			_graded = bitmap;
			return _graded;
		}
	}

	public static void Paint(Graphics g, Rectangle area, Size root, Point offset, float dim)
	{
		if (area.Width <= 0 || area.Height <= 0 || root.Width <= 0 || root.Height <= 0)
		{
			return;
		}
		Bitmap? photo = Graded();
		GraphicsState state = g.Save();
		try
		{
			g.SetClip(area, CombineMode.Intersect);
			if (photo != null)
			{
				PaintPhoto(g, photo, area, root, offset);
			}
			else
			{
				using SolidBrush brush = new SolidBrush(Palette.Bg);
				g.FillRectangle(brush, area);
			}
			PaintOverlays(g, area, root, offset);
			if (dim > 0.001f)
			{
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb(Math.Min(255, (int)(dim * 255f)), 6, 8, 6));
				g.FillRectangle(brush2, area);
			}
		}
		finally
		{
			g.Restore(state);
		}
	}

	private static void PaintPhoto(Graphics g, Bitmap photo, Rectangle area, Size root, Point offset)
	{
		double scale = Math.Max((double)root.Width / (double)photo.Width, (double)root.Height / (double)photo.Height);
		double num = (double)root.Height * 0.16;
		scale = Math.Max(scale, ((double)root.Height + num * 2.0) / (double)photo.Height);
		double num2 = (double)photo.Width * scale;
		double num3 = (double)photo.Height * scale;
		double num4 = ((double)root.Width - num2) / 2.0;
		double num5 = ((double)root.Height - num3) / 2.0 - num;
		double num6 = (double)(offset.X + area.X);
		double num7 = (double)(offset.Y + area.Y);
		double num8 = Math.Max(num6, num4);
		double num9 = Math.Max(num7, num5);
		double num10 = Math.Min(num6 + (double)area.Width, num4 + num2);
		double num11 = Math.Min(num7 + (double)area.Height, num5 + num3);
		double num12 = num10 - num8;
		double num13 = num11 - num9;
		if (num12 <= 0.0 || num13 <= 0.0)
		{
			using SolidBrush brush = new SolidBrush(Palette.Bg);
			g.FillRectangle(brush, area);
			return;
		}
		float x = (float)((double)area.X + (num8 - num6));
		float y = (float)((double)area.Y + (num9 - num7));
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		g.DrawImage(photo, new RectangleF(x, y, (float)num12, (float)num13), new RectangleF((float)((num8 - num4) / scale), (float)((num9 - num5) / scale), (float)(num12 / scale), (float)(num13 / scale)), GraphicsUnit.Pixel);
	}

	private static void PaintOverlays(Graphics g, Rectangle area, Size root, Point offset)
	{
		Rectangle rect = new Rectangle(0, 0, root.Width, root.Height);
		Rectangle clip = new Rectangle(area.X + offset.X, area.Y + offset.Y, area.Width, area.Height);
		GraphicsState state = g.Save();
		try
		{
			g.SetClip(clip, CombineMode.Intersect);
			g.TranslateTransform(-offset.X, -offset.Y);
			using (LinearGradientBrush brush = new LinearGradientBrush(rect, Color.FromArgb(18, 6, 9, 7), Color.FromArgb(126, 6, 9, 7), 90f))
			{
				g.FillRectangle(brush, rect);
			}
			using (LinearGradientBrush brush2 = new LinearGradientBrush(rect, Color.FromArgb(86, 5, 7, 5), Color.FromArgb(0, 5, 7, 5), 0f))
			{
				brush2.InterpolationColors = new ColorBlend
				{
					Colors = new Color[3]
					{
						Color.FromArgb(86, 5, 7, 5),
						Color.FromArgb(0, 5, 7, 5),
						Color.FromArgb(0, 5, 7, 5)
					},
					Positions = new float[3] { 0f, 0.75f, 1f }
				};
				g.FillRectangle(brush2, rect);
			}
			using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(26, 56, 70, 42)))
			{
				g.FillRectangle(brush3, rect);
			}
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddEllipse((float)(-root.Width) * 0.25f, (float)(-root.Height) * 0.4f, (float)root.Width * 1.5f, (float)root.Height * 1.8f);
				using PathGradientBrush brush4 = new PathGradientBrush(path)
				{
					CenterColor = Color.FromArgb(0, 0, 0, 0),
					SurroundColors = new Color[1] { Color.FromArgb(142, 4, 6, 4) },
					FocusScales = new PointF(0.55f, 0.55f)
				};
				g.FillRectangle(brush4, rect);
			}
			using (Pen pen = new Pen(Color.FromArgb(18, 0, 0, 0)))
			{
				for (int i = 0; i < root.Height; i += 3)
				{
					g.DrawLine(pen, 0, i, root.Width, i);
				}
			}
		}
		finally
		{
			g.Restore(state);
		}
	}

	private static ColorMatrix Saturation(float s, float brightness, float exposure)
	{
		float num = 0.3086f;
		float num2 = 0.6094f;
		float num3 = 0.082f;
		float num4 = (1f - s) * num;
		float num5 = (1f - s) * num2;
		float num6 = (1f - s) * num3;
		return new ColorMatrix(new float[5][]
		{
			new float[5] { (num4 + s) * exposure, num4 * exposure, num4 * exposure, 0f, 0f },
			new float[5] { num5 * exposure, (num5 + s) * exposure, num5 * exposure, 0f, 0f },
			new float[5] { num6 * exposure, num6 * exposure, (num6 + s) * exposure, 0f, 0f },
			new float[5] { 0f, 0f, 0f, 1f, 0f },
			new float[5] { brightness, brightness, brightness, 0f, 1f }
		});
	}
}
