using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

/// <summary>
/// XUnit logger adapter for ILogger.
/// </summary>
public class XUnitLogger<T> : ILogger<T>
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly string _categoryName;

	public XUnitLogger(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_categoryName = typeof(T).Name;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		try
		{
			_testOutputHelper.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, exception)}");
			if (exception != null)
			{
				_testOutputHelper.WriteLine(exception.ToString());
			}
		}
		catch
		{
			// Ignore write failures (test may have ended)
		}
	}
}
