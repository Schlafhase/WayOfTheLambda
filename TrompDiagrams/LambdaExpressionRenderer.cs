using LambdaCalculus;
using TrompDiagrams.Rendering;

namespace TrompDiagrams;

public static class LambdaExpressionRenderer
{
	public static Geometry Render(LambdaExpression lambdaExpression)
	{
		return lambdaExpression switch
		{
			LambdaVariable lambdaVariable     => new LambdaVariableRenderer(lambdaVariable).Render([]),
			LambdaApplication lambdaCall             => new LambdaCallRenderer(lambdaCall).Render([]),
			LambdaAbstraction lambdaDefinition => new LambdaDefinitionRenderer(lambdaDefinition).Render([]),
			_                                 => throw new NotImplementedException()
		};
	}
}