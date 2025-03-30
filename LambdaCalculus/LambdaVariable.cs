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
		return this == variable ? expression : this;
	}

	public override LambdaExpression AlphaConvert()
	{
		return this;
	}

	public override string ToBruijnIndex()
	{
		int index = 0;
		LambdaExpression? current = Parent;
		
		while (current is not null)
		{
			if (current is LambdaDefinition lambdaDefinition)
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

	public override LambdaExpression BetaReduce()
	{
		return this;
	}
	
	public override bool VariableIsFree(string name)
	{
		return true;
	}

	public LambdaDefinition? CapturingLambda
	{
		get
		{
			LambdaExpression? current = Parent;
			
			while (current is not null)
			{
				if (current is LambdaDefinition lambdaDefinition)
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

	public override bool Equals(LambdaExpression? other)
	{
		if (other is LambdaVariable variable)
		{
			return Name == variable.Name && ReferenceEquals(CapturingLambda, variable.CapturingLambda);
		}

		return false;
	}
}