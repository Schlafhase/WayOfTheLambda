using System.Reflection;

namespace LambdaCalculus;

public class LambdaCall : LambdaExpression
{
	public LambdaExpression Function { get; set; }
	public LambdaExpression Argument { get; set; }
	
	public override string ToString()
	{
		if (needsParentheses(Function))
		{
			return "(" + Function + ") " + Argument;
		}

		if (Argument is LambdaCall)
		{
			return Function + " (" + Argument + ")";
		}
		return Function + " " + Argument;
	}
	
	private static bool needsParentheses(LambdaExpression left)
	{
		if (left is not LambdaCall call)
		{
			return left is LambdaDefinition;
		}

		return call.Function switch
		{
			LambdaVariable _ => call.Argument is not LambdaVariable || needsParentheses(call.Argument),
			LambdaCall _     => needsParentheses(call.Function) || needsParentheses(call.Argument),
			_                => true
		};
	}

	public LambdaExpression BetaReduce()
	{
		return Function switch
		{
			LambdaDefinition lambdaDefinition => lambdaDefinition.Body.Substitute(
				lambdaDefinition.CapturedVariable, Argument),
			LambdaCall call => new LambdaCall { Function = call.BetaReduce(), Argument = Argument },
			_               => this
		};
	}

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		return new LambdaCall
		{
			Function = Function.Substitute(variable, expression),
			Argument = Argument.Substitute(variable, expression)
		};
	}
	
	public override bool Equals(LambdaExpression? other)
	{
		return other is LambdaCall call &&
			Function.Equals(call.Function) &&
			Argument.Equals(call.Argument);
	}
}