﻿namespace LambdaCalculus;

public class LambdaAbstraction : LambdaExpression
{
	private LambdaExpression? _body;
	private LambdaVariable _capturedVariable;

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

	public override string ToString() => "λ" + CapturedVariable + "." + Body;

	public override LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression)
	{
		if (variable.Name == CapturedVariable.Name)
		{
			return Clone();
		}

		return new LambdaAbstraction
		{
			CapturedVariable = CapturedVariable,
			Body = Body.Substitute(variable, expression)
		};
	}

	public override LambdaExpression AlphaConvert(LambdaExpression root)
	{
		// string newName = Guid.NewGuid().ToString();
		string newName = GetFreeVariableName(CapturedVariable.Name, root);

		return new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = newName },
			Body = Body.Substitute(CapturedVariable, new LambdaVariable { Name = newName }).AlphaConvert(root)
		};
	}

	public override string ToBruijnIndex() => "λ." + Body.ToBruijnIndex();

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

	public override bool VariableIsFree(string name) => CapturedVariable.Name != name && Body.VariableIsFree(name);
}