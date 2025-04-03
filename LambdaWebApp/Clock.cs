namespace LambdaWebApp;

public static class Clock
{
	private static Timer? _timer = new(_ => OnTick?.Invoke(), null, 0, 100);
	public static event Action? OnTick;
}