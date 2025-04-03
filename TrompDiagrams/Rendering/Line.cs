using Newtonsoft.Json;

namespace TrompDiagrams.Rendering;

public abstract class Line
{
	[JsonIgnore] public int Length { get; init; } = 3;
	public int X { get; init; }
	public int Y { get; init; }

	public abstract int X2 { get; }
	public abstract int Y2 { get; }

	[JsonIgnore] public abstract int Width { get; }
	[JsonIgnore] public abstract int Height { get; }
}