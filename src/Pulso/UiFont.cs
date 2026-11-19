using System;
using System.Drawing;

namespace Pulso;

public static class UiFont
{
	public static readonly Font Display = Pick(new string[4] { "Bahnschrift SemiBold Condensed", "Bahnschrift SemiCondensed", "Segoe UI Semibold", "Segoe UI" }, 16f, FontStyle.Regular);

	public static readonly Font Title = Pick(new string[4] { "Bahnschrift SemiBold Condensed", "Bahnschrift SemiCondensed", "Segoe UI Semibold", "Segoe UI" }, 12.5f, FontStyle.Regular);

	public static readonly Font Head = Pick(new string[3] { "Bahnschrift SemiBold", "Bahnschrift", "Segoe UI Semibold" }, 9.75f, FontStyle.Regular);

	public static readonly Font Body = Pick(new string[2] { "Segoe UI Variable Text", "Segoe UI" }, 9.25f, FontStyle.Regular);

	public static readonly Font BodyBold = Pick(new string[3] { "Segoe UI Variable Text", "Segoe UI Semibold", "Segoe UI" }, 9.25f, FontStyle.Bold);

	public static readonly Font Tiny = Pick(new string[2] { "Segoe UI Variable Text", "Segoe UI" }, 7.75f, FontStyle.Regular);

	public static readonly Font TinyBold = Pick(new string[3] { "Segoe UI Variable Text", "Segoe UI Semibold", "Segoe UI" }, 7.75f, FontStyle.Bold);

	public static readonly Font Mono = Pick(new string[2] { "Cascadia Mono", "Consolas" }, 9f, FontStyle.Regular);

	public static readonly Font MonoSmall = Pick(new string[2] { "Cascadia Mono", "Consolas" }, 8f, FontStyle.Regular);

	public static readonly Font Hero = Pick(new string[3] { "Bahnschrift SemiBold Condensed", "Bahnschrift Condensed", "Segoe UI" }, 24f, FontStyle.Regular);

	public static readonly Font HeroSmall = Pick(new string[3] { "Bahnschrift SemiBold Condensed", "Bahnschrift Condensed", "Segoe UI" }, 13f, FontStyle.Regular);

	private static Font Pick(string[] names, float size, FontStyle style)
	{
		foreach (string text in names)
		{
			try
			{
				Font font = new Font(text, size, style);
				if (font.Name.Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					return font;
				}
				font.Dispose();
			}
			catch
			{
			}
		}
		return new Font(FontFamily.GenericSansSerif, size, style);
	}
}
