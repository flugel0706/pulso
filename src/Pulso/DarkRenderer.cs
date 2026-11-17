using System.Windows.Forms;

namespace Pulso;

public sealed class DarkRenderer : ToolStripProfessionalRenderer
{
	public DarkRenderer()
		: base(new DarkColorTable())
	{
		base.RoundedEdges = true;
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		e.TextColor = ((!e.Item.Enabled) ? Palette.Muted : (e.Item.Selected ? Palette.AccentSoft : Palette.TextSoft));
		base.OnRenderItemText(e);
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		e.ArrowColor = Palette.Muted;
		base.OnRenderArrow(e);
	}
}
