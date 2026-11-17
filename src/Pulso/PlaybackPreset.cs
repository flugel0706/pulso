using System;

namespace Pulso;

public sealed class PlaybackPreset
{
	public int SpeedPercent { get; set; } = 100;


	public bool Instant { get; set; }

	public int Loops { get; set; } = 1;


	public bool Continuous { get; set; }

	public int LoopDelayMs { get; set; }

	public int StartDelayMs { get; set; }

	public bool RelativeCoords { get; set; }

	public bool PlayToRecordedWindow { get; set; }

	public bool UseScanCodes { get; set; } = true;


	public static PlaybackPreset From(Settings s)
	{
		return new PlaybackPreset
		{
			SpeedPercent = s.SpeedPercent,
			Instant = s.Instant,
			Loops = s.Loops,
			Continuous = s.Continuous,
			LoopDelayMs = s.LoopDelayMs,
			StartDelayMs = s.StartDelayMs,
			RelativeCoords = s.RelativeCoords,
			PlayToRecordedWindow = s.PlayToRecordedWindow,
			UseScanCodes = s.UseScanCodes
		};
	}

	public void ApplyTo(Settings s)
	{
		s.SpeedPercent = Math.Clamp(SpeedPercent, 5, 2000);
		s.Instant = Instant;
		s.Loops = Math.Clamp(Loops, 1, 100000);
		s.Continuous = Continuous;
		s.LoopDelayMs = Math.Clamp(LoopDelayMs, 0, 3600000);
		s.StartDelayMs = Math.Clamp(StartDelayMs, 0, 60000);
		s.RelativeCoords = RelativeCoords;
		s.PlayToRecordedWindow = PlayToRecordedWindow;
		s.UseScanCodes = UseScanCodes;
	}
}
