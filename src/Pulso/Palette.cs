using System.Drawing;

namespace Pulso;

public static class Palette
{
	public static readonly Color Bg = Color.FromArgb(9, 11, 9);

	public static readonly Color Bg2 = Color.FromArgb(14, 17, 14);

	public static readonly Color Panel = Color.FromArgb(18, 22, 18);

	public static readonly Color PanelAlt = Color.FromArgb(27, 32, 26);

	public static readonly Color PanelHover = Color.FromArgb(37, 43, 35);

	public static readonly Color Border = Color.FromArgb(62, 70, 56);

	public static readonly Color BorderSoft = Color.FromArgb(40, 46, 38);

	public static readonly Color Accent = Color.FromArgb(201, 162, 74);

	public static readonly Color AccentDeep = Color.FromArgb(146, 108, 40);

	public static readonly Color AccentSoft = Color.FromArgb(233, 199, 122);

	public static readonly Color Cyan = Color.FromArgb(146, 176, 106);

	public static readonly Color CyanDim = Color.FromArgb(120, 146, 176, 106);

	public static readonly Color Purple = Color.FromArgb(160, 178, 120);

	public static readonly Color Gold = Color.FromArgb(214, 168, 78);

	public static readonly Color Text = Color.FromArgb(232, 230, 218);

	public static readonly Color TextSoft = Color.FromArgb(186, 188, 172);

	public static readonly Color Muted = Color.FromArgb(138, 143, 124);

	public static readonly Color Danger = Color.FromArgb(206, 74, 52);

	public static readonly Color DangerDeep = Color.FromArgb(158, 46, 32);

	public static readonly Color Success = Color.FromArgb(142, 188, 92);

	public static readonly Color GridLine = Color.FromArgb(34, 39, 32);

	public static readonly Color Selection = Color.FromArgb(62, 64, 40);

	public static readonly Color Sand = Color.FromArgb(201, 162, 74);

	public static readonly Color Hazard = Color.FromArgb(226, 181, 62);

	public static readonly Color Ink = Color.FromArgb(11, 14, 10);

	public static readonly Color Glass = Color.FromArgb(222, 15, 19, 14);

	public static readonly Color GlassSoft = Color.FromArgb(198, 15, 19, 14);

	public static Color Blend(Color a, Color b, double t)
	{
		return Color.FromArgb((int)((double)(int)a.A + (double)(b.A - a.A) * t), (int)((double)(int)a.R + (double)(b.R - a.R) * t), (int)((double)(int)a.G + (double)(b.G - a.G) * t), (int)((double)(int)a.B + (double)(b.B - a.B) * t));
	}

	public static Color Alpha(Color c, int a)
	{
		return Color.FromArgb(a, c.R, c.G, c.B);
	}
}
