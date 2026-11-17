using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class EventEditDialog : Form
{
	private readonly float _k;

	public MacroEvent Result { get; }

	private int Sc(int v)
	{
		return Math.Max(1, (int)Math.Round((float)v * _k));
	}

	public EventEditDialog(MacroEvent src, int index)
	{
		EventEditDialog eventEditDialog = this;
		Result = src.Clone();
		Text = $"Editar evento #{index + 1} — {src.KindName}";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.StartPosition = FormStartPosition.CenterParent;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		_k = Math.Max(1f, (float)base.DeviceDpi / 96f);
		base.ClientSize = new Size(Sc(420), Sc(230));
		Font = new Font("Segoe UI", 9f);
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			Padding = new Padding(Sc(12)),
			RowCount = 5
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Sc(190)));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		NumericUpDown numT = new NumericUpDown
		{
			Minimum = 0m,
			Maximum = 86400000m,
			Value = Math.Clamp(src.T, 0L, 86400000L),
			Width = Sc(140)
		};
		Add(tableLayoutPanel, "Tempo desde o início (ms):", numT);
		NumericUpDown numX = null;
		NumericUpDown numY = null;
		NumericUpDown numW = null;
		EvKind kind = src.Kind;
		if ((uint)(kind - 3) <= 5u)
		{
			numX = new NumericUpDown
			{
				Minimum = 0m,
				Maximum = 100000m,
				Value = Math.Clamp(src.X, 0, 100000),
				Width = Sc(140)
			};
			numY = new NumericUpDown
			{
				Minimum = 0m,
				Maximum = 100000m,
				Value = Math.Clamp(src.Y, 0, 100000),
				Width = Sc(140)
			};
			Add(tableLayoutPanel, "Coordenada X:", numX);
			Add(tableLayoutPanel, "Coordenada Y:", numY);
		}
		if (src.Kind == EvKind.Wheel)
		{
			numW = new NumericUpDown
			{
				Minimum = -1200m,
				Maximum = 1200m,
				Increment = 120m,
				Value = Math.Clamp(src.Wheel, -1200, 1200),
				Width = Sc(140)
			};
			Add(tableLayoutPanel, "Delta da roda:", numW);
		}
		else
		{
			Add(tableLayoutPanel, "Detalhe:", new Label
			{
				Text = src.Detail,
				AutoSize = false,
				Height = Sc(26),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			});
		}
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Bottom,
			FlowDirection = FlowDirection.RightToLeft,
			Height = Sc(48),
			Padding = new Padding(Sc(12), Sc(8), Sc(12), Sc(8))
		};
		Button button = new Button
		{
			Text = "OK",
			Width = Sc(96),
			Height = Sc(30),
			DialogResult = DialogResult.OK
		};
		Button button2 = new Button
		{
			Text = "Cancelar",
			Width = Sc(96),
			Height = Sc(30),
			DialogResult = DialogResult.Cancel
		};
		flowLayoutPanel.Controls.Add(button);
		flowLayoutPanel.Controls.Add(button2);
		base.Controls.Add(tableLayoutPanel);
		base.Controls.Add(flowLayoutPanel);
		base.AcceptButton = button;
		base.CancelButton = button2;
		DarkUi.Apply(this);
		HudChrome.Attach(this, Text);
		button.Click += delegate
		{
			eventEditDialog.Result.T = (long)numT.Value;
			if (numX != null)
			{
				eventEditDialog.Result.X = (int)numX.Value;
			}
			if (numY != null)
			{
				eventEditDialog.Result.Y = (int)numY.Value;
			}
			if (numW != null)
			{
				eventEditDialog.Result.Wheel = (int)numW.Value;
			}
		};
	}

	private static void Add(TableLayoutPanel t, string label, Control c)
	{
		t.Controls.Add(new Label
		{
			Text = label,
			AutoSize = false,
			Height = 26,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		});
		c.Dock = ((c is Label) ? DockStyle.Fill : DockStyle.Left);
		t.Controls.Add(c);
	}
}
