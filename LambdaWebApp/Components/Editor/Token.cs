namespace LambdaWebApp.Components.Editor;

public struct Token
{
	public string Value { get; init; }
	public TokenType Type { get; init; }
}

public enum TokenType
{
	Lambda,
	Variable,
	Parenthesis,
	Cursor,
	Newline
}