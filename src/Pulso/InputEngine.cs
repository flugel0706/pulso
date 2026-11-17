using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pulso;

public sealed class InputEngine : IDisposable
{
	private nint _kbdHook;

	private nint _mouseHook;

	private Native.HookProc? _kbdProc;

	private Native.HookProc? _mouseProc;

	private readonly object _sync = new object();

	private readonly Stopwatch _clock = Stopwatch.StartNew();

	private long _startMs;

	private int _suppressVk;

	private long _suppressMouseUntilMs;

	public Macro? Recording { get; private set; }

	public bool IsRecording => Recording != null;

	public bool RecordMouse { get; set; } = true;


	public bool RecordKeyboard { get; set; } = true;


	public bool IgnoreOwnWindow { get; set; } = true;


	public nint OwnWindow { get; set; }

	public int OwnProcessId { get; set; }

	public string? LastError { get; private set; }

	public int EventCount
	{
		get
		{
			lock (_sync)
			{
				return Recording?.Events.Count ?? 0;
			}
		}
	}

	public event Action<int, bool>? KeyActivity;

	public void SuppressKey(int vk)
	{
		lock (_sync)
		{
			_suppressVk = vk;
		}
	}

	public bool Install()
	{
		try
		{
			nint moduleHandle = Native.GetModuleHandle(null);
			_kbdProc = KbdProc;
			_mouseProc = MouseProc;
			_kbdHook = Native.SetWindowsHookEx(13, _kbdProc, moduleHandle, 0u);
			_mouseHook = Native.SetWindowsHookEx(14, _mouseProc, moduleHandle, 0u);
			if (_kbdHook == IntPtr.Zero || _mouseHook == IntPtr.Zero)
			{
				LastError = $"SetWindowsHookEx falhou (erro {Marshal.GetLastWin32Error()}). " + "Feche o app e tente novamente.";
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			LastError = ex.Message;
			return false;
		}
	}

	public void StartRecording(int suppressVk)
	{
		nint foregroundWindow = Native.GetForegroundWindow();
		Macro macro = new Macro
		{
			ScreenW = Native.GetSystemMetrics(0),
			ScreenH = Native.GetSystemMetrics(1),
			Hwnd = ((IntPtr)foregroundWindow).ToInt64(),
			Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
		};
		if (foregroundWindow != IntPtr.Zero && Native.GetWindowRect(foregroundWindow, out var lpRect))
		{
			macro.WinLeft = lpRect.left;
			macro.WinTop = lpRect.top;
		}
		lock (_sync)
		{
			Recording = macro;
			_startMs = _clock.ElapsedMilliseconds;
			_suppressVk = ((suppressVk != 0 && ((uint)Native.GetAsyncKeyState(suppressVk) & 0x8000u) != 0) ? suppressVk : 0);
			_suppressMouseUntilMs = _startMs + 120;
		}
	}

	public Macro? StopRecording()
	{
		lock (_sync)
		{
			Macro? recording = Recording;
			Recording = null;
			_suppressVk = 0;
			return recording;
		}
	}

	public MacroEvent? GetEvent(int i)
	{
		lock (_sync)
		{
			List<MacroEvent> list = Recording?.Events;
			return (list != null && i >= 0 && i < list.Count) ? list[i] : null;
		}
	}

	public List<MacroEvent> Snapshot()
	{
		lock (_sync)
		{
			return (Recording == null) ? new List<MacroEvent>() : new List<MacroEvent>(Recording.Events);
		}
	}

	private bool ShouldSkipWindow()
	{
		if (!IgnoreOwnWindow)
		{
			return true;
		}
		nint foregroundWindow = Native.GetForegroundWindow();
		if (foregroundWindow == IntPtr.Zero)
		{
			return true;
		}
		if (foregroundWindow == OwnWindow)
		{
			return false;
		}
		if (OwnProcessId != 0)
		{
			Native.GetWindowThreadProcessId(foregroundWindow, out var lpdwProcessId);
			if (lpdwProcessId == (uint)OwnProcessId)
			{
				return false;
			}
		}
		return true;
	}

	private void Add(MacroEvent e)
	{
		lock (_sync)
		{
			if (Recording != null)
			{
				e.T = _clock.ElapsedMilliseconds - _startMs;
				Recording.Events.Add(e);
			}
		}
	}

	private nint KbdProc(int nCode, nint wParam, nint lParam)
	{
		if (nCode >= 0)
		{
			try
			{
				Native.KBDLLHOOKSTRUCT kBDLLHOOKSTRUCT = Marshal.PtrToStructure<Native.KBDLLHOOKSTRUCT>(lParam);
				int num = (int)wParam;
				bool flag = ((num == 256 || num == 260) ? true : false);
				bool flag2 = flag;
				flag = ((num == 257 || num == 261) ? true : false);
				bool flag3 = flag;
				if ((flag2 || flag3) && (kBDLLHOOKSTRUCT.flags & 0x10) == 0)
				{
					this.KeyActivity?.Invoke((int)kBDLLHOOKSTRUCT.vkCode, flag2);
					if (IsRecording && RecordKeyboard)
					{
						int vkCode = (int)kBDLLHOOKSTRUCT.vkCode;
						lock (_sync)
						{
							if (_suppressVk == vkCode)
							{
								if (flag3)
								{
									_suppressVk = 0;
								}
								return Native.CallNextHookEx(_kbdHook, nCode, wParam, lParam);
							}
						}
						if (ShouldSkipWindow())
						{
							Add(new MacroEvent
							{
								Kind = (flag2 ? EvKind.KeyDown : EvKind.KeyUp),
								Vk = vkCode,
								Scan = (int)kBDLLHOOKSTRUCT.scanCode,
								Extended = IsExtendedVk(vkCode)
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				AppLog.Write("KbdProc: " + ex.Message);
			}
		}
		return Native.CallNextHookEx(_kbdHook, nCode, wParam, lParam);
	}

	private nint MouseProc(int nCode, nint wParam, nint lParam)
	{
		if (nCode >= 0)
		{
			try
			{
				Native.MSLLHOOKSTRUCT mSLLHOOKSTRUCT = Marshal.PtrToStructure<Native.MSLLHOOKSTRUCT>(lParam);
				if ((mSLLHOOKSTRUCT.flags & 1) == 0 && IsRecording && RecordMouse && ShouldSkipWindow() && _clock.ElapsedMilliseconds >= _suppressMouseUntilMs)
				{
					int num = (int)wParam;
					int x = mSLLHOOKSTRUCT.pt.x;
					int y = mSLLHOOKSTRUCT.pt.y;
					switch (num)
					{
					case 512:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseMove,
							X = x,
							Y = y
						});
						break;
					case 513:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseDown,
							Button = 0,
							X = x,
							Y = y
						});
						break;
					case 514:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseUp,
							Button = 0,
							X = x,
							Y = y
						});
						break;
					case 516:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseDown,
							Button = 1,
							X = x,
							Y = y
						});
						break;
					case 517:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseUp,
							Button = 1,
							X = x,
							Y = y
						});
						break;
					case 519:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseDown,
							Button = 2,
							X = x,
							Y = y
						});
						break;
					case 520:
						Add(new MacroEvent
						{
							Kind = EvKind.MouseUp,
							Button = 2,
							X = x,
							Y = y
						});
						break;
					case 523:
					case 524:
					{
						int num2 = (int)((mSLLHOOKSTRUCT.mouseData >> 16) & 0xFFFF);
						Add(new MacroEvent
						{
							Kind = ((num == 523) ? EvKind.XDown : EvKind.XUp),
							Button = ((num2 == 2) ? 4 : 3),
							X = x,
							Y = y
						});
						break;
					}
					case 522:
						Add(new MacroEvent
						{
							Kind = EvKind.Wheel,
							Wheel = (short)((mSLLHOOKSTRUCT.mouseData >> 16) & 0xFFFF),
							X = x,
							Y = y
						});
						break;
					case 515:
					case 518:
					case 521:
						break;
					}
				}
			}
			catch (Exception ex)
			{
				AppLog.Write("MouseProc: " + ex.Message);
			}
		}
		return Native.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
	}

	public static bool IsExtendedVk(int vk)
	{
		switch (vk)
		{
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 44:
		case 45:
		case 46:
		case 91:
		case 92:
		case 93:
		case 111:
		case 144:
		case 163:
		case 165:
			return true;
		default:
			return false;
		}
	}

	public void Dispose()
	{
		if (_kbdHook != IntPtr.Zero)
		{
			Native.UnhookWindowsHookEx(_kbdHook);
		}
		if (_mouseHook != IntPtr.Zero)
		{
			Native.UnhookWindowsHookEx(_mouseHook);
		}
		_kbdHook = (_mouseHook = IntPtr.Zero);
	}
}
