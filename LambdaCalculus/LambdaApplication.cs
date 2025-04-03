namespace LambdaCalculus;

public class LambdaApplication : LambdaExpression
{
	private LambdaExpression? _argument;
	private LambdaExpression? _function;

	public LambdaExpression? Function
	{
		get => _function;
		set
		{
			_function = value;
			_function.Parent = this;
		}
	}

	public LambdaExpression? Argument
	{
		get => _argument;
		set
		{
			_argument = value;
			_argument.Parent = this;
		}
	}

	public override string ToString()
	{
		if (Function is LambdaAbstraction)
		{
			if (Argument is LambdaAbstraction or LambdaApplication)
			{
				return "(" + Function + ") (" + Argument + ")";
			}

			return "(" + Function + ") " + Argument;
		}

		return Argument switch
		{
			LambdaAbstraction => Function + " (" + Argument + ")",
			LambdaApplication => Function + " (" + Argument + ")",
			_                 => Function + " " + Argument
		};
	}

	// private static bool needsParentheses(LambdaExpression left)
	// {
	// 	if (left is not LambdaCall call)
	// 	{
	// 		return left is LambdaDefinition;
	// 	}
	//
	// 	return call.Function switch
	// 	{
	// 		LambdaVariable _ => call.Argument is not LambdaVariable || needsParentheses(call.Argument),
	// 		LambdaCall _     => needsParentheses(call.Function) || needsParentheses(call.Argument),
	// 		_                => true
	// 	};
	// }


	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression) =>
		new LambdaApplication
		{
			Function = Function.Substitute(variable, expression),
			Argument = Argument.Substitute(variable, expression)
		};

	public override LambdaExpression AlphaConvert(LambdaExpression root) =>
		new LambdaApplication
		{
			Function = Function.AlphaConvert(root),
			Argument = Argument.AlphaConvert(root)
		};

	public override string ToBruijnIndex()
	{
		string bruijn = "(" + Function.ToBruijnIndex() + ")(" + Argument.ToBruijnIndex() + ")";
		return bruijn;
	}

	public override LambdaExpression? BetaReduce(bool checkForBetaNormalForm = true)
	{
		if (checkForBetaNormalForm && IsBetaNormalForm())
		{
			return null;
		}

		if (Function is not LambdaAbstraction lambdaDefinition)
		{
			return new LambdaApplication
			{
				Function = Function.BetaReduce(false)!,
				Argument = Argument.BetaReduce(false)!
			};
		}

		LambdaExpression argument = Argument.AlphaConvert(lambdaDefinition);
		return lambdaDefinition.Body.Substitute(lambdaDefinition.CapturedVariable, argument);
	}

	public override bool VariableIsFree(string name) => Function.VariableIsFree(name) && Argument.VariableIsFree(name);
}