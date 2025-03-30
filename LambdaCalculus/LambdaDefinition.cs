using System.Diagnostics;

namespace LambdaCalculus;

public class LambdaDefinition : LambdaExpression
{
	private LambdaVariable _capturedVariable;
	private LambdaExpression _body;

	public LambdaVariable CapturedVariable
	{
		get => _capturedVariable;
		init
		{
			_capturedVariable = value;
			_capturedVariable.Parent = this;
		}
	}

	public LambdaExpression Body
	{
		get => _body;
		init
		{
			_body = value;
			_body.Parent = this;
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

	public override LambdaExpression AlphaConvert()
	{
		// string newName = Guid.NewGuid().ToString();
		string newName = GetFreeVariableName(CapturedVariable.Name);

		return new LambdaDefinition
		{
			CapturedVariable = new LambdaVariable { Name = newName },
			Body = Body.Substitute(CapturedVariable, new LambdaVariable { Name = newName }).AlphaConvert()
		};
	}

	public override string ToBruijnIndex()
	{
		return "λ." + Body.ToBruijnIndex();
	}

	public override LambdaExpression BetaReduce()
	{
		return new LambdaDefinition
		{
			CapturedVariable = CapturedVariable,
			Body = Body.BetaReduce()
		};
	}

	public override bool VariableIsFree(string name)
	{
		return CapturedVariable.Name != name && Body.VariableIsFree(name);
	}
	
	public override bool Equals(LambdaExpression? other)
	{
		return other is LambdaDefinition definition &&
			CapturedVariable.Equals(definition.CapturedVariable) &&
			Body.Equals(definition.Body);
	}
}