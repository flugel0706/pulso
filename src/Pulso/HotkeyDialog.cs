using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class HotkeyDialog : Form
{
	private readonly TextBox _display;

	private Hotkey _hotkey;

	private readonly float _k;

	public Hotkey Result => _hotkey;

	private int Sc(int v)
	{
		return Math.Max(1, (int)Math.Round((float)v * _k));
	}

	public HotkeyDialog(Hotkey current, string title)
	{
		Text = title;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.StartPosition = FormStartPosition.CenterParent;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		_k = Math.Max(1f, (float)base.DeviceDpi / 96f);
		base.ClientSize = new Size(Sc(400), Sc(190));
		Font = new Font("Segoe UI", 9f);
		_hotkey = current.Clone();
		Label label = new Label
		{
			Text = "Pressione a combinação desejada.\r\nEx.: Ctrl+Alt+R, Ctrl+Shift+F5, Alt+PrintScreen.",
			AutoSize = false,
			Dock = DockStyle.Top,
			Height = Sc(46),
			TextAlign = ContentAlignment.MiddleLeft
		};
		_display = new TextBox
		{
			ReadOnly = true,
			TextAlign = HorizontalAlignment.Center,
			Font = new Font("Segoe UI Semibold", 14f),
			Dock = DockStyle.Top,
			Height = Sc(40),
			Text = (_hotkey.IsValid ? _hotkey.ToString() : "(pressione uma tecla)")
		};
		Label label2 = new Label
		{
			Text = "Modificadores sozinhos não valem: a última tecla precisa ser uma tecla normal.\r\nEsc cancela a captura; Delete limpa o atalho.",
			Dock = DockStyle.Top,
			AutoSize = false,
			Height = Sc(40),
			ForeColor = Color.DimGray
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Bottom,
			FlowDirection = FlowDirection.RightToLeft,
			Height = Sc(44),
			Padding = new Padding(Sc(6))
		};
		Button button = new Button
		{
			Text = "OK",
			DialogResult = DialogResult.OK,
			Width = Sc(90),
			Height = Sc(30)
		};
		Button button2 = new Button
		{
			Text = "Cancelar",
			DialogResult = DialogResult.Cancel,
			Width = Sc(90),
			Height = Sc(30)
		};
		Button button3 = new Button
		{
			Text = "Limpar",
			Width = Sc(90),
			Height = Sc(30)
		};
		button3.Click += delegate
		{
			_hotkey = Hotkey.None;
			_display.Text = "(nenhum)";
		};
		flowLayoutPanel.Controls.Add(button);
		flowLayoutPanel.Controls.Add(button2);
		flowLayoutPanel.Controls.Add(button3);
		base.Controls.Add(flowLayoutPanel);
		base.Controls.Add(label2);
		base.Controls.Add(_display);
		base.Controls.Add(label);
		base.AcceptButton = button;
		base.CancelButton = button2;
		base.KeyPreview = true;
		DarkUi.Apply(this);
		HudChrome.Attach(this, title);
		_display.BackColor = Palette.PanelAlt;
		_display.ForeColor = Palette.Cyan;
		label2.ForeColor = Palette.Muted;
		label.ForeColor = Palette.Text;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		bool flag;
		switch (keys)
		{
		case Keys.Escape:
			base.DialogResult = DialogResult.Cancel;
			return true;
		case Keys.Delete:
			_hotkey = Hotkey.None;
			_display.Text = "(nenhum)";
			return true;
		case Keys.ShiftKey:
		case Keys.ControlKey:
		case Keys.Menu:
		case Keys.LWin:
		case Keys.RWin:
		case Keys.Shift:
		case Keys.Control:
		case Keys.Alt:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return true;
		}
		Hotkey hotkey = (_hotkey = new Hotkey
		{
			Ctrl = ((keyData & Keys.Control) != 0),
			Alt = ((keyData & Keys.Alt) != 0),
			Shift = ((keyData & Keys.Shift) != 0),
			Win = (NativeKeyDown(91) || NativeKeyDown(92)),
			Vk = (int)keys
		});
		_display.Text = hotkey.ToString();
		return true;
	}

	private static bool NativeKeyDown(int vk)
	{
		return (Native.GetAsyncKeyState(vk) & 0x8000) != 0;
	}
}
