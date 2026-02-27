using AppleDev;
using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

/// <summary>
/// Fixture that manages a simulator for integration tests.
/// </summary>
public class SimulatorFixture : IAsyncLifetime
{
	public string? SimulatorUdid { get; private set; }
	public bool IsReady { get; private set; }
	public string? SkipReason { get; private set; }

	private SimCtl? _simCtl;
	private bool _wasAlreadyBooted;

	public async Task InitializeAsync()
	{
		if (!OperatingSystem.IsMacOS())
		{
			SkipReason = "IDB only works on macOS";
			return;
		}

		var locator = new IdbCompanionLocator();
		if (!locator.CanLocate())
		{
			SkipReason = "idb_companion not installed";
			return;
		}

		_simCtl = new SimCtl();
		var simulators = await _simCtl.GetSimulatorsAsync();

		// First check for already booted simulator
		var bootedSim = simulators.FirstOrDefault(s => s.State == "Booted");
		if (bootedSim != null)
		{
			SimulatorUdid = bootedSim.Udid;
			_wasAlreadyBooted = true;
			IsReady = true;
			return;
		}

		// Find an available iPhone simulator to boot
		var availableSim = simulators
			.Where(s => s.IsAvailable)
			.Where(s => s.DeviceType?.ProductFamily?.Contains("iPhone") == true)
			.OrderByDescending(s => s.Runtime?.Version ?? string.Empty) // Prefer newer iOS versions
			.FirstOrDefault();

		if (availableSim == null)
		{
			SkipReason = "No available iPhone simulator found";
			return;
		}

		SimulatorUdid = availableSim.Udid;

		try
		{
			// Boot the simulator
			await _simCtl.BootAsync(SimulatorUdid!);
			
			// Wait for it to be fully booted
			var booted = await _simCtl.WaitForBootedAsync(SimulatorUdid!, TimeSpan.FromSeconds(300));
			if (!booted)
			{
				SkipReason = "Simulator failed to boot within timeout";
				return;
			}

			_wasAlreadyBooted = false; // We booted it, so we should shut it down
			IsReady = true;
		}
		catch (Exception ex)
		{
			SkipReason = $"Failed to boot simulator: {ex.Message}";
		}
	}

	public async Task DisposeAsync()
	{
		// Only shutdown if we booted the simulator ourselves
		if (_simCtl != null && SimulatorUdid != null && !_wasAlreadyBooted)
		{
			try
			{
				await _simCtl.ShutdownAsync(SimulatorUdid);
			}
			catch
			{
				// Ignore shutdown errors
			}
		}
	}
}

