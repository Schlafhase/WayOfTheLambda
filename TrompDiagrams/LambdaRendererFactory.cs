using LambdaCalculus;

namespace TrompDiagrams;

public static class LambdaRendererFactory
{
	public static ILambdaRenderer CreateLambdaRenderer(LambdaExpression? e)
	{
		return e switch
		{
			LambdaVariable v   => new LambdaVariableRenderer(v),
			LambdaAbstraction d => new LambdaDefinitionRenderer(d),
			LambdaApplication c       => new LambdaCallRenderer(c),
			_                  => throw new NotImplementedException()
		};
	}
}