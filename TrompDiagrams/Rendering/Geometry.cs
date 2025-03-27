namespace TrompDiagrams.Rendering;

public class Geometry
{
	public List<Line> Lines { get; init; } = [];

	public (int Width, int Height) GetDimensions()
	{
		int width = 0;
		int height = 0;

		foreach (Line line in Lines)
		{
			width = Math.Max(width, line.X + line.Width);
			height = Math.Max(height, line.Y + line.Height);
		}

		return (width, height);
	}

	public static Geometry operator +(Geometry geometry, Line line)
	{
		geometry.Lines.Add(line);
		return geometry;
	}

	public static Geometry operator +(Geometry geometry, Geometry other)
	{
		geometry.Lines.AddRange(other.Lines);
		return geometry;
	}

	public void CombineWithOffset(Geometry other, (int x, int y) offset)
	{
		foreach (Line line in other.Lines)
		{
			Line newLine = line switch
			{
				VerticalLine vl => new VerticalLine { X = vl.X + offset.x, Y = vl.Y + offset.y, Length = vl.Length },
				HorizontalLine hl => new HorizontalLine
				{
					X = hl.X + offset.x, Y = hl.Y + offset.y, Length = hl.Length
				},
				_ => throw new NotImplementedException()
			};

			this.Lines.Add(newLine);
		}
	}
}