using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

public class IdbCompanionProcessTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public IdbCompanionProcessTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void Constructor_WithNullOptions_DoesNotThrow()
	{
		using var process = new IdbCompanionProcess(null, null);
		Assert.NotNull(process);
	}

	[Fact]
	public void Constructor_WithOptions_DoesNotThrow()
	{
		var options = new IdbCompanionOptions
		{
			VerboseLogging = true
		};

		using var process = new IdbCompanionProcess(options);
		Assert.NotNull(process);
	}

	[Fact]
	public void InitialState_IsCorrect()
	{
		using var process = new IdbCompanionProcess();

		Assert.False(process.IsRunning);
		Assert.Null(process.GrpcPort);
		Assert.Null(process.TargetUdid);
		Assert.Null(process.GrpcAddress);
	}

	[SkippableFact]
	public async Task StartAsync_ThrowsOnNonMacOS()
	{
		Skip.If(OperatingSystem.IsMacOS(), "This test is for non-macOS platforms");

		using var process = new IdbCompanionProcess();
		await Assert.ThrowsAsync<PlatformNotSupportedException>(() => process.StartAsync("test-udid"));
	}

	[Fact]
	public async Task Dispose_DoesNotThrow()
	{
		var process = new IdbCompanionProcess();
		process.Dispose();

		// Should not throw
		await Task.CompletedTask;
	}

	[Fact]
	public async Task DisposeAsync_DoesNotThrow()
	{
		var process = new IdbCompanionProcess();
		await process.DisposeAsync();

		// Should not throw
	}

	[Fact]
	public async Task StopAsync_WhenNotRunning_DoesNotThrow()
	{
		using var process = new IdbCompanionProcess();
		await process.StopAsync();

		// Should not throw
	}
}
