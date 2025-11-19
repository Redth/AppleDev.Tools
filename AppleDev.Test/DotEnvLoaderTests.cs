using System;
using Xunit;

namespace AppleDev.Test;

public class DotEnvLoaderTests
{
	[Fact]
	public void Load_CanBeCalledMultipleTimes()
	{
		// Should not throw even if called multiple times
		DotEnvLoader.Load();
		DotEnvLoader.Load();
		DotEnvLoader.Load();
		
		// Test passes if no exception is thrown
		Assert.True(true);
	}

	[Fact]
	public void Load_DoesNotThrowIfEnvFileDoesNotExist()
	{
		// Should silently handle missing .env file
		DotEnvLoader.Load();
		
		// Test passes if no exception is thrown
		Assert.True(true);
	}
}

