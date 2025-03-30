namespace LambdaWebApp;

public static class Clock
{
	public static event Action? OnTick;
	private static Timer? _timer = new Timer(_ => OnTick?.Invoke(), null, 0, 100);
}