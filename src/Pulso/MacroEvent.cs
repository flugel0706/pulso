using System.Text.Json.Serialization;

namespace Pulso;

public sealed class MacroEvent
{
	public long T { get; set; }

	public EvKind Kind { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public int Button { get; set; }

	public int Vk { get; set; }

	public int Scan { get; set; }

	public bool Extended { get; set; }

	public int Wheel { get; set; }

	public long Hwnd { get; set; }

	[JsonIgnore]
	public string KindName => Kind switch
	{
		EvKind.KeyDown => "Tecla ↓", 
		EvKind.KeyUp => "Tecla ↑", 
		EvKind.MouseMove => "Mouse →", 
		EvKind.MouseDown => "Clique ↓", 
		EvKind.MouseUp => "Clique ↑", 
		EvKind.Wheel => "Roda", 
		EvKind.XDown => "Botão X ↓", 
		EvKind.XUp => "Botão X ↑", 
		_ => "?", 
	};

	[JsonIgnore]
	public string ButtonName => Button switch
	{
		0 => "esquerdo", 
		1 => "direito", 
		2 => "meio", 
		3 => "X1", 
		4 => "X2", 
		_ => "?", 
	};

	[JsonIgnore]
	public string Detail
	{
		get
		{
			switch (Kind)
			{
			case EvKind.KeyDown:
			case EvKind.KeyUp:
				return $"{Hotkey.VkName(Vk)} (vk 0x{Vk:X2}, scan 0x{Scan:X2}){(Extended ? " ext" : "")}";
			case EvKind.MouseMove:
				return $"x={X}, y={Y}";
			case EvKind.MouseDown:
			case EvKind.MouseUp:
				return $"botão {ButtonName} em {X},{Y}";
			case EvKind.Wheel:
				return $"{((Wheel > 0) ? "+" : "")}{Wheel} em {X},{Y}";
			case EvKind.XDown:
			case EvKind.XUp:
				return $"botão {ButtonName} em {X},{Y}";
			default:
				return "";
			}
		}
	}

	public MacroEvent Clone()
	{
		return (MacroEvent)MemberwiseClone();
	}
}
