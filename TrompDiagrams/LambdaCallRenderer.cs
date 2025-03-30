using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public class LambdaCallRenderer(LambdaApplication c) : ILambdaRenderer
{
	public Geometry Render(Dictionary<string, int> variableHeights, int currentHeight = 0)
	{
		Geometry g = new();
		Geometry function = LambdaRendererFactory
							.CreateLambdaRenderer(c.Function)
							.Render(variableHeights, currentHeight);
		(int Width, int Height) functionDimensions = function.GetDimensions();

		Geometry argument = LambdaRendererFactory
							.CreateLambdaRenderer(c.Argument)
							.Render(variableHeights, currentHeight);
		(int Width, int Height) argumentDimensions = argument.GetDimensions();

		int functionExtension = functionDimensions.Height > (argumentDimensions.Height + 1)
			? 1
			: (argumentDimensions.Height + 1) - functionDimensions.Height;
		
		int argumentExtension = argumentDimensions.Height > (functionDimensions.Height + 1)
			? 1
			: (functionDimensions.Height + 1) - argumentDimensions.Height;

		function += new VerticalLine
		{
			X = 1,
			Y = functionDimensions.Height,
			Length = functionExtension + 1
		};
		argument += new VerticalLine
		{
			X = 1,
			Y = argumentDimensions.Height,
			Length = argumentExtension
		};

		g += function;
		g.CombineWithOffset(argument, (functionDimensions.Width + 1, 0));

		Line application = new HorizontalLine
		{
			X = 2,
			Y = functionDimensions.Height + functionExtension - 1,
			Length = functionDimensions.Width
		};

		g += application;

		// g.CombineWithOffset(argument, (application.X + application.Width - 1, -2));
		return g;
		
		// Geometry result = new Geometry();
		// result.CombineWithOffset(g, (0, 1));
		// return result;
	}
}