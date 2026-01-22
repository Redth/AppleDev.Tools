using Xunit.Abstractions;

namespace AppleDev.Test;

[Collection("SimCtl")]
public class SimCtlTests
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimCtl _simCtl;

	public SimCtlTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_simCtl = new SimCtl(new XUnitLogger<SimCtl>(testOutputHelper));
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
		await _simCtl.EraseAsync(udid);

		_testOutputHelper.WriteLine($"Erased: {udid}");


		// Open simulator.app
		//Assert.True(await _simCtl.OpenSimulatorAppAsync(udid));

		// Boot
		await _simCtl.BootAsync(udid);
		_testOutputHelper.WriteLine($"Booting: {udid}");

		// Wait for boot complete
		Assert.True(await _simCtl.WaitForBootedAsync(udid, TimeSpan.FromSeconds(240)));

		_testOutputHelper.WriteLine($"Waited for boot: {udid}");

		// Verify we found a sim
		var booted = (await _simCtl.GetSimulatorsAsync())
			.FirstOrDefault(i => !string.IsNullOrEmpty(i.Udid) && i.Udid == udid && i.IsBooted);

		Assert.NotNull(booted);

		await _simCtl.ShutdownAsync(udid);

		_testOutputHelper.WriteLine($"Shutdown: {udid}");
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
		var exception = Record.Exception(() => System.Text.Json.JsonSerializer.Deserialize<object>(result));
		Assert.Null(exception);

		// Additionally, try to deserialize to the expected structure
		var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(result);
		Assert.NotNull(parsed);
		Assert.Contains("com.apple.Bridge", parsed.Keys);
		Assert.Contains("com.apple.webapp", parsed.Keys);
	}

	[Fact]
	public async Task GetDeviceTypeScreenInfo()
	{
		var deviceTypes = await _simCtl.GetSimulatorDeviceTypes(includeScreenInfo: true);

		Assert.NotNull(deviceTypes);
		Assert.NotEmpty(deviceTypes);

		var iPhone = deviceTypes.FirstOrDefault(dt => dt.Name?.Contains("iPhone") == true);
		Assert.NotNull(iPhone, "iPhone device not found");
		Assert.NotNull(iPhone.Screen, "Screen info not populated for iPhone");
		Assert.True(iPhone.Screen.Width > 0);
		Assert.True(iPhone.Screen.Height > 0);
		Assert.True(iPhone.Screen.Scale > 0);
		Assert.True(iPhone.Screen.WidthDPI > 0);
		Assert.True(iPhone.Screen.HeightDPI > 0);
		Assert.NotNull(iPhone.ModelIdentifier, "ModelIdentifier not set");
		Assert.NotNull(iPhone.ProductClass, "ProductClass not set");
	}

	[Fact]
	public async Task GetSimulatorsWithScreenInfo()
	{
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: true, includeScreenInfo: true);

		Assert.NotNull(sims);
		Assert.NotEmpty(sims);

		var simWithScreen = sims.FirstOrDefault(s => s.DeviceType?.Screen != null);
		Assert.NotNull(simWithScreen);

		var screen = simWithScreen.DeviceType!.Screen;
		Assert.NotNull(screen);
		Assert.True(screen.PixelWidth > 0);
		Assert.True(screen.PixelHeight > 0);
	}
}
