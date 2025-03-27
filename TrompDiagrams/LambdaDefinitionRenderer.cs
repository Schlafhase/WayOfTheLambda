using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaDefinitionRenderer(LambdaDefinition d) : ILambdaRenderer
{
	public Geometry Render(Dictionary<Guid, int> variableHeights, int currentHeight = 0)
	{
		Geometry g = new();
		variableHeights[d.CapturedVariable.Id] = currentHeight;
		Geometry body = LambdaRendererFactory.CreateLambdaRenderer(d.Body).Render(variableHeights, currentHeight + 2);
		
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