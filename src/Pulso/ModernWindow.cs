using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pulso;

public static class ModernWindow
{
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

	private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

	private const int DWMWCP_ROUND = 2;

	public static void Apply(Form f, bool rounded)
	{
		try
		{
			int value = 1;
			if (Native.DwmSetWindowAttribute(f.Handle, 20, ref value, 4) != 0)
			{
				Native.DwmSetWindowAttribute(f.Handle, 19, ref value, 4);
			}
			if (rounded)
			{
				int value2 = 2;
				Native.DwmSetWindowAttribute(f.Handle, 33, ref value2, 4);
			}
		}
		catch
		{
		}
	}

	public static void RoundRegion(Form f, int radius)
	{
		if (f.WindowState == FormWindowState.Maximized || f.Width <= 0 || f.Height <= 0)
		{
			return;
		}
		using GraphicsPath path = Hud.Round(new Rectangle(0, 0, f.Width, f.Height), radius);
		f.Region = new Region(path);
	}
}
