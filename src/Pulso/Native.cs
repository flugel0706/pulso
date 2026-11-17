using System.Runtime.InteropServices;

namespace Pulso;

internal static class Native
{
	public struct POINT
	{
		public int x;

		public int y;
	}

	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	public struct KBDLLHOOKSTRUCT
	{
		public uint vkCode;

		public uint scanCode;

		public uint flags;

		public uint time;

		public nint dwExtraInfo;
	}

	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;

		public uint mouseData;

		public uint flags;

		public uint time;

		public nint dwExtraInfo;
	}

	public delegate nint HookProc(int nCode, nint wParam, nint lParam);

	public struct INPUT
	{
		public uint type;

		public InputUnion U;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct InputUnion
	{
		[FieldOffset(0)]
		public MOUSEINPUT mi;

		[FieldOffset(0)]
		public KEYBDINPUT ki;

		[FieldOffset(0)]
		public HARDWAREINPUT hi;
	}

	public struct MOUSEINPUT
	{
		public int dx;

		public int dy;

		public uint mouseData;

		public uint dwFlags;

		public uint time;

		public nint dwExtraInfo;
	}

	public struct KEYBDINPUT
	{
		public ushort wVk;

		public ushort wScan;

		public uint dwFlags;

		public uint time;

		public nint dwExtraInfo;
	}

	public struct HARDWAREINPUT
	{
		public uint uMsg;

		public ushort wParamL;

		public ushort wParamH;
	}

	public const int WH_KEYBOARD_LL = 13;

	public const int WH_MOUSE_LL = 14;

	public const int WM_KEYDOWN = 256;

	public const int WM_KEYUP = 257;

	public const int WM_SYSKEYDOWN = 260;

	public const int WM_SYSKEYUP = 261;

	public const int WM_MOUSEMOVE = 512;

	public const int WM_LBUTTONDOWN = 513;

	public const int WM_LBUTTONUP = 514;

	public const int WM_RBUTTONDOWN = 516;

	public const int WM_RBUTTONUP = 517;

	public const int WM_MBUTTONDOWN = 519;

	public const int WM_MBUTTONUP = 520;

	public const int WM_MOUSEWHEEL = 522;

	public const int WM_XBUTTONDOWN = 523;

	public const int WM_XBUTTONUP = 524;

	public const uint LLKHF_INJECTED = 16u;

	public const uint LLMHF_INJECTED = 1u;

	public const int SM_CXSCREEN = 0;

	public const int SM_CYSCREEN = 1;

	public const int VK_SHIFT = 16;

	public const int VK_CONTROL = 17;

	public const int VK_MENU = 18;

	public const int VK_LWIN = 91;

	public const int VK_RWIN = 92;

	public const int VK_LSHIFT = 160;

	public const int VK_RSHIFT = 161;

	public const int VK_LCONTROL = 162;

	public const int VK_RCONTROL = 163;

	public const int VK_LMENU = 164;

	public const int VK_RMENU = 165;

	public const uint INPUT_MOUSE = 0u;

	public const uint INPUT_KEYBOARD = 1u;

	public const uint KEYEVENTF_EXTENDEDKEY = 1u;

	public const uint KEYEVENTF_KEYUP = 2u;

	public const uint KEYEVENTF_SCANCODE = 8u;

	public const uint MOUSEEVENTF_MOVE = 1u;

	public const uint MOUSEEVENTF_LEFTDOWN = 2u;

	public const uint MOUSEEVENTF_LEFTUP = 4u;

	public const uint MOUSEEVENTF_RIGHTDOWN = 8u;

	public const uint MOUSEEVENTF_RIGHTUP = 16u;

	public const uint MOUSEEVENTF_MIDDLEDOWN = 32u;

	public const uint MOUSEEVENTF_MIDDLEUP = 64u;

	public const uint MOUSEEVENTF_XDOWN = 128u;

	public const uint MOUSEEVENTF_XUP = 256u;

	public const uint MOUSEEVENTF_WHEEL = 2048u;

	public const uint MOUSEEVENTF_ABSOLUTE = 32768u;

	public const int WM_NCLBUTTONDOWN = 161;

	public const int HTCAPTION = 2;

	public const uint SWP_NOSIZE = 1u;

	public const uint SWP_NOMOVE = 2u;

	public const uint SWP_NOZORDER = 4u;

	public const uint SWP_NOACTIVATE = 16u;

	public const uint SWP_FRAMECHANGED = 32u;

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool UnhookWindowsHookEx(nint hhk);

	[DllImport("user32.dll")]
	public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern nint GetModuleHandle(string? lpModuleName);

	[DllImport("user32.dll")]
	public static extern short GetAsyncKeyState(int vKey);

	[DllImport("user32.dll")]
	public static extern nint GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll")]
	public static extern bool IsWindow(nint hWnd);

	[DllImport("user32.dll")]
	public static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	public static extern int GetSystemMetrics(int nIndex);

	[DllImport("user32.dll")]
	public static extern uint MapVirtualKey(uint uCode, uint uMapType);

	[DllImport("user32.dll")]
	public static extern bool PostMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AttachConsole(int dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AllocConsole();

	[DllImport("dwmapi.dll")]
	public static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	public static extern int SetWindowTheme(nint hWnd, string? subAppName, string? subIdList);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern nint SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);
}
