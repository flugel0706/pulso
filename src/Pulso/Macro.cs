using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulso;

public sealed class Macro
{
	public static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	public const int RecSize = 20;

	public string App { get; set; } = "Pulso";


	public int Version { get; set; } = 1;


	public string Created { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


	public int ScreenW { get; set; }

	public int ScreenH { get; set; }

	public long Hwnd { get; set; }

	public int WinLeft { get; set; }

	public int WinTop { get; set; }

	public List<MacroEvent> Events { get; set; } = new List<MacroEvent>();


	[JsonIgnore]
	public long Duration
	{
		get
		{
			if (Events.Count != 0)
			{
				List<MacroEvent> events = Events;
				return events[events.Count - 1].T;
			}
			return 0L;
		}
	}

	[JsonIgnore]
	public bool IsEmpty => Events.Count == 0;

	public long RecordedWidth => 1L;

	public void Save(string path)
	{
		File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOpts), Encoding.UTF8);
	}

	public static Macro Load(string path)
	{
		Macro? obj = JsonSerializer.Deserialize<Macro>(File.ReadAllText(path, Encoding.UTF8), JsonOpts) ?? throw new InvalidDataException("Arquivo de macro vazio ou inválido.");
		Macro macro = obj;
		if (macro.Events == null)
		{
			List<MacroEvent> list2 = (macro.Events = new List<MacroEvent>());
		}
		obj.Events.RemoveAll((MacroEvent e) => e == null);
		obj.Events.Sort((MacroEvent a, MacroEvent b) => a.T.CompareTo(b.T));
		return obj;
	}

	public static Macro ImportRec(string path)
	{
		byte[] array = File.ReadAllBytes(path);
		if (array.Length < 20 || array.Length % 20 != 0)
		{
			throw new InvalidDataException($"O arquivo não parece um .rec válido (tamanho {array.Length} bytes, precisa ser múltiplo de 20).");
		}
		Macro macro = new Macro
		{
			Created = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm:ss")
		};
		int num = array.Length / 20;
		uint num2 = 0u;
		long num3 = 0L;
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			int num5 = i * 20;
			uint num6 = BitConverter.ToUInt32(array, num5);
			uint num7 = BitConverter.ToUInt32(array, num5 + 4);
			uint y = BitConverter.ToUInt32(array, num5 + 8);
			uint num8 = BitConverter.ToUInt32(array, num5 + 12);
			uint num9 = BitConverter.ToUInt32(array, num5 + 16);
			num3 = ((i != 0) ? (num3 + (num8 - num2)) : 0);
			num2 = num8;
			MacroEvent macroEvent = num6 switch
			{
				256u => new MacroEvent
				{
					Kind = EvKind.KeyDown,
					Vk = (int)(num7 & 0xFF),
					Scan = (int)((num7 >> 8) & 0xFF)
				}, 
				257u => new MacroEvent
				{
					Kind = EvKind.KeyUp,
					Vk = (int)(num7 & 0xFF),
					Scan = (int)((num7 >> 8) & 0xFF)
				}, 
				512u => new MacroEvent
				{
					Kind = EvKind.MouseMove,
					X = (int)num7,
					Y = (int)y
				}, 
				513u => new MacroEvent
				{
					Kind = EvKind.MouseDown,
					Button = 0,
					X = (int)num7,
					Y = (int)y
				}, 
				514u => new MacroEvent
				{
					Kind = EvKind.MouseUp,
					Button = 0,
					X = (int)num7,
					Y = (int)y
				}, 
				516u => new MacroEvent
				{
					Kind = EvKind.MouseDown,
					Button = 1,
					X = (int)num7,
					Y = (int)y
				}, 
				517u => new MacroEvent
				{
					Kind = EvKind.MouseUp,
					Button = 1,
					X = (int)num7,
					Y = (int)y
				}, 
				_ => null, 
			};
			if (macroEvent == null)
			{
				if (num6 == 515)
				{
					macro.Events.Add(new MacroEvent
					{
						T = num3,
						Kind = EvKind.MouseDown,
						Button = 0,
						X = (int)num7,
						Y = (int)y,
						Hwnd = num9
					});
					macro.Events.Add(new MacroEvent
					{
						T = num3,
						Kind = EvKind.MouseUp,
						Button = 0,
						X = (int)num7,
						Y = (int)y,
						Hwnd = num9
					});
				}
				else
				{
					num4++;
				}
			}
			else
			{
				macroEvent.T = num3;
				macroEvent.Hwnd = num9;
				macro.Events.Add(macroEvent);
			}
		}
		if (num4 > 0)
		{
			AppLog.Write($"ImportRec: {num4} registro(s) sem equivalente foram ignorados.");
		}
		return macro;
	}

	public int ExportRec(string path)
	{
		using FileStream output = new FileStream(path, FileMode.Create, FileAccess.Write);
		using BinaryWriter binaryWriter = new BinaryWriter(output);
		int num = 0;
		foreach (MacroEvent @event in Events)
		{
			uint value;
			uint value2;
			uint value3;
			switch (@event.Kind)
			{
			case EvKind.KeyDown:
				value = 256u;
				value2 = (uint)((@event.Scan & 0xFF) << 8) | ((uint)@event.Vk & 0xFFu);
				value3 = (uint)@event.Scan & 0xFFu;
				break;
			case EvKind.KeyUp:
				value = 257u;
				value2 = (uint)((@event.Scan & 0xFF) << 8) | ((uint)@event.Vk & 0xFFu);
				value3 = (uint)@event.Scan & 0xFFu;
				break;
			case EvKind.MouseMove:
				value = 512u;
				value2 = (uint)@event.X;
				value3 = (uint)@event.Y;
				break;
			case EvKind.MouseDown:
				if (@event.Button == 0)
				{
					value = 513u;
					value2 = (uint)@event.X;
					value3 = (uint)@event.Y;
					break;
				}
				if (@event.Button == 1)
				{
					value = 516u;
					value2 = (uint)@event.X;
					value3 = (uint)@event.Y;
					break;
				}
				goto default;
			case EvKind.MouseUp:
				if (@event.Button == 0)
				{
					value = 514u;
					value2 = (uint)@event.X;
					value3 = (uint)@event.Y;
					break;
				}
				if (@event.Button == 1)
				{
					value = 517u;
					value2 = (uint)@event.X;
					value3 = (uint)@event.Y;
					break;
				}
				goto default;
			default:
				num++;
				continue;
			}
			binaryWriter.Write(value);
			binaryWriter.Write(value2);
			binaryWriter.Write(value3);
			binaryWriter.Write((uint)(1000 + @event.T));
			binaryWriter.Write((uint)@event.Hwnd);
		}
		if (num > 0)
		{
			AppLog.Write($"ExportRec: {num} evento(s) sem equivalente .rec foram ignorados.");
		}
		return num;
	}
}
