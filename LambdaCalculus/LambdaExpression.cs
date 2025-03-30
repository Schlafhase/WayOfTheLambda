using System.Text.RegularExpressions;

namespace LambdaCalculus;

public abstract partial class LambdaExpression
{
	public LambdaExpression? Parent;

	public abstract LambdaExpression Substitute(LambdaVariable variable, LambdaExpression expression);

	/// <summary>
	/// Changes all variable names in an expression to unique names.
	/// </summary>
	/// <returns></returns>
	public abstract LambdaExpression AlphaConvert();

	public abstract string ToBruijnIndex();

	/// <summary>
	/// Substitutes all instances of a variable in an abstraction with the argument of the application.
	/// </summary>
	/// <param name="checkForBetaNormalForm"></param>
	/// <returns>A <see cref="LambdaExpression"/> if possible.</returns>
	public abstract LambdaExpression? BetaReduce(bool checkForBetaNormalForm = true);

	public bool IsBetaNormalForm()
	{
		return this switch
		{
			LambdaApplication application => application.Function is not LambdaAbstraction &&
				application.Function.IsBetaNormalForm() && application.Argument.IsBetaNormalForm(),

			LambdaAbstraction abstraction => abstraction.Body.IsBetaNormalForm(),
			_                             => true
		};
	}

	public LambdaExpression Root
	{
		get
		{
			LambdaExpression current = this;

			while (current.Parent is not null)
			{
				current = current.Parent;
			}

			return current;
		}
	}

	public string GetFreeVariableName(string name)
	{
		// TODO: eta conversion
		if (Root.VariableIsFree(name))
		{
			return name;
		}

		Match match = _numberedVariableRegex.Match(name);

		if (match.Success)
		{
			int index = int.Parse(match.Groups[1].Value) + 1;
			string newName = name[..match.Index] + "_";

			while (!Root.VariableIsFree(newName + index))
			{
				index++;
			}

			return newName + index;
		}
		else
		{
			int index = 1;
			string newName = name + "_";

			while (!Root.VariableIsFree(newName + index))
			{
				index++;
			}

			return newName + index;
		}
	}

	[GeneratedRegex(@"_(\d+)$", RegexOptions.Compiled)]
	private static partial Regex numberedVariableRegexGenerator();

	private static readonly Regex _numberedVariableRegex = numberedVariableRegexGenerator();

	public abstract bool VariableIsFree(string name);

	public bool AlphaEquivalent(LambdaExpression other)
	{
		return ToBruijnIndex() == other.ToBruijnIndex();
	}

	public LambdaExpression? Clone(LambdaExpression? parent = null)
	{
		LambdaExpression? clone;

		switch (this)
		{
			case LambdaVariable variable:
				clone = new LambdaVariable { Name = variable.Name, Parent = parent };
				break;
			case LambdaAbstraction definition:
				clone = new LambdaAbstraction
				{
					Parent = parent
				};
				(clone as LambdaAbstraction).CapturedVariable = definition.CapturedVariable.Clone(clone) as LambdaVariable;
				(clone as LambdaAbstraction).Body = definition.Body.Clone(clone);
				break;
			case LambdaApplication call:
				clone = new LambdaApplication
				{
					Parent = parent
				};
				(clone as LambdaApplication).Function = call.Function.Clone(clone);
				(clone as LambdaApplication).Argument = call.Argument.Clone(clone);
				break;
			default:
				clone = null;
				break;
		}
		
		return clone;
	}

