using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pulso;

public static class AppIcon
{
	private static readonly Dictionary<int, Icon> _cache = new Dictionary<int, Icon>();

	public static Icon App => For(48);

	public static Icon Small => For(16);

	public static Icon For(int size)
	{
		lock (_cache)
		{
			if (_cache.TryGetValue(size, out Icon value))
			{
				return value;
			}
			Icon icon = Load(size);
			_cache[size] = icon;
			return icon;
		}
	}

	private static Icon Load(int size)
	{
		Size size2 = new Size(size, size);
		try
		{
			using Stream stream = typeof(AppIcon).Assembly.GetManifestResourceStream("Pulso.app.ico");
			if (stream != null)
			{
				return new Icon(stream, size2);
			}
		}
		catch
		{
		}
		try
		{
			Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			if (icon != null)
			{
				return new Icon(icon, size2);
			}
		}
		catch
		{
		}
		return (Icon)SystemIcons.Application.Clone();
	}
}
