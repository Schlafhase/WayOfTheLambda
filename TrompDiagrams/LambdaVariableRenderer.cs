using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaVariableRenderer(LambdaVariable v) : ILambdaRenderer
{
	public Geometry Render(Dictionary<Guid, int> variableHeights, int currentHeight = 0)
	{
		int y = 0;
		
		if (variableHeights.TryGetValue(v.Id, out int height))
		{
			y = -(currentHeight - height);
		}
		
		return new Geometry() + new VerticalLine
		{
			X = 1,
			Y = y,
			Length = currentHeight - y
		};
	}
}