using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class SettingsForm : Form
{
	private sealed class CardBody : Panel
	{
		private readonly float _k;

		public int Y { get; private set; }

		public CardBody(float scale)
		{
			_k = scale;
			BackColor = Palette.Panel;
			DoubleBuffered = true;
		}

		private int S(int v)
		{
			return Math.Max(1, (int)Math.Round((float)v * _k));
		}

		public void Row(string label, Control control, string? hint = null)
		{
			base.Controls.Add(Caption(label, Y));
			control.BackColor = Palette.PanelAlt;
			control.Location = new Point(S(270), Y + S(1));
			if (control is TextBox)
			{
				control.Width = Math.Max(S(80), base.Width - S(274));
			}
			control.Height = Math.Max(control.Height, S(26));
			base.Controls.Add(control);
			Y += S(36);
			if (hint != null)
			{
				Note(hint, S(270), base.Width - S(274), 18);
			}
		}

		public void Row2(string label, Control a, Control b)
		{
			base.Controls.Add(Caption(label, Y));
			a.BackColor = Palette.PanelAlt;
			a.Location = new Point(S(270), Y + S(3));
			a.Height = Math.Max(a.Height, S(26));
			base.Controls.Add(a);
			b.Location = new Point(S(270) + a.Width + S(12), Y + S(6));
			base.Controls.Add(b);
			Y += S(36);
		}

		public void Check(Control check)
		{
			check.BackColor = Palette.Panel;
			check.Location = new Point(S(270), Y + S(4));
			base.Controls.Add(check);
			Y += S(34);
		}

		public void Note(string text, int x, int width, int height)
		{
			Label value = new Label
			{
				Text = text,
				AutoSize = false,
				BackColor = Palette.Panel,
				ForeColor = Palette.Muted,
				Font = UiFont.Tiny,
				Location = new Point(x, Y),
				Size = new Size(Math.Max(S(60), width), S(height))
			};
			base.Controls.Add(value);
			Y += S(height) + S(4);
		}

		private Label Caption(string text, int y)
		{
			return new Label
			{
				Text = text,
				AutoSize = false,
				BackColor = Palette.Panel,
				ForeColor = Palette.TextSoft,
				TextAlign = ContentAlignment.MiddleLeft,
				Location = new Point(0, y),
				Size = new Size(S(262), S(30))
			};
		}
	}

	private readonly Settings _s;

	private readonly float _k;

	private readonly TextBox _hkRecord = new TextBox();

	private readonly TextBox _hkPlay = new TextBox();

	private readonly TextBox _hkPause = new TextBox();

	private readonly TextBox _hkStop = new TextBox();

	private readonly TextBox _hkCompact = new TextBox();

	private readonly NumericUpDown _speed = new NumericUpDown();

	private readonly NumericUpDown _loops = new NumericUpDown();

	private readonly NumericUpDown _loopDelay = new NumericUpDown();

	private readonly NumericUpDown _startDelay = new NumericUpDown();

	private readonly PulseCheck _instant = new PulseCheck();

	private readonly PulseCheck _continuous = new PulseCheck();

	private readonly PulseCheck _relative = new PulseCheck();

	private readonly PulseCheck _postToWindow = new PulseCheck();

	private readonly PulseCheck _scanCodes = new PulseCheck();

	private readonly PulseCheck _recMouse = new PulseCheck();

	private readonly PulseCheck _recKeyboard = new PulseCheck();

	private readonly PulseCheck _ignoreOwn = new PulseCheck();

	private readonly PulseCheck _topMost = new PulseCheck();

	private readonly PulseCheck _tray = new PulseCheck();

	private readonly PulseCheck _minBar = new PulseCheck();

	private readonly PulseCheck _confirmClear = new PulseCheck();

	public Settings Result { get; private set; }

	private int Sc(int v)
	{
		return Math.Max(1, (int)Math.Round((float)v * _k));
	}

	public SettingsForm(Settings current, float uiScale = 1f)
	{
		_s = current.Clone();
		Result = _s;
		_k = ((uiScale <= 0f) ? 1f : uiScale);
		Text = "Configurações — Pulso";
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		int val = Screen.FromPoint(System.Windows.Forms.Cursor.Position).WorkingArea.Height - Sc(70);
		base.ClientSize = new Size(Sc(704), Math.Min(Sc(780), Math.Max(Sc(560), val)));
		Font = new Font("Segoe UI", 9f);
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true,
			Padding = new Padding(Sc(18), Sc(14), Sc(18), Sc(14)),
			BackColor = Palette.Bg
		};
		DarkUi.DarkScroll(flowLayoutPanel);
		flowLayoutPanel.Controls.Add(BuildHotkeys());
		flowLayoutPanel.Controls.Add(BuildPlayback());
		flowLayoutPanel.Controls.Add(BuildRecording());
		flowLayoutPanel.Controls.Add(BuildGeneral());
		Panel buttons = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Sc(58),
			BackColor = Palette.Bg,
			Padding = new Padding(Sc(18), Sc(12), Sc(18), Sc(12))
		};
		buttons.Paint += delegate(object? _, PaintEventArgs e)
		{
			using Pen pen = new Pen(Palette.BorderSoft);
			e.Graphics.DrawLine(pen, 0, 0, buttons.Width, 0);
		};
		Button cancel = new Button
		{
			Text = "Cancelar",
			Width = Sc(104),
			Height = Sc(32),
			DialogResult = DialogResult.Cancel
		};
		Button ok = new Button
		{
			Text = "Salvar alterações",
			Width = Sc(150),
			Height = Sc(32)
		};
		ok.Click += delegate
		{
			if (ValidateAll())
			{
				base.DialogResult = DialogResult.OK;
			}
		};
		cancel.Location = new Point(buttons.Width - Sc(18) - cancel.Width, Sc(12));
		ok.Location = new Point(cancel.Left - Sc(8) - ok.Width, Sc(12));
		buttons.Controls.Add(ok);
		buttons.Controls.Add(cancel);
		buttons.Resize += delegate
		{
			cancel.Left = buttons.Width - Sc(18) - cancel.Width;
			ok.Left = cancel.Left - Sc(8) - ok.Width;
		};
		base.Controls.Add(flowLayoutPanel);
		base.Controls.Add(buttons);
		_speed.Value = Math.Clamp(_s.SpeedPercent, 5, 2000);
		_loops.Value = Math.Clamp(_s.Loops, 1, 100000);
		_loopDelay.Value = Math.Clamp(_s.LoopDelayMs, 0, 600000);
		_startDelay.Value = Math.Clamp(_s.StartDelayMs, 0, 60000);
		_instant.Checked = _s.Instant;
		_continuous.Checked = _s.Continuous;
		_relative.Checked = _s.RelativeCoords;
		_postToWindow.Checked = _s.PlayToRecordedWindow;
		_scanCodes.Checked = _s.UseScanCodes;
		_recMouse.Checked = _s.RecordMouse;
		_recKeyboard.Checked = _s.RecordKeyboard;
		_ignoreOwn.Checked = _s.IgnoreOwnWindow;
		_topMost.Checked = _s.TopMost;
		_tray.Checked = _s.MinimizeToTray;
		_minBar.Checked = _s.MinimizeToBar;
		_confirmClear.Checked = _s.ConfirmClear;
		ApplyHotkey(_hkRecord, _s.HotkeyRecord);
		ApplyHotkey(_hkPlay, _s.HotkeyPlay);
		ApplyHotkey(_hkPause, _s.HotkeyPause);
		ApplyHotkey(_hkStop, _s.HotkeyStop);
		ApplyHotkey(_hkCompact, _s.HotkeyCompact);
		DarkUi.Apply(this);
		BackColor = Palette.Bg;
		MakeOpaque(this);
		HudChrome.Attach(this, "Configurações");
		base.Shown += delegate
		{
			Invalidate(invalidateChildren: true);
			Update();
		};
	}

	private void ApplyHotkey(TextBox box, string combo)
	{
		box.Text = combo;
		box.Tag = Hotkey.Parse(combo);
	}

	private static void MakeOpaque(Control root)
	{
		foreach (Control control in root.Controls)
		{
			if ((control is Label || control is CheckBox) ? true : false)
			{
				control.BackColor = Palette.Panel;
			}
			MakeOpaque(control);
		}
	}

	private CardPanel Card(string title, out CardBody body)
	{
		CardPanel obj = new CardPanel
		{
			Width = Sc(636),
			Height = Sc(120),
			Radius = 14,
			Fill = Palette.Panel,
			BorderColor = Palette.BorderSoft,
			Padding = new Padding(Sc(18), Sc(14), Sc(18), Sc(14)),
			Margin = new Padding(0, 0, 0, Sc(12))
		};
		Label value = new Label
		{
			Text = title,
			Dock = DockStyle.Top,
			Height = Sc(26),
			Font = UiFont.Head,
			ForeColor = Palette.Text,
			BackColor = Palette.Panel,
			TextAlign = ContentAlignment.MiddleLeft
		};
		body = new CardBody(_k)
		{
			Dock = DockStyle.Fill,
			Width = Sc(600)
		};
		obj.Controls.Add(body);
		obj.Controls.Add(value);
		return obj;
	}

	private void Finish(CardPanel card, CardBody body)
	{
		card.MinimumSize = new Size(height: card.Height = Sc(14) * 2 + Sc(26) + body.Y + Sc(4), width: Sc(636));
	}

	private CardPanel BuildHotkeys()
	{
		CardBody body;
		CardPanel cardPanel = Card("Atalhos globais", out body);
		AddHotkeyRow(body, "Gravar:", _hkRecord);
		AddHotkeyRow(body, "Reproduzir:", _hkPlay);
		AddHotkeyRow(body, "Pausar / continuar:", _hkPause);
		AddHotkeyRow(body, "Parar tudo:", _hkStop);
		AddHotkeyRow(body, "Modo compacto:", _hkCompact);
		body.Note("Funcionam mesmo com o Pulso em segundo plano. F12 é sempre emergência.", Sc(270), Sc(326), 32);
		Finish(cardPanel, body);
		return cardPanel;
	}

	private void AddHotkeyRow(CardBody body, string label, TextBox box)
	{
		TextBox box2 = box;
		string label2 = label;
		box2.ReadOnly = true;
		box2.Font = new Font("Cascadia Mono", 10f);
		box2.Height = Sc(28);
		box2.Width = Sc(210);
		box2.BackColor = Palette.PanelAlt;
		box2.ForeColor = Palette.AccentSoft;
		box2.BorderStyle = BorderStyle.FixedSingle;
		HudButton hudButton = new HudButton
		{
			Text = "Alterar",
			Kind = HudKind.Ghost,
			Width = Sc(84),
			Height = Sc(28),
			Radius = 7,
			Font = UiFont.Body
		};
		hudButton.Click += delegate
		{
			using HotkeyDialog hotkeyDialog = new HotkeyDialog(((Hotkey)box2.Tag) ?? Hotkey.None, "Atalho — " + label2.TrimEnd(':'));
			hotkeyDialog.TopMost = base.TopMost;
			if (hotkeyDialog.ShowDialog(this) == DialogResult.OK)
			{
				if (hotkeyDialog.Result.IsValid && hotkeyDialog.Result.Vk == 123)
				{
					MessageBox.Show(this, "F12 é reservado para o botão de emergência. Escolha outra tecla.", "Atalho inválido", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else
				{
					box2.Tag = hotkeyDialog.Result;
					box2.Text = hotkeyDialog.Result.ToString();
				}
			}
		};
		body.Row2(label2, box2, hudButton);
	}

	private CardPanel BuildPlayback()
	{
		CardBody body;
		CardPanel cardPanel = Card("Reprodução", out body);
		_speed.Minimum = 5m;
		_speed.Maximum = 2000m;
		_speed.Increment = 5m;
		_speed.Width = Sc(140);
		body.Row("Velocidade (%):", _speed, "100 = tempo real · 200 = 2× mais rápido · 50 = metade");
		_instant.Text = "Sem intervalos (o mais rápido possível)";
		body.Check(_instant);
		_loops.Minimum = 1m;
		_loops.Maximum = 100000m;
		_loops.Width = Sc(110);
		_continuous.Text = "Repetir continuamente";
		body.Row2("Repetições:", _loops, _continuous);
		_loopDelay.Minimum = 0m;
		_loopDelay.Maximum = 600000m;
		_loopDelay.Increment = 250m;
		_loopDelay.Width = Sc(140);
		body.Row("Espera entre passagens (ms):", _loopDelay);
		_startDelay.Minimum = 0m;
		_startDelay.Maximum = 60000m;
		_startDelay.Increment = 500m;
		_startDelay.Width = Sc(140);
		body.Row("Espera antes de começar (ms):", _startDelay);
		_relative.Text = "Coordenadas relativas à janela gravada";
		body.Check(_relative);
		_postToWindow.Text = "Enviar teclas direto para a janela gravada (PostMessage)";
		body.Check(_postToWindow);
		_scanCodes.Text = "Injetar por scan code (recomendado para jogos)";
		body.Check(_scanCodes);
		Finish(cardPanel, body);
		return cardPanel;
	}

	private CardPanel BuildRecording()
	{
		CardBody body;
		CardPanel cardPanel = Card("Gravação", out body);
		_recMouse.Text = "Gravar mouse (movimento, cliques, roda, botões X)";
		body.Check(_recMouse);
		_recKeyboard.Text = "Gravar teclado";
		body.Check(_recKeyboard);
		_ignoreOwn.Text = "Ignorar cliques na janela do Pulso";
		body.Check(_ignoreOwn);
		Finish(cardPanel, body);
		return cardPanel;
	}

	private CardPanel BuildGeneral()
	{
		CardBody body;
		CardPanel cardPanel = Card("Geral", out body);
		_topMost.Text = "Manter a janela sempre no topo";
		body.Check(_topMost);
		_tray.Text = "Minimizar para a bandeja do sistema";
		body.Check(_tray);
		_minBar.Text = "Ao minimizar, virar a barra compacta";
		body.Check(_minBar);
		_confirmClear.Text = "Pedir confirmação antes de limpar a macro";
		body.Check(_confirmClear);
		Finish(cardPanel, body);
		return cardPanel;
	}

	private bool ValidateAll()
	{
		(string, Hotkey)[] array = new(string, Hotkey)[5]
		{
			("Gravar", (Hotkey)_hkRecord.Tag),
			("Reproduzir", (Hotkey)_hkPlay.Tag),
			("Pausar", (Hotkey)_hkPause.Tag),
			("Parar", (Hotkey)_hkStop.Tag),
			("Modo compacto", (Hotkey)_hkCompact.Tag)
		};
		(string, Hotkey)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			(string, Hotkey) tuple = array2[i];
			var (text, _) = tuple;
			if (!tuple.Item2.IsValid)
			{
				MessageBox.Show(this, "O atalho de \"" + text + "\" não está definido.", "Atalho obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			for (int k = j + 1; k < array.Length; k++)
			{
				if (array[j].Item2.SameAs(array[k].Item2))
				{
					MessageBox.Show(this, $"Os atalhos de \"{array[j].Item1}\" e \"{array[k].Item1}\" são iguais ({array[j].Item2}).\r\n" + "Escolha combinações diferentes.", "Atalhos em conflito", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
		}
		_s.HotkeyRecord = array[0].Item2.ToString();
		_s.HotkeyPlay = array[1].Item2.ToString();
		_s.HotkeyPause = array[2].Item2.ToString();
		_s.HotkeyStop = array[3].Item2.ToString();
		_s.HotkeyCompact = array[4].Item2.ToString();
		_s.SpeedPercent = (int)_speed.Value;
		_s.Loops = (int)_loops.Value;
		_s.LoopDelayMs = (int)_loopDelay.Value;
		_s.StartDelayMs = (int)_startDelay.Value;
		_s.Instant = _instant.Checked;
		_s.Continuous = _continuous.Checked;
		_s.RelativeCoords = _relative.Checked;
		_s.PlayToRecordedWindow = _postToWindow.Checked;
		_s.UseScanCodes = _scanCodes.Checked;
		_s.RecordMouse = _recMouse.Checked;
		_s.RecordKeyboard = _recKeyboard.Checked;
		_s.IgnoreOwnWindow = _ignoreOwn.Checked;
		_s.TopMost = _topMost.Checked;
		_s.MinimizeToTray = _tray.Checked;
		_s.MinimizeToBar = _minBar.Checked;
		_s.ConfirmClear = _confirmClear.Checked;
		Result = _s;
		return true;
	}
}
