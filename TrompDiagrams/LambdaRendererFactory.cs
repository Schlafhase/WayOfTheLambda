using LambdaCalculus;

namespace TrompDiagrams;

public static class LambdaRendererFactory
{
	public static ILambdaRenderer CreateLambdaRenderer(LambdaExpression? e)
	{
		return e switch
		{
			LambdaVariable v    => new LambdaVariableRenderer(v),
			LambdaAbstraction d => new LambdaAbstractionRenderer(d),
			LambdaApplication c => new LambdaApplicationRenderer(c),
			_                   => throw new NotImplementedException()
		};
	}
}