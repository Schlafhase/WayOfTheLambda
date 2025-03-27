using System.Text.RegularExpressions;

namespace LambdaCalculus;

public abstract partial class LambdaExpression : IEquatable<LambdaExpression>
{
	public abstract LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression);
	
	public abstract bool Equals(LambdaExpression? other);

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((LambdaExpression)obj);
	}
	
	public static bool operator ==(LambdaExpression? left, LambdaExpression? right)
	{
		return left?.Equals(right) ?? right is null;
	}
	
	public static bool operator !=(LambdaExpression? left, LambdaExpression? right)
	{
		return !left?.Equals(right) ?? right is not null;
	}

	public static LambdaExpression TRUE() =>
		new LambdaDefinition
		{
			CapturedVariable = new LambdaVariable { Name = "x" },
			Body = new LambdaDefinition
			{
				CapturedVariable = new LambdaVariable { Name = "y" },
				Body = new LambdaVariable { Name = "x" }
			}
		};

	public static LambdaExpression FALSE() =>
		new LambdaDefinition
		{
			CapturedVariable = new LambdaVariable { Name = "x" },
			Body = new LambdaDefinition
			{
				CapturedVariable = new LambdaVariable { Name = "y" },
				Body = new LambdaVariable { Name = "y" }
			}
		};
	
	public static LambdaExpression IF() =>
		new LambdaDefinition
		{
			CapturedVariable = new LambdaVariable { Name = "p" },
			Body = new LambdaDefinition
			{
				CapturedVariable = new LambdaVariable { Name = "a" },
				Body = new LambdaDefinition
				{
					CapturedVariable = new LambdaVariable { Name = "b" },
					Body = new LambdaCall
					{
						Function = new LambdaCall
						{
							Function = new LambdaVariable { Name = "p" },
							Argument = new LambdaVariable { Name = "a" }
						},
						Argument = new LambdaVariable { Name = "b" }
					}
				}
			}
		};
	
	public static LambdaExpression NOT() =>
		 new LambdaDefinition
		{
			CapturedVariable = new LambdaVariable { Name = "p" },
			Body = new LambdaCall
			{
				Function = new LambdaCall
				{
					Function = new LambdaVariable { Name = "p" },
					Argument = FALSE()
				},
				Argument = TRUE()
			}
		};
	
	public static LambdaExpression Parse(string input)
	{
		return parseRecursive(input);
	}

	
	private static LambdaExpression parseRecursive(string input)
	{
		if (lambdaVariableRegex.Match(input).Success)
		{
			return new LambdaVariable { Name = input };
		}
		
		if (input.StartsWith('λ'))
		{
			string lambdaVariable;
			
			try
			{
				lambdaVariable = input.Substring(1, input.IndexOf('.') - 1).Trim();
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentException("Invalid lambda expression: " + input);
			}
			
			if (!lambdaVariableRegex.Match(lambdaVariable).Success)
			{
				throw new ArgumentException("Invalid lambda variable: " + lambdaVariable);
			}

			string body = removeOuterParentheses(input[(input.IndexOf('.') + 1)..].Trim());
			return new LambdaDefinition
			{
				CapturedVariable = new LambdaVariable { Name = lambdaVariable },
				Body = parseRecursive(body)
			};
		}
		
		throw new ArgumentException("Invalid lambda expression: " + input);
	}

	private static string removeOuterParentheses(string input)
	{
		int parenthesesDepth = 0;
		string body = "";
			
		foreach (char c in input)
		{
			switch (c)
			{
				case '(':
					parenthesesDepth++;
					break;
				case ')':
					parenthesesDepth--;
					break;
			}

			if (parenthesesDepth < 0)
			{
				break;
			}
			body += c;
		}
		return body;
	}
	
	[GeneratedRegex(@"^ *[a-z0-9]+ *$")]
	private static partial Regex lambdaVariableRegexGenerator();
	private static readonly Regex lambdaVariableRegex = lambdaVariableRegexGenerator();
}