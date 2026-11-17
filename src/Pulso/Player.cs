using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pulso;

public sealed class Player
{
	private Thread? _thread;

	private volatile bool _stop;

	private volatile bool _paused;

	private readonly object _sync = new object();

	public bool IsRunning
	{
		get
		{
			lock (_sync)
			{
				return _thread?.IsAlive ?? false;
			}
		}
	}

	public bool IsPaused => _paused;

	public event Action<string>? Status;

	public event Action<int, int>? LoopProgress;

	public event Action? Finished;

	private void Raise(string s)
	{
		try
		{
			this.Status?.Invoke(s);
		}
		catch
		{
		}
	}

	public void Start(Macro macro, Settings s)
	{
		Macro macro2 = macro;
		Settings s2 = s;
		lock (_sync)
		{
			Thread thread = _thread;
			if (thread == null || !thread.IsAlive)
			{
				_stop = false;
				_paused = false;
				_thread = new Thread((ThreadStart)delegate
				{
					Run(macro2, s2);
				})
				{
					IsBackground = true,
					Name = "Pulso.Player"
				};
				_thread.Start();
			}
		}
	}

	public void Pause()
	{
		_paused = true;
		Raise("Pausado");
	}

	public void Resume()
	{
		_paused = false;
		Raise("Reproduzindo");
	}

	public void Stop()
	{
		_stop = true;
		_paused = false;
		this.Status?.Invoke("Parado");
	}

	private void Run(Macro macro, Settings s)
	{
		try
		{
			int systemMetrics = Native.GetSystemMetrics(0);
			int systemMetrics2 = Native.GetSystemMetrics(1);
			double num = ((s.Instant || s.SpeedPercent <= 0) ? 0.0 : (100.0 / (double)s.SpeedPercent));
			int dx = 0;
			int dy = 0;
			if (s.RelativeCoords && macro.Hwnd != 0)
			{
				nint hWnd = new IntPtr(macro.Hwnd);
				if (Native.IsWindow(hWnd) && Native.GetWindowRect(hWnd, out var lpRect))
				{
					dx = lpRect.left - macro.WinLeft;
					dy = lpRect.top - macro.WinTop;
				}
				else
				{
					AppLog.Write("Coordenadas relativas: janela gravada não existe mais; usando posição absoluta.");
				}
			}
			if (s.StartDelayMs > 0)
			{
				Countdown(s.StartDelayMs, s.StartDelayMs);
			}
			int num2 = 0;
			while (!_stop)
			{
				num2++;
				this.LoopProgress?.Invoke(num2, (!s.Continuous) ? s.Loops : 0);
				Raise($"Reproduzindo (passagem {num2}{(s.Continuous ? "" : ("/" + s.Loops))})");
				Stopwatch stopwatch = Stopwatch.StartNew();
				long num3 = 0L;
				long num4 = 0L;
				for (int i = 0; i < macro.Events.Count; i++)
				{
					if (_stop)
					{
						break;
					}
					MacroEvent macroEvent = macro.Events[i];
					long num5 = ((num == 0.0) ? 0 : ((long)((double)macroEvent.T * num)));
					while (!_stop)
					{
						if (_paused)
						{
							if (num4 == 0L)
							{
								num4 = stopwatch.ElapsedMilliseconds;
							}
							Thread.Sleep(5);
							continue;
						}
						if (num4 > 0)
						{
							num3 += stopwatch.ElapsedMilliseconds - num4;
							num4 = 0L;
						}
						long num6 = stopwatch.ElapsedMilliseconds - num3;
						long num7 = num5 - num6;
						if (num7 <= 0)
						{
							break;
						}
						Thread.Sleep((int)Math.Min(num7, 15L));
					}
					if (_stop)
					{
						break;
					}
					Inject(macroEvent, s, dx, dy, systemMetrics, systemMetrics2);
				}
				if (_stop || (!s.Continuous && num2 >= s.Loops))
				{
					break;
				}
				if (s.LoopDelayMs > 0)
				{
					Countdown(s.LoopDelayMs, s.LoopDelayMs);
				}
			}
			Raise(_stop ? "Parado" : "Concluído");
		}
		catch (Exception ex)
		{
			AppLog.Write("Player: " + ex);
			Raise("Erro: " + ex.Message);
		}
		finally
		{
			try
			{
				this.Finished?.Invoke();
			}
			catch
			{
			}
		}
	}

