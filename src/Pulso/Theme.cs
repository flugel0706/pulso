using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public static class Theme
{
	public static void Glass(Control root)
	{
		foreach (Control control in root.Controls)
		{
			if (control is Label || control is FlowLayoutPanel || control.GetType() == typeof(Panel))
			{
				try
				{
					control.BackColor = Color.Transparent;
				}
				catch
				{
				}
			}
			Glass(control);
		}
	}
}
