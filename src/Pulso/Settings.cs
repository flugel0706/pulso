using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Pulso;

public sealed class Settings
{
	public string HotkeyRecord { get; set; } = "Ctrl+Alt+R";


	public string HotkeyPlay { get; set; } = "Ctrl+Alt+P";


	public string HotkeyPause { get; set; } = "Ctrl+Alt+Espaço";


	public string HotkeyStop { get; set; } = "Ctrl+Alt+S";


	public string HotkeyCompact { get; set; } = "Ctrl+Alt+M";


	public int SpeedPercent { get; set; } = 100;


	public bool Instant { get; set; }

	public int Loops { get; set; } = 1;


	public bool Continuous { get; set; }

	public int LoopDelayMs { get; set; }

	public int StartDelayMs { get; set; }

	public bool RecordMouse { get; set; } = true;


	public bool RecordKeyboard { get; set; } = true;


	public bool IgnoreOwnWindow { get; set; } = true;


	public bool RelativeCoords { get; set; }

	public bool PlayToRecordedWindow { get; set; }

	public bool UseScanCodes { get; set; } = true;


	public bool TopMost { get; set; } = true;


	public bool MinimizeToTray { get; set; }

	public bool MinimizeToBar { get; set; }

	public bool ConfirmClear { get; set; } = true;


	public bool ShowTrayBubble { get; set; } = true;


	public string LastFolder { get; set; } = "";

	public List<string> RecentFiles { get; set; } = new List<string>();


	public static string PathFile => Paths.SettingsFile;

	public Settings Clone()
	{
		return (Settings)MemberwiseClone();
	}

	public static Settings Load()
	{
		try
		{
			if (File.Exists(PathFile))
			{
				Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(PathFile, Encoding.UTF8), Macro.JsonOpts);
				if (settings != null)
				{
					return Sanitize(settings);
				}
			}
		}
		catch (Exception ex)
		{
			AppLog.Write("Falha ao ler configurações: " + ex.Message);
		}
		return Sanitize(new Settings());
	}

	private static Settings Sanitize(Settings s)
	{
		s.SpeedPercent = Math.Clamp(s.SpeedPercent, 5, 2000);
		s.Loops = Math.Clamp(s.Loops, 1, 100000);
		s.LoopDelayMs = Math.Clamp(s.LoopDelayMs, 0, 3600000);
		s.StartDelayMs = Math.Clamp(s.StartDelayMs, 0, 60000);
		if (!Hotkey.Parse(s.HotkeyRecord).IsValid)
		{
			s.HotkeyRecord = "Ctrl+Alt+R";
		}
		if (!Hotkey.Parse(s.HotkeyPlay).IsValid)
		{
			s.HotkeyPlay = "Ctrl+Alt+P";
		}
		if (!Hotkey.Parse(s.HotkeyPause).IsValid)
		{
			s.HotkeyPause = "Ctrl+Alt+Espaço";
		}
		if (!Hotkey.Parse(s.HotkeyStop).IsValid)
		{
			s.HotkeyStop = "Ctrl+Alt+S";
		}
		if (!Hotkey.Parse(s.HotkeyCompact).IsValid)
		{
			s.HotkeyCompact = "Ctrl+Alt+M";
		}
		if (s.RecentFiles == null)
		{
			s.RecentFiles = new List<string>();
		}
		s.RecentFiles.RemoveAll((string p) => string.IsNullOrWhiteSpace(p));
		if (s.RecentFiles.Count > 8)
		{
			s.RecentFiles.RemoveRange(8, s.RecentFiles.Count - 8);
		}
		return s;
	}

	public void Save()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(PathFile));
			File.WriteAllText(PathFile, JsonSerializer.Serialize(this, Macro.JsonOpts), Encoding.UTF8);
		}
		catch (Exception ex)
		{
			AppLog.Write("Falha ao salvar configurações: " + ex.Message);
		}
	}
}
