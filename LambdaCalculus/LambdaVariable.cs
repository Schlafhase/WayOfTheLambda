namespace LambdaCalculus;

public class LambdaVariable : LambdaExpression
{
	public string Name { get; init; }

	public override string ToString()
	{
		return Name;
	}

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		return CapturingLambda?.AlphaEquivalent(variable.CapturingLambda) ?? false ? expression : Clone(); // TODO: Probably replace alpha equivalent
	}

	public override LambdaExpression AlphaConvert(LambdaExpression root)
	{
		return Clone();
	}

	public override string ToBruijnIndex()
	{
		int index = 0;
		LambdaExpression? current = Parent;
		
		while (current is not null)
		{
			if (current is LambdaAbstraction lambdaDefinition)
			{
				index++;
				if (lambdaDefinition.CapturedVariable.Name == Name)
				{
					return index.ToString();
				}
			}

			current = current.Parent;
		}

		return "0";
	}

	public override LambdaExpression? BetaReduce(bool checkForBetaNormalForm = true)
	{
		return checkForBetaNormalForm ? null : Clone();
	}
	
	public override bool VariableIsFree(string name)
	{
		return true;
	}

	public LambdaAbstraction? CapturingLambda
	{
		get
		{
			LambdaExpression? current = Parent;
			
			while (current is not null)
			{
				if (current is LambdaAbstraction lambdaDefinition)
				{
					if (lambdaDefinition.CapturedVariable.Name == Name)
					{
						return lambdaDefinition;
					}
				}

				current = current.Parent;
			}
			return null;
		}
	}
}