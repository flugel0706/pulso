using System;

namespace Pulso;

public sealed class MacroPackage
{
	public string App { get; set; } = "Pulso";


	public int Version { get; set; } = 1;


	public string Name { get; set; } = "";


	public DateTime CreatedAt { get; set; } = DateTime.Now;


	public int Events { get; set; }

	public long DurationMs { get; set; }

	public Macro Macro { get; set; } = new Macro();


	public PlaybackPreset Play { get; set; } = new PlaybackPreset();

}
