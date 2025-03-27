using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaCallRenderer(LambdaCall c) : ILambdaRenderer
{
	public Geometry Render(Dictionary<Guid, int> variableHeights, int currentHeight = 0)
	{
		Geometry g = new();
		Geometry function = LambdaRendererFactory.CreateLambdaRenderer(c.Function).Render(variableHeights, currentHeight);
		(int Width, int Height) functionDimensions = function.GetDimensions();
		function += new VerticalLine
		{
			X = 1,
			Y = functionDimensions.Height,
			Length = 1
		};
		Geometry argument = LambdaRendererFactory.CreateLambdaRenderer(c.Argument).Render(variableHeights, currentHeight);

		g += function;

		Line application = new HorizontalLine
		{
			X = 1,
			Y = functionDimensions.Height - 1,
			Length = functionDimensions.Width + 1
		};

		g += application;

		g.CombineWithOffset(argument, (application.X + application.Width - 1, -2));

		Geometry result = new Geometry();
		result.CombineWithOffset(g, (0, 1));
		return result;
	}
}