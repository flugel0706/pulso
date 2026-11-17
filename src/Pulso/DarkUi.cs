using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public static class DarkUi
{
	public static void DarkScroll(Control c)
	{
		Control c2 = c;
		if (c2.IsHandleCreated)
		{
			Apply();
		}
		c2.HandleCreated += delegate
		{
			Apply();
		};
		void Apply()
		{
			try
			{
				Native.SetWindowTheme(c2.Handle, "DarkMode_Explorer", null);
			}
			catch
			{
			}
		}
	}

	public static void DarkTitle(Form f)
	{
		ModernWindow.Apply(f, rounded: true);
	}

	public static void DarkSpin(NumericUpDown nu)
	{
		NumericUpDown nu2 = nu;
		if (nu2.IsHandleCreated)
		{
			Apply();
		}
		nu2.HandleCreated += delegate
		{
			Apply();
		};
		void Apply()
		{
			try
			{
				Native.SetWindowTheme(nu2.Handle, "DarkMode_Explorer", null);
				foreach (Control control in nu2.Controls)
				{
					if (control.GetType().Name.Contains("UpDownButtons"))
					{
						control.Visible = false;
					}
					else
					{
						Native.SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
					}
				}
			}
			catch
			{
			}
		}
	}

	public static void Apply(Form f)
	{
		f.BackColor = Palette.Bg;
		f.ForeColor = Palette.Text;
		Style(f);
	}

	public static void Style(Control root)
	{
		bool flag = root is HudButton;
		if (!flag)
		{
			bool flag2 = ((root is BrandMark || root is StatusDot) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return;
		}
		if (root is CardPanel)
		{
			foreach (Control control in root.Controls)
			{
				Style(control);
			}
			return;
		}
		if (!(root is TextBox textBox))
		{
			if (!(root is NumericUpDown numericUpDown))
			{
				if (!(root is CheckBox checkBox))
				{
					if (!(root is Label label))
					{
						if (!(root is GroupBox groupBox))
						{
							if (!(root is Button button))
							{
								if (!(root is FlowLayoutPanel flowLayoutPanel))
								{
									if (!(root is TableLayoutPanel tableLayoutPanel))
									{
										if (root is Panel panel && (panel.BackColor == SystemColors.Control || panel.BackColor == Color.White))
										{
											panel.BackColor = Palette.Bg;
										}
									}
									else if (tableLayoutPanel.BackColor == SystemColors.Control)
									{
										tableLayoutPanel.BackColor = Color.Transparent;
									}
								}
								else
								{
									flowLayoutPanel.BackColor = Palette.Bg;
								}
							}
							else
							{
								button.FlatStyle = FlatStyle.Flat;
								button.FlatAppearance.BorderColor = Palette.Border;
								button.FlatAppearance.MouseOverBackColor = Palette.PanelHover;
								button.FlatAppearance.MouseDownBackColor = Palette.PanelAlt;
								button.BackColor = Palette.PanelAlt;
								button.ForeColor = Palette.Text;
								button.Height = Math.Max(button.Height, 32);
							}
						}
						else
						{
							groupBox.ForeColor = Palette.Text;
							groupBox.BackColor = Palette.Panel;
						}
					}
					else
					{
						label.BackColor = Color.Transparent;
						if (label.ForeColor == Color.DimGray || label.ForeColor == Palette.Muted)
						{
							label.ForeColor = Palette.Muted;
						}
						else if (label.ForeColor != Palette.Accent && label.ForeColor != Palette.Danger && label.ForeColor != Palette.Success)
						{
							label.ForeColor = Palette.Text;
						}
					}
				}
				else
				{
					checkBox.ForeColor = Palette.TextSoft;
					checkBox.BackColor = Color.Transparent;
					checkBox.FlatStyle = FlatStyle.Flat;
				}
			}
			else
			{
				numericUpDown.BackColor = Palette.PanelAlt;
				numericUpDown.ForeColor = Palette.Text;
				numericUpDown.BorderStyle = BorderStyle.FixedSingle;
				numericUpDown.TextAlign = HorizontalAlignment.Right;
				DarkSpin(numericUpDown);
			}
		}
		else
		{
			textBox.BackColor = Palette.PanelAlt;
			textBox.ForeColor = Palette.Text;
			textBox.BorderStyle = BorderStyle.FixedSingle;
		}
		foreach (Control control2 in root.Controls)
		{
			Style(control2);
		}
	}

	public static void StyleGrid(DataGridView g)
	{
		DarkScroll(g);
		g.BackgroundColor = Palette.Panel;
		g.GridColor = Palette.GridLine;
		g.BorderStyle = BorderStyle.None;
		g.EnableHeadersVisualStyles = false;
		g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
		g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
		g.ColumnHeadersHeight = 40;
		g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
		{
			BackColor = Palette.Panel,
			ForeColor = Palette.Muted,
			SelectionBackColor = Palette.Panel,
			SelectionForeColor = Palette.Muted,
			Font = UiFont.TinyBold,
			Alignment = DataGridViewContentAlignment.MiddleLeft,
			Padding = new Padding(10, 0, 0, 0),
			WrapMode = DataGridViewTriState.False
		};
		g.DefaultCellStyle = new DataGridViewCellStyle
		{
			BackColor = Palette.Panel,
			ForeColor = Palette.TextSoft,
			SelectionBackColor = Palette.Selection,
			SelectionForeColor = Color.White,
			Font = UiFont.Body,
			Padding = new Padding(10, 0, 0, 0),
			WrapMode = DataGridViewTriState.False
		};
		g.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
		{
			BackColor = Color.FromArgb(24, 27, 37),
			ForeColor = Palette.TextSoft,
			SelectionBackColor = Palette.Selection,
			SelectionForeColor = Color.White,
			Padding = new Padding(10, 0, 0, 0)
		};
		g.RowTemplate.Height = 31;
		g.RowsDefaultCellStyle.SelectionBackColor = Palette.Selection;
		g.RowHeadersWidth = 4;
		g.ScrollBars = ScrollBars.Vertical;
		g.AllowUserToResizeRows = false;
	}
}
