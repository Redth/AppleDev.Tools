using Microsoft.Extensions.Logging;

namespace AppleDev.FbIdb;

/// <summary>
/// Adapter that wraps an ILogger and changes its category type.
/// </summary>
internal class LoggerAdapter<T> : ILogger<T>
{
	private readonly ILogger _innerLogger;

	public LoggerAdapter(ILogger innerLogger)
	{
		_innerLogger = innerLogger;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> _innerLogger.BeginScope(state);

	public bool IsEnabled(LogLevel logLevel)
		=> _innerLogger.IsEnabled(logLevel);

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		=> _innerLogger.Log(logLevel, eventId, state, exception, formatter);
}