/// <summary>
/// Integration tests for IdbClient that require a real simulator.
/// These tests require macOS with Xcode and idb_companion installed.
/// </summary>
[Collection("Idb")]
public class IdbClientIntegrationTests : IClassFixture<SimulatorFixture>
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimulatorFixture _fixture;

	public IdbClientIntegrationTests(ITestOutputHelper testOutputHelper, SimulatorFixture fixture)
	{
		_testOutputHelper = testOutputHelper;
		_fixture = fixture;
	}

	private void SkipIfNotReady()
	{
		Skip.If(!_fixture.IsReady, _fixture.SkipReason ?? "Simulator not ready");
	}

	private async Task<IdbClient> CreateConnectedClientAsync()
	{
		_testOutputHelper.WriteLine($"Creating IDB client for simulator: {_fixture.SimulatorUdid}");
		
		var client = new IdbClient(
			_fixture.SimulatorUdid!, 
			new IdbCompanionOptions 
			{ 
				VerboseLogging = true,
				StartupTimeout = TimeSpan.FromSeconds(120)
			},
			new XUnitLogger<IdbClient>(_testOutputHelper));
		
		try
		{
			await client.ConnectAsync();
		}
		catch (TimeoutException ex)
		{
			await client.DisposeAsync();
			Skip.If(true, $"idb_companion failed to start: {ex.Message}");
		}

		_testOutputHelper.WriteLine($"Connected to IDB companion, IsConnected={client.IsConnected}");
		return client;
	}

	[SkippableFact]
	public async Task IdbClient_CanBeCreated()
	{
		SkipIfNotReady();

		_testOutputHelper.WriteLine($"Using simulator UDID: {_fixture.SimulatorUdid}");

		await using var client = new IdbClient(_fixture.SimulatorUdid!, logger: new XUnitLogger<IdbClient>(_testOutputHelper));
		Assert.NotNull(client);
		Assert.Equal(_fixture.SimulatorUdid, client.TargetUdid);
	}

	[SkippableFact]
	public async Task ConnectAsync_ConnectsToSimulator()
	{
		SkipIfNotReady();

		_testOutputHelper.WriteLine($"Using booted simulator: {_fixture.SimulatorUdid}");

		await using var client = await CreateConnectedClientAsync();
		
		_testOutputHelper.WriteLine($"Connected: {client.IsConnected}");
		
		Assert.True(client.IsConnected);
	}

	[SkippableFact]
	public async Task DescribeAsync_ReturnsTargetInfo()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		var description = await client.DescribeAsync();

		_testOutputHelper.WriteLine($"Target: {description.Name}");
		_testOutputHelper.WriteLine($"UDID: {description.Udid}");
		_testOutputHelper.WriteLine($"State: {description.State}");
		_testOutputHelper.WriteLine($"Type: {description.TargetType}");
		_testOutputHelper.WriteLine($"OS Version: {description.OsVersion}");
		_testOutputHelper.WriteLine($"Architecture: {description.Architecture}");

		if (description.Screen != null)
		{
			_testOutputHelper.WriteLine($"Screen: {description.Screen.Width}x{description.Screen.Height} @ {description.Screen.Density}x");
		}

		Assert.NotEmpty(description.Udid);
		Assert.NotEmpty(description.Name);
	}

	[SkippableFact]
	public async Task ListAppsAsync_ReturnsApps()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		var apps = await client.ListAppsAsync();

		_testOutputHelper.WriteLine($"Found {apps.Count} apps:");
		foreach (var app in apps.Take(10))
		{
			_testOutputHelper.WriteLine($"  {app.BundleId} ({app.Name}) - {app.InstallType}");
		}

		Assert.NotEmpty(apps);
	}

	[SkippableFact]
	public async Task ScreenshotAsync_TakesScreenshot()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		var screenshot = await client.ScreenshotAsync();

		_testOutputHelper.WriteLine($"Screenshot: {screenshot.ImageData.Length} bytes, format: {screenshot.ImageFormat}");

		Assert.NotEmpty(screenshot.ImageData);
		Assert.NotEmpty(screenshot.ImageFormat);
	}

	[SkippableFact]
	public async Task SetLocationAsync_SetsLocation()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		// Set location to San Francisco
		await client.SetLocationAsync(37.7749, -122.4194);

		_testOutputHelper.WriteLine("Location set to San Francisco (37.7749, -122.4194)");

		// If we get here without exception, the test passed
	}

	[SkippableFact]
	public async Task FocusAsync_FocusesSimulator()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		await client.FocusAsync();

		_testOutputHelper.WriteLine("Simulator focused");
	}

	[SkippableFact]
	public async Task ListCrashLogsAsync_ReturnsCrashLogs()
	{
		SkipIfNotReady();
		// idb_companion v1.1.8 crashes server-side with uncaught NSException in crash_list.
		// See: https://github.com/facebook/idb/issues/868
		// Unlike PROTOCOL_ERROR, this is a server crash that retries cannot fix.
		Skip.If(true, "idb_companion v1.1.8 crash_list is broken (facebook/idb#868)");

		await using var client = await CreateConnectedClientAsync();

		var crashLogs = await client.ListCrashLogsAsync();

		_testOutputHelper.WriteLine($"Found {crashLogs.Count} crash logs");
		foreach (var log in crashLogs.Take(5))
		{
			_testOutputHelper.WriteLine($"  {log.Name} - {log.ProcessName} ({log.CrashTime})");
		}

		// May be empty if no crashes, but should not throw
	}

	[SkippableFact]
	public async Task ListTestBundlesAsync_ReturnsTestBundles()
	{
		SkipIfNotReady();

		await using var client = await CreateConnectedClientAsync();

		var bundles = await client.ListTestBundlesAsync();

		_testOutputHelper.WriteLine($"Found {bundles.Count} test bundles");
		foreach (var bundle in bundles.Take(5))
		{
			_testOutputHelper.WriteLine($"  {bundle.Name} ({bundle.BundleId})");
		}

		// May be empty, but should not throw
	}
}

[CollectionDefinition("Idb")]
public class IdbCollection : ICollectionFixture<IdbCollectionFixture>
{
}

public class IdbCollectionFixture
{
	// Shared fixture for IDB tests
}
