using System.Text.Json;
using System.Xml;
using Xunit.Abstractions;

namespace AppleDev.Test;

[Collection("SimCtl")]
public class SimCtlComplexTests : IAsyncLifetime
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimCtl _simCtl;
	private readonly string _testSimName;

	public SimCtlComplexTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_simCtl = new SimCtl(new XUnitLogger<SimCtl>(testOutputHelper));
		_testSimName = $"Test-iPhone-{DateTime.Now:yyyyMMdd-HHmmss}";
	}

	public async Task InitializeAsync()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();

		// Find an iPhone device type
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		// Create a simulator
		var success = await _simCtl.CreateAsync(_testSimName, iPhoneType.Identifier!);
		Assert.True(success);
	}

	private bool _isBooted = false;

	async Task BootAndWaitAsync()
	{
		// Skip if already booted
		if (_isBooted)
			return;

		// Boot the simulator
		var bootSuccess = await _simCtl.BootAsync(_testSimName);
		Assert.True(bootSuccess, "Failed to boot the simulator");

		// Wait for boot to complete with reduced timeout
		var waitSuccess = await _simCtl.WaitForBootedAsync(_testSimName, TimeSpan.FromSeconds(60));
		Assert.True(waitSuccess, "Failed to wait for the simulator to boot");

		_isBooted = true;
	}

	public async Task DisposeAsync()
	{
		try
		{
			await _simCtl.DeleteAsync(_testSimName);
		}
		catch (Exception ex)
		{
			_testOutputHelper.WriteLine($"Failed to clean up test simulator: {ex.Message}");
		}
	}

	[Fact]
	public async Task CreateSimulator_ShouldSucceed()
	{
		// Verify the simulator was created
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var createdSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(createdSim);
		Assert.Equal(_testSimName, createdSim.Name);
	}

	[Fact]
	public async Task BootAndShutdownSimulator_ShouldSucceed()
	{
		await BootAndWaitAsync();

		// Verify the simulator is booted
		var updatedSims = await _simCtl.GetSimulatorsAsync();
		var bootedSim = updatedSims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(bootedSim);
		Assert.True(bootedSim.IsBooted, "Simulator is not in booted state");

		// Shutdown the simulator
		var shutdownSuccess = await _simCtl.ShutdownAsync(_testSimName);
		Assert.True(shutdownSuccess, "Failed to shutdown the simulator");

		// Verify the simulator is shutdown
		var retries = 3;
		while (retries-- > 0 && bootedSim.IsBooted)
		{
			await Task.Delay(500); // Give it a moment to shutdown

			updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
			var shutdownSim = updatedSims.FirstOrDefault(s => s.Name == _testSimName);
			Assert.NotNull(shutdownSim);
			Assert.False(shutdownSim.IsBooted, "Simulator is still in booted state after shutdown");
		}
	}

	[Fact]
	public async Task DeleteSimulator_ShouldSucceed()
	{
		// Get the created simulator
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var createdSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(createdSim);
		Assert.NotNull(createdSim.Udid);

		// Delete the simulator
		var deleteSuccess = await _simCtl.DeleteAsync(_testSimName);
		Assert.True(deleteSuccess, "Failed to delete the simulator");

		// Verify the simulator was deleted
		var updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var deletedSim = updatedSims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.Null(deletedSim);
	}

	[Fact]
	public async Task EraseSimulator_ShouldSucceed()
	{
		// Get the created simulator
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var testSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(testSim);
		Assert.NotNull(testSim.Udid);

		// Erase the simulator
		var eraseSuccess = await _simCtl.EraseAsync(testSim.Udid);
		Assert.True(eraseSuccess, "Failed to erase the simulator");

		// Verify the simulator still exists but is clean
		var updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var erasedSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.NotNull(erasedSim);
	}

	[Fact]
	public async Task SimulatorLifecycle_CreateBootShutdownDelete_ShouldSucceed()
	{
		// Get the created simulator
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var testSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(testSim);
		Assert.NotNull(testSim.Udid);

		// Boot
		var bootSuccess = await _simCtl.BootAsync(testSim.Udid);
		Assert.True(bootSuccess, "Failed to boot the simulator");

		var waitSuccess = await _simCtl.WaitForBootedAsync(testSim.Udid, TimeSpan.FromSeconds(120));
		Assert.True(waitSuccess, "Failed to wait for the simulator to boot");

		// Verify booted state
		var updatedSims = await _simCtl.GetSimulatorsAsync();
		var bootedSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.NotNull(bootedSim);
		Assert.True(bootedSim.IsBooted, "Simulator is not in booted state");

		// Shutdown
		var shutdownSuccess = await _simCtl.ShutdownAsync(testSim.Udid);
		Assert.True(shutdownSuccess, "Failed to shutdown the simulator");

		// Give it time to shutdown
		await Task.Delay(3000);

		// Verify shutdown state
		updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var shutdownSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.NotNull(shutdownSim);
		Assert.False(shutdownSim.IsBooted, "Simulator is still in booted state after shutdown");

		// Delete
		var deleteSuccess = await _simCtl.DeleteAsync(testSim.Udid);
		Assert.True(deleteSuccess, "Failed to delete the simulator");

		// Verify deletion
		updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var deletedSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.Null(deletedSim);
	}

	[Fact]
	public async Task GetAppsAsync_ShouldReturnAppsWithCorrectProperties()
	{
		await BootAndWaitAsync();

		// Get the installed apps
		var apps = await _simCtl.GetAppsAsync(_testSimName);
		Assert.NotNull(apps);

		// At minimum, verify we have some apps and they have basic identifiers
		Assert.True(apps.Count > 0, "Should have at least some apps");
		Assert.True(apps.All(a => !string.IsNullOrEmpty(a.CFBundleIdentifier)), "All apps should have a CFBundleIdentifier");

		// Specifically look for the com.apple.Bridge app to validate all properties
		var bridgeApp = apps.FirstOrDefault(a => a.CFBundleIdentifier == "com.apple.Bridge");
		Assert.NotNull(bridgeApp);

		// Validate all required properties for com.apple.Bridge
		Assert.Equal("System", bridgeApp.ApplicationType);
		Assert.Equal("com.apple.Bridge", bridgeApp.CFBundleIdentifier);
		Assert.Equal("Watch", bridgeApp.CFBundleDisplayName);
		Assert.Equal("Bridge", bridgeApp.CFBundleExecutable);
		Assert.Equal("Watch", bridgeApp.CFBundleName);
		Assert.Equal("1.0", bridgeApp.CFBundleVersion);

		// Validate path structure
		Assert.NotNull(bridgeApp.Bundle);
		Assert.Contains("Bridge.app", bridgeApp.Bundle);
		Assert.StartsWith("file://", bridgeApp.Bundle);

		Assert.NotNull(bridgeApp.Path);
		Assert.Contains("Bridge.app", bridgeApp.Path);

		// Validate DataContainer
		Assert.NotNull(bridgeApp.DataContainer);
		Assert.StartsWith("file://", bridgeApp.DataContainer);
		Assert.Contains("Containers/Data/Application", bridgeApp.DataContainer);

		// Validate GroupContainers - should have specific groups
		Assert.NotNull(bridgeApp.GroupContainers);
		Assert.True(bridgeApp.GroupContainers.Count > 0, "Bridge app should have group containers");

		// Check for expected group containers
		var expectedGroups = new[]
		{
			"243LU875E5.groups.com.apple.podcasts",
			"group.com.apple.bridge",
			"group.com.apple.iBooks",
			"group.com.apple.mail",
			"group.com.apple.stocks",
			"group.com.apple.weather"
		};

		foreach (var expectedGroup in expectedGroups)
		{
			Assert.Contains(expectedGroup, bridgeApp.GroupContainers.Keys);
			Assert.NotNull(bridgeApp.GroupContainers[expectedGroup]);
			Assert.StartsWith("file://", bridgeApp.GroupContainers[expectedGroup]);
			Assert.Contains("Containers/Shared/AppGroup", bridgeApp.GroupContainers[expectedGroup]);
		}

		// Validate SBAppTags - should contain "watch-companion"
		Assert.NotNull(bridgeApp.SBAppTags);
		Assert.Contains("watch-companion", bridgeApp.SBAppTags);
	}

	[Fact]
	public async Task GetLogsAsync_ShouldReturnLogOutput()
	{
		await BootAndWaitAsync();

		// Get logs - should return some log data
		var logs = await _simCtl.GetLogsAsync(_testSimName);
		Assert.NotNull(logs);
		Assert.NotEmpty(logs);
	}

	[Fact]
	public async Task GetLogsAsync_WithStart_ShouldReturnLogOutput()
	{
		await BootAndWaitAsync();

		// Get logs - should return some log data
		var logs = await _simCtl.GetLogsAsync(_testSimName, start: DateTimeOffset.Now.AddSeconds(-5));
		Assert.NotNull(logs);
		Assert.NotEmpty(logs);
	}

	[Fact]
	public async Task GetLogsAsync_WithPredicate_ShouldFilterLogs()
	{
		await BootAndWaitAsync();

		// Reduced delay - simulator boot already generated plenty of logs
		await Task.Delay(1000);

		// Get logs with a predicate filter
		var logs = await _simCtl.GetLogsAsync(_testSimName, predicate: "senderImagePath contains 'Maps'");
		Assert.NotNull(logs);
		Assert.NotEmpty(logs);

		try
		{
			Assert.Contains(logs, log => log.Subsystem == "com.apple.Maps");
			Assert.DoesNotContain(logs, log => log.Subsystem == "ClockKit");
		}
		catch
		{
			_testOutputHelper.WriteLine($"Logs for {_testSimName}:");
			foreach (var log in logs)
			{
				_testOutputHelper.WriteLine(JsonSerializer.Serialize(log));
			}
			throw;
		}
	}

	[Fact]
	public async Task GetLogsPlainAsync_WithPredicate_ShouldFilterLogs()
	{
		await BootAndWaitAsync();

		// Reduced delay - simulator boot already generated plenty of logs
		await Task.Delay(1000);

		// Get logs with a predicate filter
		var logs = await _simCtl.GetLogsPlainAsync(_testSimName, predicate: "senderImagePath contains 'Maps'");
		Assert.NotNull(logs);
		Assert.NotEmpty(logs);

		try
		{
			Assert.Contains(logs, log => log.Contains("[com.apple.Maps:GeneralMapsWidget]"));
			Assert.DoesNotContain(logs, log => log.Contains("Clock"));

		}
		catch
		{
			_testOutputHelper.WriteLine($"Logs for {_testSimName}:");
			foreach (var log in logs)
			{
				_testOutputHelper.WriteLine(log);
			}
			throw;
		}
	}

	[Fact]
	public async Task CollectLogsAsync_ShouldSucceed()
	{
		await BootAndWaitAsync();

		// Create a unique output path for the test
		var outputPath = Path.Combine(Path.GetTempPath(), $"test-logs-{DateTime.Now:yyyyMMdd-HHmmss}.logarchive");

		try
		{
			// Collect logs
			var success = await _simCtl.CollectLogsAsync(_testSimName, outputPath: outputPath, start: DateTimeOffset.Now.AddSeconds(-5));
			Assert.True(success, "Failed to collect logs");

			// Verify the log archive was created
			Assert.True(Directory.Exists(outputPath), "Log archive should exist");
			var plist = Assert.Single(Directory.GetFiles(outputPath, "Info.plist"));
			new XmlDocument().Load(plist);
		}
		finally
		{
			// Clean up the log archive
			if (Directory.Exists(outputPath))
			{
				try
				{
					Directory.Delete(outputPath, true);
				}
				catch (Exception ex)
				{
					_testOutputHelper.WriteLine($"Failed to clean up log archive: {ex.Message}");
				}
			}
		}
	}
}