	public static LambdaExpression TRUE() =>
		new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = "x" },
			Body = new LambdaAbstraction
			{
				CapturedVariable = new LambdaVariable { Name = "y" },
				Body = new LambdaVariable { Name = "x" }
			}
		};

	public static LambdaExpression FALSE() =>
		new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = "x" },
			Body = new LambdaAbstraction
			{
				CapturedVariable = new LambdaVariable { Name = "y" },
				Body = new LambdaVariable { Name = "y" }
			}
		};

	public static LambdaExpression IF() =>
		new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = "p" },
			Body = new LambdaAbstraction
			{
				CapturedVariable = new LambdaVariable { Name = "a" },
				Body = new LambdaAbstraction
				{
					CapturedVariable = new LambdaVariable { Name = "b" },
					Body = new LambdaApplication
					{
						Function = new LambdaApplication
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
		new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = "p" },
			Body = new LambdaApplication
			{
				Function = new LambdaApplication
				{
					Function = new LambdaVariable { Name = "p" },
					Argument = FALSE()
				},
				Argument = TRUE()
			}
		};

	public static LambdaExpression AND() =>
		new LambdaAbstraction
		{
			CapturedVariable = new LambdaVariable { Name = "p" },
			Body = new LambdaAbstraction
			{
				CapturedVariable = new LambdaVariable { Name = "q" },
				Body = new LambdaApplication
				{
					Function = new LambdaApplication
					{
						Function = new LambdaVariable { Name = "p" },
						Argument = new LambdaVariable { Name = "q" },
					},
					Argument = FALSE()
				}
			}
		};

	public static LambdaExpression Parse(string input, int index = 0)
	{
		while (true)
		{
			input = input.Trim();

			if (string.IsNullOrWhiteSpace(input))
			{
				throw new InvalidTermException("Invalid lambda expression: empty string.", index);
			}

			bool startsWithParenthesis = input.StartsWith('(');
			bool endsWithParenthesis = input.EndsWith(')');

			#region Variable

			if (_variableRegex.IsMatch(input))
			{
				return new LambdaVariable { Name = input };
			}

			#endregion

			#region Parenthesis

			// Check if outer parentheses are matched and therefore redundant.
			if (startsWithParenthesis)
			{
				int parenthesesDepth = 1;

				for (int i = 1; i < input.Length; i++)
				{
					char c = input[i];

					switch (c)
					{
						case '(':
							parenthesesDepth++;
							break;
						case ')':
							parenthesesDepth--;
							break;
					}

					if (parenthesesDepth == 0)
					{
						if (i == input.Length - 1)
						{
							return Parse(input[1..^1], index + 1);
						}

						break;
					}

					if (parenthesesDepth < 0)
					{
						throw new InvalidTermException("Mismatched parenthesis", index + i);
					}
				}

				if (parenthesesDepth != 0)
				{
					throw new InvalidTermException("Mismatched parenthesis", index);
				}
			}

			#endregion

			#region Abstraction

			if (input.StartsWith('λ') || input.StartsWith('\\'))
			{
				int dotIndex = input.IndexOf('.');

				if (dotIndex == -1)
				{
					throw new InvalidTermException("Invalid lambda expression: missing dot after variable.", index);
				}

				string variable = input[1..dotIndex].Trim();

				if (variable.Length == 0)
				{
					throw new InvalidTermException("Invalid lambda expression: missing variable.", index);
				}

				if (!_variableRegex.IsMatch(variable))
				{
					throw new InvalidTermException(
						"Invalid lambda expression: variable contains invalid characters. Allowed characters are a-z, A-Z, 0-9, _, and -.",
						index + 1);
				}

				string body = input[(dotIndex + 1)..];

				if (string.IsNullOrWhiteSpace(body))
				{
					throw new InvalidTermException("Invalid lambda expression: missing body.", index + dotIndex + 1);
				}

				return new LambdaAbstraction
				{
					CapturedVariable = new LambdaVariable { Name = variable }, Body = Parse(body, index + dotIndex + 1)
				};
			}

			#endregion

			#region Application

			int argumentEndIndex = input.Length;
			int argumentStartIndex = 0;
			bool argumentInParentheses = false;

			int depth = 0;

			// Try to find lambda expression outside parentheses.
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];

				switch (c)
				{
					case '(':
						depth++;
						break;
					case ')':
						depth--;
						break;
				}

				if (depth == 0 && c is '\\' or 'λ')
				{
					argumentStartIndex = i;
					goto argumentFound;
				}
			}

			depth = 0;

			if (endsWithParenthesis)
			{
				argumentInParentheses = true;
				depth++;
				argumentEndIndex--;

				for (int i = input.Length - 2; i >= 0; i--)
				{
					char c = input[i];

					switch (c)
					{
						case ')':
							depth++;
							break;
						case '(':
							depth--;
							break;
					}

					if (depth == 0)
					{
						argumentStartIndex = i + 1;
						break;
					}

					if (depth < 0)
					{
						throw new InvalidTermException("Mismatched parentheses.", index + (input.Length - i));
					}

					if (i == 0)
					{
						throw new InvalidTermException("Mismatched parentheses.", index + input.Length);
					}
				}
			}
			else
			{
				for (int j = input.Length - 1; j >= 0; j--)
				{
					char c = input[j];

					if (c is ' ' or ')')
					{
						argumentStartIndex = j + 1;
						break;
					}

					if (j == 0)
					{
						throw new InvalidTermException(
							"Invalid lambda expression: missing space or parenthesis before application argument.",
							index);
					}
				}
			}

			argumentFound:
			string argument = input[argumentStartIndex..argumentEndIndex];
			string function = input[0..(argumentStartIndex - (argumentInParentheses ? 1 : 0))];

			return new LambdaApplication
				{ Function = Parse(function, index), Argument = Parse(argument, index + argumentStartIndex) };

			#endregion
		}
	}

	[GeneratedRegex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled)]
	private static partial Regex variableRegexGenerator();

	private static readonly Regex _variableRegex = variableRegexGenerator();
}