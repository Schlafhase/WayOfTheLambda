namespace TrompDiagrams.Rendering;

public class HorizontalLine : Line
{
	public override int Width => Length;
	public override int Height => 1; // Padding
}