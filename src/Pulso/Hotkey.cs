using System;
using System.Collections.Generic;
using System.Text;

namespace Pulso;

public sealed class Hotkey
{
	private static readonly Dictionary<string, int> Named = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{
		["Backspace"] = 8,
		["Tab"] = 9,
		["Enter"] = 13,
		["Return"] = 13,
		["Esc"] = 27,
		["Escape"] = 27,
		["Space"] = 32,
		["Espaço"] = 32,
		["Espaco"] = 32,
		["PageUp"] = 33,
		["PageDown"] = 34,
		["End"] = 35,
		["Home"] = 36,
		["Left"] = 37,
		["Up"] = 38,
		["Right"] = 39,
		["Down"] = 40,
		["PrintScreen"] = 44,
		["PrtSc"] = 44,
		["Insert"] = 45,
		["Delete"] = 46,
		["Del"] = 46,
		["NumLock"] = 144,
		["ScrollLock"] = 145,
		["CapsLock"] = 20,
		["Pause"] = 19,
		["Apps"] = 93,
		["Multiply"] = 106,
		["Add"] = 107,
		["Subtract"] = 109,
		["Decimal"] = 110,
		["Divide"] = 111,
		[";"] = 186,
		["="] = 187,
		[","] = 188,
		["-"] = 189,
		["."] = 190,
		["/"] = 191,
		["`"] = 192,
		["["] = 219,
		["\\"] = 220,
		["]"] = 221,
		["'"] = 222
	};

	public bool Ctrl { get; set; }

	public bool Alt { get; set; }

	public bool Shift { get; set; }

	public bool Win { get; set; }

	public int Vk { get; set; }

	public bool IsValid => Vk != 0;

	public static Hotkey None => new Hotkey
	{
		Vk = 0
	};

	public Hotkey Clone()
	{
		return new Hotkey
		{
			Ctrl = Ctrl,
			Alt = Alt,
			Shift = Shift,
			Win = Win,
			Vk = Vk
		};
	}

	public bool SameAs(Hotkey? o)
	{
		if (o != null && Ctrl == o.Ctrl && Alt == o.Alt && Shift == o.Shift && Win == o.Win)
		{
			return Vk == o.Vk;
		}
		return false;
	}

	public static Hotkey Parse(string? s)
	{
		Hotkey hotkey = new Hotkey();
		if (string.IsNullOrWhiteSpace(s))
		{
			return hotkey;
		}
		string[] array = s.Split('+');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			bool flag = i == array.Length - 1;
			if (text.Length == 0)
			{
				if (flag)
				{
					hotkey.Vk = 187;
				}
				continue;
			}
			switch (text.ToLowerInvariant())
			{
			case "ctrl":
			case "control":
				hotkey.Ctrl = true;
				continue;
			case "menu":
			case "alt":
				hotkey.Alt = true;
				continue;
			case "shift":
				hotkey.Shift = true;
				continue;
			case "windows":
			case "win":
				hotkey.Win = true;
				continue;
			}
			if (flag && TryVk(text, out var vk))
			{
				hotkey.Vk = vk;
			}
		}
		return hotkey;
	}

	public override string ToString()
	{
		if (Vk == 0)
		{
			return "(nenhum)";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (Ctrl)
		{
			stringBuilder.Append("Ctrl+");
		}
		if (Alt)
		{
			stringBuilder.Append("Alt+");
		}
		if (Shift)
		{
			stringBuilder.Append("Shift+");
		}
		if (Win)
		{
			stringBuilder.Append("Win+");
		}
		stringBuilder.Append(VkName(Vk));
		return stringBuilder.ToString();
	}

	public static bool TryVk(string name, out int vk)
	{
		vk = 0;
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (Named.TryGetValue(name, out vk))
		{
			return true;
		}
		if (name.Length == 1)
		{
			char c = char.ToUpperInvariant(name[0]);
			if (c >= 'A' && c <= 'Z')
			{
				vk = c;
				return true;
			}
			if (c >= '0' && c <= '9')
			{
				vk = c;
				return true;
			}
		}
		else
		{
			char c2 = name[0];
			bool flag = ((c2 == 'F' || c2 == 'f') ? true : false);
			if (flag && int.TryParse(name.AsSpan(1), out var result) && result >= 1 && result <= 24)
			{
				vk = 111 + result;
				return true;
			}
			if (name.Length >= 6 && name.StartsWith("NUMPAD", StringComparison.OrdinalIgnoreCase) && int.TryParse(name.AsSpan(6), out var result2) && result2 >= 0 && result2 <= 9)
			{
				vk = 96 + result2;
				return true;
			}
		}
		return false;
	}

	public static string VkName(int vk)
	{
		if (vk == 32)
		{
			return "Espaço";
		}
		foreach (KeyValuePair<string, int> item in Named)
		{
			if (item.Value == vk && item.Key.Length > 1)
			{
				return item.Key;
			}
		}
		if (vk >= 48 && vk <= 57)
		{
			return ((char)vk).ToString();
		}
		if (vk >= 65 && vk <= 90)
		{
			return ((char)vk).ToString();
		}
		if (vk >= 96 && vk <= 105)
		{
			return "Numpad" + (vk - 96);
		}
		if (vk >= 112 && vk <= 135)
		{
			return "F" + (vk - 111);
		}
		foreach (KeyValuePair<string, int> item2 in Named)
		{
			if (item2.Value == vk)
			{
				return item2.Key;
			}
		}
		return "VK_" + vk.ToString("X2");
	}
}
