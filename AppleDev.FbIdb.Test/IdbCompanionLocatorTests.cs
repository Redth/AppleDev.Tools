using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

public class IdbCompanionLocatorTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public IdbCompanionLocatorTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void Constructor_WithNullOptions_DoesNotThrow()
	{
		// Arrange & Act
		var locator = new IdbCompanionLocator(null, null);

		// Assert
		Assert.NotNull(locator);
	}

	[Fact]
	public void Constructor_WithOptions_DoesNotThrow()
	{
		// Arrange
		var options = new IdbCompanionOptions
		{
			CompanionPath = "/some/path/idb_companion"
		};

		// Act
		var locator = new IdbCompanionLocator(options);

		// Assert
		Assert.NotNull(locator);
	}

	[SkippableFact]
	public void Locate_ThrowsOnNonMacOS()
	{
		Skip.If(OperatingSystem.IsMacOS(), "This test is for non-macOS platforms");

		var locator = new IdbCompanionLocator();
		Assert.Throws<PlatformNotSupportedException>(() => locator.Locate());
	}

	[SkippableFact]
	public void Locate_WithInvalidCustomPath_ThrowsFileNotFoundException()
	{
		Skip.IfNot(OperatingSystem.IsMacOS(), "IDB only works on macOS");

		var options = new IdbCompanionOptions
		{
			CompanionPath = "/nonexistent/path/idb_companion"
		};

		var locator = new IdbCompanionLocator(options);
		Assert.Throws<FileNotFoundException>(() => locator.Locate());
	}

	[SkippableFact]
	public void Locate_WithEnvironmentVariable_ThrowsIfPathInvalid()
	{
		Skip.IfNot(OperatingSystem.IsMacOS(), "IDB only works on macOS");

		// Set environment variable to invalid path
		var originalValue = Environment.GetEnvironmentVariable(IdbCompanionOptions.CompanionPathEnvironmentVariable);
		try
		{
			Environment.SetEnvironmentVariable(IdbCompanionOptions.CompanionPathEnvironmentVariable, "/nonexistent/env/path");

			var locator = new IdbCompanionLocator();
			Assert.Throws<FileNotFoundException>(() => locator.Locate());
		}
		finally
		{
			// Restore original value
			Environment.SetEnvironmentVariable(IdbCompanionOptions.CompanionPathEnvironmentVariable, originalValue);
		}
	}

	[SkippableFact]
	public void CanLocate_ReturnsFalseWhenNotFound()
	{
		Skip.IfNot(OperatingSystem.IsMacOS(), "IDB only works on macOS");

		var options = new IdbCompanionOptions
		{
			CompanionPath = "/nonexistent/path/idb_companion"
		};

		var locator = new IdbCompanionLocator(options);
		Assert.False(locator.CanLocate());
	}

	[SkippableFact]
	public void CanLocate_ReturnsTrueWhenFoundViaBrew()
	{
		Skip.IfNot(OperatingSystem.IsMacOS(), "IDB only works on macOS");

		// Check if idb_companion is installed via Homebrew
		var brewPath = "/opt/homebrew/bin/idb_companion";
		var usrLocalPath = "/usr/local/bin/idb_companion";

		Skip.IfNot(File.Exists(brewPath) || File.Exists(usrLocalPath), 
			"idb_companion not installed via Homebrew");

		var locator = new IdbCompanionLocator();
		Assert.True(locator.CanLocate());

		var path = locator.Locate();
		_testOutputHelper.WriteLine($"Found idb_companion at: {path}");
		Assert.True(File.Exists(path));
	}
}
