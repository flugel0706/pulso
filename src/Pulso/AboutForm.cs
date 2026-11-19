using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class AboutForm : Form
{
	public AboutForm()
	{
		Text = "Sobre o Pulso";
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		base.ClientSize = new Size(520, 330);
		base.BackColor = Palette.Bg;
		base.ForeColor = Palette.Text;
		base.KeyPreview = true;
		DoubleBuffered = true;
		HudChrome.Attach(this, "Sobre o Pulso");
		BackdropPanel panel = new BackdropPanel
		{
			Dock = DockStyle.Fill,
			Dim = 0.52f
		};
		BrandMark mark = new BrandMark
		{
			Location = new Point(28, 66),
			Size = new Size(44, 44)
		};
		Label title = new Label
		{
			Text = "PULSO",
			AutoSize = true,
			Location = new Point(84, 58),
			Font = UiFont.Hero,
			ForeColor = Palette.Text,
			BackColor = Color.Transparent
		};
		Label sub = new Label
		{
			Text = "v" + Program.Version + "  //  ESTÚDIO TÁTICO DE AUTOMAÇÃO",
			AutoSize = true,
			Location = new Point(86, 96),
			Font = UiFont.TinyBold,
			ForeColor = Palette.Accent,
			BackColor = Color.Transparent
		};
		Panel rule = new Panel
		{
			Location = new Point(28, 134),
			Size = new Size(464, 1),
			BackColor = Palette.Alpha(Palette.Accent, 110)
		};
		string[] lines = new string[5]
		{
			"Gravação global de mouse e teclado",
			"Linha do tempo editável com desfazer/refazer",
			"Reprodução com velocidade, repetições e pausas",
			"Exportação .pulso e compilação em .exe",
			"F12 interrompe tudo, sempre"
		};
		for (int i = 0; i < lines.Length; i++)
		{
			Label item = new Label
			{
				Text = "//  " + lines[i],
				AutoSize = true,
				Location = new Point(28, 148 + i * 26),
				Font = UiFont.Body,
				ForeColor = Palette.TextSoft,
				BackColor = Color.Transparent
			};
			panel.Controls.Add(item);
		}
		Label credit = new Label
		{
			Text = "MIT © 2026 Pulso Studio  •  fundo: USMC (domínio público)",
			AutoSize = true,
			Location = new Point(28, 324),
			Font = UiFont.Tiny,
			ForeColor = Palette.Muted,
			BackColor = Color.Transparent
		};
		HudButton close = new HudButton
		{
			Text = "Fechar",
			Kind = HudKind.Primary,
			Accent = Palette.Accent,
			Width = 124,
			Height = 36,
			Radius = 4,
			Location = new Point(368, 310)
		};
		close.Click += delegate
		{
			Close();
		};
		panel.Controls.Add(mark);
		panel.Controls.Add(title);
		panel.Controls.Add(sub);
		panel.Controls.Add(rule);
		panel.Controls.Add(credit);
		panel.Controls.Add(close);
		base.Controls.Add(panel);
		base.KeyDown += delegate(object? _, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter)
			{
				Close();
			}
		};
	}
}
