using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

public class IdbCompanionOptionsTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public IdbCompanionOptionsTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void DefaultValues_AreCorrect()
	{
		var options = new IdbCompanionOptions();

		Assert.Null(options.CompanionPath);
		Assert.Equal(0, options.GrpcPort);
		Assert.Equal(TimeSpan.FromSeconds(30), options.StartupTimeout);
		Assert.Equal(TimeSpan.FromSeconds(10), options.ShutdownTimeout);
		Assert.Equal(TimeSpan.FromSeconds(60), options.OperationTimeout);
		Assert.False(options.VerboseLogging);
		Assert.Null(options.TargetUdid);
	}

	[Fact]
	public void CompanionPathEnvironmentVariable_IsCorrect()
	{
		Assert.Equal("IDB_COMPANION_PATH", IdbCompanionOptions.CompanionPathEnvironmentVariable);
	}

	[Fact]
	public void Properties_CanBeSet()
	{
		var options = new IdbCompanionOptions
		{
			CompanionPath = "/custom/path",
			GrpcPort = 12345,
			StartupTimeout = TimeSpan.FromSeconds(60),
			ShutdownTimeout = TimeSpan.FromSeconds(20),
			OperationTimeout = TimeSpan.FromSeconds(120),
			VerboseLogging = true,
			TargetUdid = "test-udid"
		};

		Assert.Equal("/custom/path", options.CompanionPath);
		Assert.Equal(12345, options.GrpcPort);
		Assert.Equal(TimeSpan.FromSeconds(60), options.StartupTimeout);
		Assert.Equal(TimeSpan.FromSeconds(20), options.ShutdownTimeout);
		Assert.Equal(TimeSpan.FromSeconds(120), options.OperationTimeout);
		Assert.True(options.VerboseLogging);
		Assert.Equal("test-udid", options.TargetUdid);
	}
}
