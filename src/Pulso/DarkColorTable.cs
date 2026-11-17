using System.Drawing;
using System.Windows.Forms;

namespace Pulso;

public sealed class DarkColorTable : ProfessionalColorTable
{
	public override Color ToolStripDropDownBackground => Palette.Panel;

	public override Color ImageMarginGradientBegin => Palette.Panel;

	public override Color ImageMarginGradientMiddle => Palette.Panel;

	public override Color ImageMarginGradientEnd => Palette.Panel;

	public override Color MenuBorder => Palette.Border;

	public override Color MenuItemBorder => Palette.Accent;

	public override Color MenuItemSelected => Palette.PanelAlt;

	public override Color MenuItemSelectedGradientBegin => Palette.PanelAlt;

	public override Color MenuItemSelectedGradientEnd => Palette.PanelAlt;

	public override Color MenuItemPressedGradientBegin => Palette.PanelAlt;

	public override Color MenuItemPressedGradientEnd => Palette.PanelAlt;

	public override Color MenuStripGradientBegin => Palette.Bg;

	public override Color MenuStripGradientEnd => Palette.Bg;

	public override Color ToolStripBorder => Palette.BorderSoft;

	public override Color ToolStripGradientBegin => Palette.Panel;

	public override Color ToolStripGradientMiddle => Palette.Panel;

	public override Color ToolStripGradientEnd => Palette.Panel;

	public override Color SeparatorDark => Palette.BorderSoft;

	public override Color SeparatorLight => Palette.BorderSoft;
}
