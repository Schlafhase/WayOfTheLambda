namespace LambdaCalculus;

public class LambdaVariable : LambdaExpression
{
	public string Name { get; set; }
	public Guid Id { get; set; } = Guid.NewGuid();
	public LambdaDefinition? CapturingLambda { get; set; }
	
	public override string ToString()
	{
		return Name;
	}

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		if (expression is LambdaDefinition lambdaDefinition)
		{
			expression = new LambdaDefinition
			{
				CapturedVariable = lambdaDefinition.CapturedVariable,
				Body = lambdaDefinition.Body
			};
		}
		return this == variable ? expression : this;
	}
	
	public override bool Equals(LambdaExpression? other)
	{
		if (other is LambdaVariable variable)
		{
			return Id == variable.Id;
		}
		return false;
	}
}