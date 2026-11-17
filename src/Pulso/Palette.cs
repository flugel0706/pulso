using System.Drawing;

namespace Pulso;

public static class Palette
{
	public static readonly Color Bg = Color.FromArgb(12, 13, 18);

	public static readonly Color Bg2 = Color.FromArgb(17, 19, 26);

	public static readonly Color Panel = Color.FromArgb(21, 24, 33);

	public static readonly Color PanelAlt = Color.FromArgb(28, 32, 45);

	public static readonly Color PanelHover = Color.FromArgb(35, 40, 56);

	public static readonly Color Border = Color.FromArgb(43, 48, 64);

	public static readonly Color BorderSoft = Color.FromArgb(33, 37, 49);

	public static readonly Color Accent = Color.FromArgb(139, 124, 255);

	public static readonly Color AccentDeep = Color.FromArgb(104, 86, 235);

	public static readonly Color AccentSoft = Color.FromArgb(170, 158, 255);

	public static readonly Color Cyan = Accent;

	public static readonly Color CyanDim = Color.FromArgb(120, 139, 124, 255);

	public static readonly Color Purple = Color.FromArgb(167, 139, 250);

	public static readonly Color Gold = Color.FromArgb(245, 184, 76);

	public static readonly Color Text = Color.FromArgb(233, 235, 242);

	public static readonly Color TextSoft = Color.FromArgb(184, 189, 204);

	public static readonly Color Muted = Color.FromArgb(134, 140, 158);

	public static readonly Color Danger = Color.FromArgb(242, 109, 125);

	public static readonly Color DangerDeep = Color.FromArgb(205, 74, 92);

	public static readonly Color Success = Color.FromArgb(94, 224, 160);

	public static readonly Color GridLine = Color.FromArgb(33, 37, 49);

	public static readonly Color Selection = Color.FromArgb(48, 44, 82);

	public static Color Blend(Color a, Color b, double t)
	{
		return Color.FromArgb((int)((double)(int)a.A + (double)(b.A - a.A) * t), (int)((double)(int)a.R + (double)(b.R - a.R) * t), (int)((double)(int)a.G + (double)(b.G - a.G) * t), (int)((double)(int)a.B + (double)(b.B - a.B) * t));
	}

	public static Color Alpha(Color c, int a)
	{
		return Color.FromArgb(a, c.R, c.G, c.B);
	}
}
