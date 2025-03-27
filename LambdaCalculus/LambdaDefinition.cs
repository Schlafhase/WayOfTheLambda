namespace LambdaCalculus;

public class LambdaDefinition : LambdaExpression
{
	private LambdaVariable _capturedVariable;
	private readonly LambdaExpression _body;
	
	private readonly Guid _id = Guid.NewGuid();

	public LambdaVariable CapturedVariable
	{
		get => _capturedVariable;
		init
		{
			_capturedVariable = value;
			value.CapturingLambda = this;
		}
	}

	public LambdaExpression Body
	{
		get => _body;
		init
		{
			_body = value;
			setCapturingLambdaRecursive(this, value);
		}
	}

	private static void setCapturingLambdaRecursive(LambdaDefinition parentLambda, LambdaExpression body)
	{
		while (true)
		{
			switch (body)
			{
				case LambdaVariable variable when variable.Name == parentLambda.CapturedVariable.Name:
					variable.CapturingLambda = parentLambda;
					variable.Id = parentLambda.CapturedVariable.Id;
					break;
				case LambdaDefinition definition when definition.CapturedVariable.Name == parentLambda.CapturedVariable.Name:
					throw new ArgumentException("Conflicting variable name " + parentLambda.CapturedVariable.Name);
				case LambdaDefinition definition:
					body = definition.Body;
					continue;
				case LambdaCall call:
					setCapturingLambdaRecursive(parentLambda, call.Function);
					body = call.Argument;
					continue;
			}

			break;
		}
	}

	public override string ToString()
	{
		return "λ" + CapturedVariable + "." + Body;
	}

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		if (variable == CapturedVariable)
		{
			return this;
		}

		return new LambdaDefinition
		{
			CapturedVariable = CapturedVariable,
			Body = Body.Substitute(variable, expression)
		};
	}

	public override bool Equals(LambdaExpression? other)
	{
		return other is LambdaDefinition definition &&
			_id == definition._id;
		// return other is LambdaDefinition definition &&
		// 	CapturedVariable.Equals(definition.CapturedVariable) &&
		// 	Body.Equals(definition.Body);
	}
}