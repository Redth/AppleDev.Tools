using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AppleDev.Test;

class XUnitLogger<T> : ILogger<T>
{
	private readonly ITestOutputHelper _testOutputHelper;

	public XUnitLogger(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	public IDisposable? BeginScope<TState>(TState state)
		where TState : notnull
	{
		throw new NotImplementedException();
	}

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		var message = formatter(state, exception);

		_testOutputHelper.WriteLine($"[{logLevel}] {message}");

		if (exception != null)
		{
			_testOutputHelper.WriteLine(exception.ToString());
		}
	}
}