	private void Countdown(int ms, int total)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (stopwatch.ElapsedMilliseconds < ms && !_stop)
		{
			long value = (total - stopwatch.ElapsedMilliseconds + 999) / 1000;
			Raise($"Iniciando em {value}s...");
			Thread.Sleep(50);
		}
	}

	private static void Inject(MacroEvent ev, Settings s, int dx, int dy, int sw, int sh)
	{
		switch (ev.Kind)
		{
		case EvKind.KeyDown:
		case EvKind.KeyUp:
			if (s.PlayToRecordedWindow && ev.Hwnd != 0L)
			{
				nint hWnd = new IntPtr(ev.Hwnd);
				if (Native.IsWindow(hWnd))
				{
					int num4 = 1 | ((ev.Scan & 0xFF) << 16) | (ev.Extended ? 16777216 : 0);
					if (ev.Kind == EvKind.KeyUp)
					{
						num4 |= -1073741824;
					}
					Native.PostMessage(hWnd, (ev.Kind == EvKind.KeyDown) ? 256 : 257, new IntPtr(ev.Vk), new IntPtr(num4));
					break;
				}
			}
			SendKey(ev, s);
			break;
		case EvKind.MouseMove:
		{
			int num2 = Clamp(ev.X + dx, 0, Math.Max(0, sw - 1));
			int num3 = Clamp(ev.Y + dy, 0, Math.Max(0, sh - 1));
			SendMouse(32769u, 0u, num2 * 65535 / Math.Max(1, sw - 1), num3 * 65535 / Math.Max(1, sh - 1));
			break;
		}
		case EvKind.MouseDown:
		case EvKind.MouseUp:
		{
			bool num = ev.Kind == EvKind.MouseDown;
			int button = ev.Button;
			uint flags = (num ? (button switch
			{
				0 => 2u, 
				1 => 8u, 
				2 => 32u, 
				_ => 128u, 
			}) : (button switch
			{
				0 => 4u, 
				1 => 16u, 
				2 => 64u, 
				_ => 256u, 
			}));
			uint data = ((ev.Button >= 3) ? ((uint)(((ev.Button != 4) ? 1 : 2) << 16)) : 0u);
			SendMouse(flags, data, 0, 0);
			break;
		}
		case EvKind.Wheel:
			SendMouse(2048u, (uint)ev.Wheel, 0, 0);
			break;
		case EvKind.XDown:
		case EvKind.XUp:
			SendMouse((ev.Kind == EvKind.XDown) ? 128u : 256u, (uint)(((ev.Button != 4) ? 1 : 2) << 16), 0, 0);
			break;
		}
	}

	private static int Clamp(int v, int lo, int hi)
	{
		if (v >= lo)
		{
			if (v <= hi)
			{
				return v;
			}
			return hi;
		}
		return lo;
	}

	private static void SendKey(MacroEvent ev, Settings s)
	{
		Native.INPUT iNPUT = default(Native.INPUT);
		iNPUT.type = 1u;
		Native.INPUT iNPUT2 = iNPUT;
		uint num = ((ev.Kind == EvKind.KeyUp) ? 2u : 0u);
		if (ev.Extended)
		{
			num |= 1u;
		}
		if (s.UseScanCodes)
		{
			iNPUT2.U.ki = new Native.KEYBDINPUT
			{
				wVk = 0,
				wScan = (ushort)((uint)ev.Scan & 0xFFu),
				dwFlags = (num | 8u),
				dwExtraInfo = 1196709444
			};
		}
		else
		{
			iNPUT2.U.ki = new Native.KEYBDINPUT
			{
				wVk = (ushort)ev.Vk,
				wScan = (ushort)((uint)ev.Scan & 0xFFu),
				dwFlags = num,
				dwExtraInfo = 1196709444
			};
		}
		Native.SendInput(1u, new Native.INPUT[1] { iNPUT2 }, Marshal.SizeOf<Native.INPUT>());
	}

	private static void SendMouse(uint flags, uint data, int dx, int dy)
	{
		Native.INPUT iNPUT = default(Native.INPUT);
		iNPUT.type = 0u;
		Native.INPUT iNPUT2 = iNPUT;
		iNPUT2.U.mi = new Native.MOUSEINPUT
		{
			dx = dx,
			dy = dy,
			mouseData = data,
			dwFlags = flags,
			dwExtraInfo = 1196709444
		};
		Native.SendInput(1u, new Native.INPUT[1] { iNPUT2 }, Marshal.SizeOf<Native.INPUT>());
	}
}
