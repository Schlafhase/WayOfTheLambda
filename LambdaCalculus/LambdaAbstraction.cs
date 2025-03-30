using System.Diagnostics;

namespace LambdaCalculus;

public class LambdaAbstraction : LambdaExpression
{
	private LambdaVariable _capturedVariable;
	private LambdaExpression? _body;

	public LambdaVariable CapturedVariable
	{
		get => _capturedVariable;
		set
		{
			_capturedVariable = value;
			_capturedVariable.Parent = this;
		}
	}

	public LambdaExpression? Body
	{
		get => _body;
		set
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

		return new LambdaAbstraction
		{
			CapturedVariable = CapturedVariable,
			Body = Body.Substitute(variable, expression)
		};
	}

	public override LambdaExpression AlphaConvert()
	{
		// string newName = Guid.NewGuid().ToString();
		string newName = GetFreeVariableName(CapturedVariable.Name);

		return new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = newName },
			Body = Body.Substitute(CapturedVariable, new LambdaVariable { Name = newName, Parent = this}).AlphaConvert()
		};
	}

	public override string ToBruijnIndex()
	{
		return "λ." + Body.ToBruijnIndex();
	}

	public override LambdaExpression? BetaReduce(bool checkForBetaNormalForm = true)
	{
		if (checkForBetaNormalForm && IsBetaNormalForm())
		{
			return null;
		}
		
		return new LambdaAbstraction
		{
			CapturedVariable = CapturedVariable.Clone() as LambdaVariable,
			Body = Body.BetaReduce(false)!
		};
	}

	public override bool VariableIsFree(string name)
	{
		return CapturedVariable.Name != name && Body.VariableIsFree(name);
	}
}