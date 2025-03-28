using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaDefinitionRenderer(LambdaDefinition d) : ILambdaRenderer
{
	public Geometry Render(Dictionary<string, int> variableHeights, int currentHeight = 0)
	{
		Geometry g = new();
		
		Dictionary<string, int> newHeights = variableHeights.ToDictionary();
		newHeights[d.CapturedVariable.Name] = currentHeight;
		
		Geometry body = LambdaRendererFactory.CreateLambdaRenderer(d.Body).Render(newHeights, currentHeight + 2);
		
		(int Width, int Height) bodyDimensions = body.GetDimensions();

		Line abstraction = new HorizontalLine
		{
			Length = bodyDimensions.Width
		};
		
		g += abstraction;

		// int xOffset = d.Body is LambdaDefinition ? 0 : 1;
		// int yOffset = d.Body is LambdaDefinition ? 2 : 1;
		//
		// g.CombineWithOffset(body, (xOffset, yOffset));
		
		g.CombineWithOffset(body, (0, 2));
		
		return g;
	}
}