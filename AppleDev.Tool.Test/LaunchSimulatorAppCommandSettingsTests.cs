using AppleDev.Tool.Commands;
using Xunit;

namespace AppleDev.Tool.Test;

public class LaunchSimulatorAppCommandSettingsTests
{
	[Fact]
	public void ParseEnvironmentVariables_WithNull_ReturnsNull()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = null,
		};

		var result = settings.ParseEnvironmentVariables();

		Assert.Null(result);
	}

	[Fact]
	public void ParseEnvironmentVariables_WithEmpty_ReturnsNull()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = Array.Empty<string>(),
		};

		var result = settings.ParseEnvironmentVariables();

		Assert.Null(result);
	}

	[Fact]
	public void ParseEnvironmentVariables_WithValidPairs_ReturnsDictionary()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = new[] { "KEY1=value1", "KEY2=value2" },
		};

		var result = settings.ParseEnvironmentVariables();

		Assert.NotNull(result);
		Assert.Equal(2, result.Count);
		Assert.Equal("value1", result["KEY1"]);
		Assert.Equal("value2", result["KEY2"]);
	}

	[Fact]
	public void ParseEnvironmentVariables_WithEqualsInValue_PreservesFullValue()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = new[] { "CONNECTION=host=localhost;port=5432" },
		};

		var result = settings.ParseEnvironmentVariables();

		Assert.NotNull(result);
		Assert.Single(result);
		Assert.Equal("host=localhost;port=5432", result["CONNECTION"]);
	}

	[Fact]
	public void ParseEnvironmentVariables_WithInvalidFormat_Throws()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = new[] { "NOEQUALS" },
		};

		Assert.Throws<InvalidOperationException>(() => settings.ParseEnvironmentVariables());
	}

	[Fact]
	public void Validate_WithValidEnvVars_ReturnsSuccess()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = new[] { "KEY=value" },
		};

		var result = settings.Validate();

		Assert.True(result.Successful);
	}

	[Fact]
	public void Validate_WithInvalidEnvVars_ReturnsError()
	{
		var settings = new LaunchSimulatorAppCommandSettings
		{
			Target = "booted",
			BundleId = "com.example.app",
			EnvironmentVariables = new[] { "NOEQUALS" },
		};

		var result = settings.Validate();

		Assert.False(result.Successful);
	}
}
