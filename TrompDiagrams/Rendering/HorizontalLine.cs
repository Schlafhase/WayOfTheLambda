using Newtonsoft.Json;

namespace TrompDiagrams.Rendering;

public class HorizontalLine : Line
{
	[JsonIgnore] public override int Width => Length;
	[JsonIgnore] public override int Height => 1;
	public override int X2 => X + Length;
	public override int Y2 => Y;
}