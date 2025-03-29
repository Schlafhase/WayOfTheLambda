namespace LambdaCalculus;

public class InvalidTermException(string message, int index) : Exception(message)
{
	public int Index { get; } = index;
}