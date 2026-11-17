namespace Pulso;

public sealed class CompiledPayload
{
	public string App { get; set; } = "Pulso";


	public int V { get; set; } = 2;


	public Macro Macro { get; set; } = new Macro();


	public Settings Play { get; set; } = new Settings();

}
