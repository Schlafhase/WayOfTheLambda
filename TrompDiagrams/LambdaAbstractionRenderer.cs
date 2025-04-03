using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaAbstractionRenderer(LambdaAbstraction a) : ILambdaRenderer
{
	public Geometry Render(Dictionary<string, int> variableHeights, int currentHeight = 0)
	{
		Geometry g = new();

		Dictionary<string, int> newHeights = variableHeights.ToDictionary();
		newHeights[a.CapturedVariable.Name] = currentHeight;

		Geometry body = LambdaRendererFactory.CreateLambdaRenderer(a.Body).Render(newHeights, currentHeight + 2);

		(int Width, int Height) bodyDimensions = body.GetDimensions();

		Line abstraction = new HorizontalLine
		{
			Length = bodyDimensions.Width
		};

		g += abstraction;

		g.CombineWithOffset(body, (0, 2));

		return g;
	}
}