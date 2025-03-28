using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaVariableRenderer(LambdaVariable v) : ILambdaRenderer
{
	public Geometry Render(Dictionary<string, int> variableHeights, int currentHeight = 0)
	{
		int y = -currentHeight - 1;
		
		if (variableHeights.TryGetValue(v.Name, out int height))
		{
			y = -(currentHeight - height);
		}
		
		return new Geometry() + new VerticalLine
		{
			X = 1,
			Y = y,
			Length = - y
		};
	}
}