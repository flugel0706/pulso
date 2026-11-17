using System;
using System.IO;

namespace Pulso;

public static class Paths
{
	private static string? _base;

	public static string BaseDir
	{
		get
		{
			if (_base != null)
			{
				return _base;
			}
			string baseDirectory = AppContext.BaseDirectory;
			try
			{
				string path = Path.Combine(baseDirectory, ".pulso_probe");
				File.WriteAllText(path, "x");
				File.Delete(path);
				_base = baseDirectory;
			}
			catch
			{
				_base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pulso");
			}
			Directory.CreateDirectory(_base);
			return _base;
		}
	}

	public static string SettingsFile => Path.Combine(BaseDir, "Pulso.config.json");

	public static string LogFile => Path.Combine(BaseDir, "Pulso.log");

	public static string MacrosDir => Path.Combine(BaseDir, "macros");

	public static string DefaultMacroFile
	{
		get
		{
			Directory.CreateDirectory(MacrosDir);
			return Path.Combine(MacrosDir, "macro.gmt");
		}
	}
}
