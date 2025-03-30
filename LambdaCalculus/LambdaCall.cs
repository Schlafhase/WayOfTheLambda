using System.Reflection;

namespace LambdaCalculus;

public class LambdaCall : LambdaExpression
{
	private LambdaExpression _function;
	private LambdaExpression _argument;

	public LambdaExpression Function
	{
		get => _function;
		set
		{
			_function = value;
			_function.Parent = this;
		}
	}

	public LambdaExpression Argument
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
		if (Function is LambdaDefinition)
		{
			if (Argument is LambdaDefinition or LambdaCall)
			{
				return "(" + Function + ") (" + Argument + ")";
			}
			return "(" + Function + ") " + Argument;
		}

		return Argument switch
		{
			LambdaDefinition => Function + " (" + Argument + ")",
			LambdaCall       => Function + " (" + Argument + ")",
			_                => Function + " " + Argument
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
	

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		return new LambdaCall
		{
			Function = Function.Substitute(variable, expression),
			Argument = Argument.Substitute(variable, expression)
		};
	}

	public override LambdaExpression AlphaConvert()
	{
		return new LambdaCall
		{
			Function = Function.AlphaConvert(),
			Argument = Argument.AlphaConvert()
		};
	}

	public override string ToBruijnIndex()
	{
		return Function.ToBruijnIndex() + " " + Argument.ToBruijnIndex();
	}

	public override LambdaExpression BetaReduce()
	{
		if (Function is not LambdaDefinition lambdaDefinition)
		{
			return new LambdaCall
			{
				Function = Function.BetaReduce(),
				Argument = Argument.BetaReduce()
			};
		}

		LambdaDefinition? newDefinition = (lambdaDefinition.AlphaConvert() as LambdaDefinition);
		return newDefinition.Body.Substitute(newDefinition.CapturedVariable, Argument.AlphaConvert());

	}

	public override bool VariableIsFree(string name)
	{
		return Function.VariableIsFree(name) && Argument.VariableIsFree(name);
	}

	public override bool Equals(LambdaExpression? other)
	{
		return other is LambdaCall call &&
			Function.Equals(call.Function) &&
			Argument.Equals(call.Argument);
	}
}