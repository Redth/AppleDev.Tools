using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AppleDev.Test;

public class SimCtlTests : IAsyncDisposable
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimCtl _simCtl;
	private readonly string _testSimName;

	public SimCtlTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_simCtl = new SimCtl();
		_testSimName = $"Test-iPhone-{DateTime.Now:yyyyMMdd-HHmmss}";
	}

	public async ValueTask DisposeAsync()
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
	public async Task GetAnySimulators()
	{
		var sims = await _simCtl.GetSimulatorsAsync();

		Assert.NotNull(sims);
		Assert.NotEmpty(sims);

		foreach (var sim in sims)
		{
			_testOutputHelper.WriteLine($"{sim.Name} - {sim.Runtime} - {sim.Udid}");
		}
	}

	[Fact]
	public async Task GetAnySimulatorGroups()
	{
		var simGrps = await _simCtl.GetSimulatorGroupsAsync();

		Assert.NotNull(simGrps);
		Assert.NotEmpty(simGrps);

		foreach (var sg in simGrps)
		{
			_testOutputHelper.WriteLine($"{sg.Name} - {sg.ProductFamily} - {sg.Identifier} - {sg.BundlePath}");

			Assert.NotEmpty(sg.Devices);

			foreach (var d in sg.Devices)
				_testOutputHelper.WriteLine($"{d.Name} - {d.Runtime} - {d.Udid}");
		}
	}

	[Fact]
	public async Task ControlSimulator()
	{
		var sims = await _simCtl.GetSimulatorsAsync();

		var s = sims.FirstOrDefault(s => s.DeviceType?.ProductFamily?.Contains("iPhone") ?? false);

		_testOutputHelper.WriteLine($"Found: {s?.Name} - {s?.Udid}");

		var udid = s?.Udid;

		Assert.NotNull(udid);

		// Erase the sim first
		Assert.True(await _simCtl.EraseAsync(udid));

		_testOutputHelper.WriteLine($"Erased: {udid}");


		// Open simulator.app
		//Assert.True(await _simCtl.OpenSimulatorAppAsync(udid));

		// Boot
		Assert.True(await _simCtl.BootAsync(udid));
		_testOutputHelper.WriteLine($"Booting: {udid}");

		// Wait for boot complete
		Assert.True(await _simCtl.WaitForBootedAsync(udid, TimeSpan.FromSeconds(240)));

		_testOutputHelper.WriteLine($"Waited for boot: {udid}");

		// Verify we found a sim
		var booted = (await _simCtl.GetSimulatorsAsync())
			.FirstOrDefault(i => !string.IsNullOrEmpty(i.Udid) && i.Udid == udid && i.IsBooted);

		Assert.NotNull(booted);
	}

	[Fact]
	public void PreprocessSimctlOutput_CanParseStrangeFormat()
	{
		// Arrange - Real _simCtl output format with multiple apps and complex nested structures
		var input =
		"""
		{
			"com.apple.Bridge" =     {
				ApplicationType = System;
				Bundle = "file:///Library/Developer/CoreSimulator/Volumes/iOS_22E238/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS%2018.4.simruntime/Contents/Resources/RuntimeRoot/Applications/Bridge.app/";
				CFBundleDisplayName = Watch;
				CFBundleExecutable = Bridge;
				CFBundleIdentifier = "com.apple.Bridge";
				CFBundleName = Watch;
				CFBundleVersion = "1.0";
				DataContainer = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Data/Application/8D15394A-12B2-41C0-9444-4F5E2DF46370/";
				GroupContainers =         {
					"243LU875E5.groups.com.apple.podcasts" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/8661BFAD-B327-4281-9C60-F8D35E0D3143/";
					"group.com.apple.bridge" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/E129B06E-1D39-40A9-87CD-63AB07F4AF18/";
					"group.com.apple.iBooks" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/B40E5042-AD3A-4276-B61C-BC9B650021C9/";
					"group.com.apple.mail" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/927C6AF0-96E2-40E2-8518-88470E1DEEAA/";
					"group.com.apple.stocks" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/4614AB97-A05F-4879-9596-C0D5EA245A81/";
					"group.com.apple.weather" = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Shared/AppGroup/6870B26C-5F8C-4725-8925-BC712154AF1F/";
				};
				Path = "/Library/Developer/CoreSimulator/Volumes/iOS_22E238/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS 18.4.simruntime/Contents/Resources/RuntimeRoot/Applications/Bridge.app";
				SBAppTags =         (
					"watch-companion"
				);
			};
			"com.apple.webapp" =     {
				ApplicationType = System;
				Bundle = "file:///Library/Developer/CoreSimulator/Volumes/iOS_22E238/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS%2018.4.simruntime/Contents/Resources/RuntimeRoot/Applications/Web.app/";
				CFBundleDisplayName = Web;
				CFBundleExecutable = Web;
				CFBundleIdentifier = "com.apple.webapp";
				CFBundleName = Web;
				CFBundleVersion = "8621.1.15.10.7";
				DataContainer = "file:///Users/matthew/Library/Developer/CoreSimulator/Devices/6AFAB369-C151-48FC-AB19-D664B7161D1E/data/Containers/Data/Application/87EFA537-4CB3-4CBF-91C0-ED8B9859AC13/";
				GroupContainers =         {
				};
				Path = "/Library/Developer/CoreSimulator/Volumes/iOS_22E238/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS 18.4.simruntime/Contents/Resources/RuntimeRoot/Applications/Web.app";
				SBAppTags =         (
				);
			};
		}
		""";

		// Act
		var result = SimCtl.PreprocessSimctlOutput(input);

		// Assert
		_testOutputHelper.WriteLine("Input:");
		_testOutputHelper.WriteLine(input);
		_testOutputHelper.WriteLine("\nProcessed:");
		_testOutputHelper.WriteLine(result);

		// The result should be valid JSON
		Assert.DoesNotContain("\"{\"", result); // Should not have quoted opening braces
		Assert.DoesNotContain("\"[\"", result); // Should not have quoted opening brackets
		Assert.DoesNotContain("\":\"", result); // Should not have quoted colons in already quoted strings

		// Check that specific structures are correct
		Assert.Contains("\"com.apple.Bridge\" :     {", result); // Top-level keys should be quoted
		Assert.Contains("\"ApplicationType\": \"System\"", result); // Unquoted values should be quoted
		Assert.Contains("\"CFBundleVersion\": \"1.0\"", result); // Already quoted values should remain quoted
		Assert.Contains("\"GroupContainers\":         {", result); // Nested objects should not be quoted
		Assert.Contains("\"SBAppTags\":         [", result); // Arrays should use brackets
		Assert.Contains("\"243LU875E5.groups.com.apple.podcasts\" : \"file:///", result); // Complex keys should be quoted

		// Empty structures should work
		Assert.Contains("\"GroupContainers\":         {\n		}", result); // Empty objects
		Assert.Contains("\"SBAppTags\":         [\n		]", result); // Empty arrays

		// Try to parse as JSON to ensure it's valid
		var exception = Record.Exception(() => Newtonsoft.Json.JsonConvert.DeserializeObject(result));
		Assert.Null(exception);

		// Additionally, try to deserialize to the expected structure
		var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
		Assert.NotNull(parsed);
		Assert.Contains("com.apple.Bridge", parsed.Keys);
		Assert.Contains("com.apple.webapp", parsed.Keys);
	}

	[Fact]
	public async Task CreateSimulator_ShouldSucceed()
	{
		// Create a simulator
		var iPhoneType = await GetTestingDeviceTypeAsync();
		var success = await _simCtl.CreateAsync(_testSimName, iPhoneType.Identifier!);
		Assert.True(success);

		// Verify the simulator was created
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var createdSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(createdSim);
		Assert.Equal(_testSimName, createdSim.Name);
	}

	[Fact]
	public async Task BootAndShutdownSimulator_ShouldSucceed()
	{
		// Create a temporary simulator for testing
		var iPhoneDeviceType = await GetTestingDeviceTypeAsync();
		await _simCtl.CreateAsync(_testSimName, iPhoneDeviceType.Identifier!);

		// Boot the simulator
		var bootSuccess = await _simCtl.BootAsync(_testSimName);
		Assert.True(bootSuccess, "Failed to boot the simulator");

		// Wait for boot to complete
		var waitSuccess = await _simCtl.WaitForBootedAsync(_testSimName, TimeSpan.FromSeconds(120));
		Assert.True(waitSuccess, "Failed to wait for the simulator to boot");

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
		await CreateTestSimulatorAsync();

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
        await CreateTestSimulatorAsync();

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
		var iPhoneDeviceType = await GetTestingDeviceTypeAsync();

		// 1. Create
		var createSuccess = await _simCtl.CreateAsync(_testSimName, iPhoneDeviceType.Identifier!);
		Assert.True(createSuccess);

		// Get the created simulator
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var testSim = sims.FirstOrDefault(s => s.Name == _testSimName);
		Assert.NotNull(testSim);
		Assert.NotNull(testSim.Udid);

		// 2. Boot
		var bootSuccess = await _simCtl.BootAsync(testSim.Udid);
		Assert.True(bootSuccess, "Failed to boot the simulator");

		var waitSuccess = await _simCtl.WaitForBootedAsync(testSim.Udid, TimeSpan.FromSeconds(120));
		Assert.True(waitSuccess, "Failed to wait for the simulator to boot");

		// 3. Verify booted state
		var updatedSims = await _simCtl.GetSimulatorsAsync();
		var bootedSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.NotNull(bootedSim);
		Assert.True(bootedSim.IsBooted, "Simulator is not in booted state");

		// 4. Shutdown
		var shutdownSuccess = await _simCtl.ShutdownAsync(testSim.Udid);
		Assert.True(shutdownSuccess, "Failed to shutdown the simulator");

		// Give it time to shutdown
		await Task.Delay(3000);

		// 5. Verify shutdown state
		updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var shutdownSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.NotNull(shutdownSim);
		Assert.False(shutdownSim.IsBooted, "Simulator is still in booted state after shutdown");

		// 6. Delete
		var deleteSuccess = await _simCtl.DeleteAsync(testSim.Udid);
		Assert.True(deleteSuccess, "Failed to delete the simulator");

		// 7. Verify deletion
		updatedSims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var deletedSim = updatedSims.FirstOrDefault(s => s.Udid == testSim.Udid);
		Assert.Null(deletedSim);
	}

    private async Task CreateTestSimulatorAsync()
    {
        var iPhoneDeviceType = await GetTestingDeviceTypeAsync();

        await _simCtl.CreateAsync(_testSimName, iPhoneDeviceType.Identifier!);
    }

	[Fact]
	public async Task GetAppsAsync_ShouldReturnAppsWithCorrectProperties()
	{
		// Create a temporary simulator for testing
		var iPhoneDeviceType = await GetTestingDeviceTypeAsync();
		await _simCtl.CreateAsync(_testSimName, iPhoneDeviceType.Identifier!);

		// Boot the simulator
		var bootSuccess = await _simCtl.BootAsync(_testSimName);
		Assert.True(bootSuccess, "Failed to boot the simulator");

		// Wait for boot to complete
		var waitSuccess = await _simCtl.WaitForBootedAsync(_testSimName, TimeSpan.FromSeconds(120));
		Assert.True(waitSuccess, "Failed to wait for the simulator to boot");

		// Get the installed apps
		var apps = await _simCtl.GetAppsAsync(_testSimName);
		Assert.NotNull(apps);

		// At minimum, verify we have some apps and they have basic identifiers
		Assert.True(apps.Count > 0, "Should have at least some apps");
		Assert.True(apps.All(a => !string.IsNullOrEmpty(a.CFBundleIdentifier)),  "All apps should have a CFBundleIdentifier");

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

	private async Task<SimCtlDeviceType> GetTestingDeviceTypeAsync()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();

		// Find an iPhone device type
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		return iPhoneType;
	}
}
