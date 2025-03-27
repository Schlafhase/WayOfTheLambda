namespace TrompDiagrams.Rendering;

public abstract class Line
{
	public int Length { get; init; } = 3;
	public int X { get; init; }
	public int Y { get; init; }
	
	public abstract int Width { get; }
	public abstract int Height { get; }
}