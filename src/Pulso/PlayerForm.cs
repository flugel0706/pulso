using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class PlayerForm : Form
{
	private readonly Settings _s;

	private readonly Macro _macro;

	private readonly Player _player = new Player();

	private readonly InputEngine _engine = new InputEngine();

	private readonly NotifyIcon _tray = new NotifyIcon();

	private readonly HashSet<int> _down = new HashSet<int>();

	private bool _stopped;

	public PlayerForm(Macro macro, Settings settings)
	{
		_s = settings;
		_macro = macro;
		Text = "Pulso — reprodução";
		base.ShowInTaskbar = false;
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.Manual;
		base.Location = new Point(-32000, -32000);
		base.Size = new Size(1, 1);
		base.Opacity = 0.0;
		_tray.Icon = AppIcon.Small;
		_tray.Text = "Pulso — reproduzindo macro";
		_tray.Visible = true;
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip
		{
			Items = 
			{
				{
					"Pausar / continuar",
					(Image?)null,
					(EventHandler?)delegate
					{
						TogglePause();
					}
				},
				{
					"Reiniciar",
					(Image?)null,
					(EventHandler?)delegate
					{
						Restart();
					}
				},
				{
					"Parar e sair",
					(Image?)null,
					(EventHandler?)delegate
					{
						StopAndExit();
					}
				}
			}
		};
		_tray.ContextMenuStrip = contextMenuStrip;
		_tray.DoubleClick += delegate
		{
			TogglePause();
		};
		_player.Status += delegate(string st)
		{
			string st2 = st;
			Ui(delegate
			{
				_tray.Text = "Pulso — " + st2;
			});
		};
		_player.Finished += delegate
		{
			Ui(OnFinished);
		};
		base.Load += delegate
		{
			if (!_engine.Install())
			{
				AppLog.Write("PlayerForm: falha ao instalar hooks.");
			}
			_engine.OwnProcessId = Environment.ProcessId;
			_engine.OwnWindow = base.Handle;
			_engine.KeyActivity += OnKeyActivity;
			StartPlayback();
		};
	}

	private void Ui(Action a)
	{
		try
		{
			if (!base.IsDisposed)
			{
				if (base.InvokeRequired)
				{
					BeginInvoke(a);
				}
				else
				{
					a();
				}
			}
		}
		catch
		{
		}
	}

	private void StartPlayback()
	{
		if (_macro.IsEmpty)
		{
			_tray.ShowBalloonTip(2500, "Pulso", "A macro compilada está vazia.", ToolTipIcon.Warning);
			StopAndExit();
			return;
		}
		if (_s.ShowTrayBubble)
		{
			_tray.ShowBalloonTip(1500, "Pulso", "Macro em execução. F12 encerra.", ToolTipIcon.Info);
		}
		_player.Start(_macro, _s);
	}

	private void Restart()
	{
		_player.Stop();
		_stopped = false;
		StartPlayback();
	}

	private void TogglePause()
	{
		if (_player.IsRunning)
		{
			if (_player.IsPaused)
			{
				_player.Resume();
			}
			else
			{
				_player.Pause();
			}
		}
	}

	private void OnFinished()
	{
		if (!_stopped && !_s.Continuous)
		{
			_tray.ShowBalloonTip(1200, "Pulso", "Macro concluída.", ToolTipIcon.Info);
			Timer t = new Timer
			{
				Interval = 900
			};
			t.Tick += delegate
			{
				t.Stop();
				t.Dispose();
				Close();
			};
			t.Start();
		}
	}

	private void StopAndExit()
	{
		_stopped = true;
		_player.Stop();
		Close();
	}

	private void OnKeyActivity(int vk, bool down)
	{
		if (!down)
		{
			_down.Remove(vk);
		}
		else if (_down.Add(vk))
		{
			if (vk == 123)
			{
				Post(StopAndExit);
			}
			else if (Matches(_s.HotkeyStop, vk))
			{
				Post(StopAndExit);
			}
			else if (Matches(_s.HotkeyPause, vk))
			{
				Post(TogglePause);
			}
		}
	}

	private void Post(Action action)
	{
		try
		{
			if (!base.IsDisposed)
			{
				BeginInvoke(action);
			}
		}
		catch
		{
		}
	}

	private bool Matches(string combo, int vk)
	{
		Hotkey hotkey = Hotkey.Parse(combo);
		if (!hotkey.IsValid || hotkey.Vk != vk)
		{
			return false;
		}
		if (hotkey.Ctrl != DownVk(17))
		{
			return false;
		}
		if (hotkey.Alt != DownVk(18))
		{
			return false;
		}
		if (hotkey.Shift != DownVk(16))
		{
			return false;
		}
		bool flag = DownVk(91) || DownVk(92);
		return hotkey.Win == flag;
	}

	private static bool DownVk(int vk)
	{
		return (Native.GetAsyncKeyState(vk) & 0x8000) != 0;
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		_player.Stop();
		_engine.Dispose();
		_tray.Visible = false;
		_tray.Dispose();
		base.OnFormClosing(e);
	}
}
