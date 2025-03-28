using System.Runtime.Versioning;
using System.Text;
using SkiaSharp;

namespace TrompDiagrams.Rendering;

public class Geometry
{
	public List<Line> Lines { get; init; } = [];

	public (int Width, int Height) GetDimensions(bool includeNegative = false)
	{
		(int X, int Y) offset = GetNegativeOffset(this);
		
		int width = 0;
		int height = 0;

		foreach (Line line in Lines)
		{
			if (includeNegative)
			{
				width = Math.Max(width, line.X + line.Width + offset.X);
				height = Math.Max(height, line.Y + line.Height + offset.Y);
			}
			else
			{
				width = Math.Max(width, line.X + line.Width);
				height = Math.Max(height, line.Y + line.Height);
			}
		}

		return (width, height);
	}
	
	public static (int X, int Y) GetNegativeOffset(Geometry geometry)
	{
		int offsetX = -geometry.Lines.Min(l => l.X);
		int offsetY = -geometry.Lines.Min(l => l.Y);
		return (offsetX, offsetY);
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

	public override string ToString()
	{
		(int Width, int Height) dim = GetDimensions(true);
		(int X, int Y) offset = GetNegativeOffset(this);
		
		Geometry offsetGeometry = new();
		offsetGeometry.CombineWithOffset(this, offset);
        
        List<string> lines = [];
        
        for (int y = 0; y < dim.Height; y++)
        {
        	lines.Add(new StringBuilder(dim.Width).Insert(0, " ", dim.Width).ToString());
        }
        
        foreach (Line line in offsetGeometry.Lines)
        {
        	switch (line)
        		{
        			case HorizontalLine hLine:
        			{
        				for (int x = hLine.X; x < hLine.X + hLine.Length; x++)
        				{
        					try
        					{
        						lines[hLine.Y] = string.Concat(lines[hLine.Y].AsSpan()[..x], "#", lines[hLine.Y].AsSpan(x + 1));
        					}
        					catch (IndexOutOfRangeException) { }
        				}
        	
        				break;
        			}
        			case VerticalLine vLine:
        			{
        				for (int y = vLine.Y; y < vLine.Y + vLine.Length; y++)
        				{
        					try
        					{
        						for (int i = vLine.Y; i < vLine.Y + vLine.Length; i++)
        						{
        							lines[i] = string.Concat(lines[i].AsSpan()[..vLine.X], "#", lines[i].AsSpan(vLine.X + 1));
        						}
        					}
        					catch (IndexOutOfRangeException) { }
        				}
        	
        				break;
        			}
        		}
			
        }
        
        // stretch
        for (int i = 0; i < lines.Count; i++)
        {
        	string line = lines[i];
        	string newLine = "";
        
        	foreach (char c in line)
        	{
        		newLine += c;
        		newLine += c;
        		newLine += c;
        	}
        	
        	lines[i] = newLine;
        }
		return string.Join(Environment.NewLine, lines);
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

	public SKBitmap ToBitmap()
	{
		(int Width, int Height) dim = GetDimensions(true);
		(int X, int Y) offset = GetNegativeOffset(this);
		
		Geometry offsetGeometry = new();
		offsetGeometry.CombineWithOffset(this, offset);
		
		SKBitmap bitmap = new(dim.Width, dim.Height);
		
		using SKCanvas canvas = new(bitmap);
		
		foreach (Line line in offsetGeometry.Lines)
		{
			switch (line)
			{
				case HorizontalLine hLine:
				{
					canvas.DrawLine(hLine.X, hLine.Y, hLine.X + hLine.Length, hLine.Y, new SKPaint { Color = SKColors.Black });
					break;
				}
				case VerticalLine vLine:
				{
					canvas.DrawLine(vLine.X, vLine.Y, vLine.X, vLine.Y + vLine.Length, new SKPaint { Color = SKColors.Black });
					break;
				}
			}
		}
		
		return bitmap;
	}
}