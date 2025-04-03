using System.Text.Json.Serialization;

namespace TrompDiagrams.Rendering;

public class VerticalLine : Line
{
	[JsonIgnore] public override int Width => 2; // Padding
	[JsonIgnore] public override int Height => Length;
	public override int X2 => X;
	public override int Y2 => Y + Length;
}