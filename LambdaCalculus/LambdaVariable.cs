namespace LambdaCalculus;

public class LambdaVariable : LambdaExpression
{
	public string Name { get; init; }

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

	public override string ToString() => Name;

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression) =>
		CapturingLambda?.AlphaEquivalent(variable.CapturingLambda) ?? false ? expression : Clone();

	public override LambdaExpression AlphaConvert(LambdaExpression root) => Clone();

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

	public override LambdaExpression? BetaReduce(bool checkForBetaNormalForm = true) =>
		checkForBetaNormalForm ? null : Clone();

	public override bool VariableIsFree(string name) => true;
}