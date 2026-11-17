using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public static class NumericPrompt
{
	public static int? Ask(IWin32Window owner, string title, string label, int min, int max, int value, int step = 1)
	{
		float i = ((owner is Control { DeviceDpi: >0 } control) ? ((float)control.DeviceDpi / 96f) : 1f);
		using (Form form = new Form
		{
			Text = title,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			StartPosition = FormStartPosition.CenterParent,
			MaximizeBox = false,
			MinimizeBox = false,
			ShowInTaskbar = false,
			ClientSize = new Size(Sc(460), Sc(160)),
			Font = new Font("Segoe UI", 9f)
		})
		{
			Label label2 = new Label
			{
				Text = label,
				Dock = DockStyle.Top,
				Height = Sc(56),
				TextAlign = ContentAlignment.MiddleLeft,
				Padding = new Padding(Sc(12), 0, Sc(12), 0)
			};
			NumericUpDown numericUpDown = new NumericUpDown
			{
				Minimum = min,
				Maximum = max,
				Value = Math.Clamp(value, min, max),
				Increment = step,
				Width = Sc(160),
				Left = Sc(12),
				Top = Sc(70)
			};
			Button button = new Button
			{
				Text = "OK",
				DialogResult = DialogResult.OK,
				Width = Sc(96),
				Height = Sc(30),
				Left = Sc(230),
				Top = Sc(112)
			};
			Button button2 = new Button
			{
				Text = "Cancelar",
				DialogResult = DialogResult.Cancel,
				Width = Sc(96),
				Height = Sc(30),
				Left = Sc(334),
				Top = Sc(112)
			};
			form.Controls.AddRange(new Control[4] { label2, numericUpDown, button, button2 });
			form.AcceptButton = button;
			form.CancelButton = button2;
			DarkUi.Apply(form);
			HudChrome.Attach(form, title, shiftAbsolute: true);
			return (form.ShowDialog(owner) == DialogResult.OK) ? new int?((int)numericUpDown.Value) : null;
		}
		int Sc(int v)
		{
			return Math.Max(1, (int)Math.Round((float)v * i));
		}
	}
}
