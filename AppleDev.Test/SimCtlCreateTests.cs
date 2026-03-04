using Xunit.Abstractions;

namespace AppleDev.Test;

[Collection("SimCtl")]
public class SimCtlCreateTests : IAsyncLifetime
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimCtl _simCtl;
	private readonly string _testSimName;

	public SimCtlCreateTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_simCtl = new SimCtl(new XUnitLogger<SimCtl>(testOutputHelper));
		_testSimName = $"Test-Create-{DateTime.Now:yyyyMMdd-HHmmss}";
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		try
		{
			await _simCtl.ShutdownAsync(_testSimName);
		}
		catch { }

		try
		{
			await _simCtl.DeleteAsync(_testSimName);
		}
		catch (Exception ex)
		{
			_testOutputHelper.WriteLine($"Cleanup failed: {ex.Message}");
		}
	}

	[Fact]
	public async Task CreateAsync_CreatesSimulator()
	{
		// Verify the simulator does not exist before creating
		var existingBefore = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		Assert.DoesNotContain(existingBefore, s => string.Equals(s.Name, _testSimName, StringComparison.Ordinal));

		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		var success = await _simCtl.CreateAsync(_testSimName, iPhoneType.Identifier!);
		Assert.True(success, "CreateAsync should return true");

		// Find the created simulator by name
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var device = sims.FirstOrDefault(s => string.Equals(s.Name, _testSimName, StringComparison.Ordinal));

		Assert.NotNull(device);
		Assert.Equal(_testSimName, device.Name);
		Assert.NotNull(device.Udid);
		Assert.NotEmpty(device.Udid);
		Assert.True(Guid.TryParse(device.Udid, out _), $"UDID '{device.Udid}' is not a valid UUID");
		Assert.NotNull(device.DeviceTypeIdentifier);
		Assert.NotNull(device.State);
		_testOutputHelper.WriteLine($"Created simulator: Name={device.Name}, UDID={device.Udid}, State={device.State}");
	}

	[Fact]
	public async Task CreateAndBoot_FullLifecycle()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		// Create
		var success = await _simCtl.CreateAsync(_testSimName, iPhoneType.Identifier!);
		Assert.True(success, "CreateAsync should return true");

		// Find by name
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var device = sims.FirstOrDefault(s => string.Equals(s.Name, _testSimName, StringComparison.Ordinal));
		Assert.NotNull(device);

		// Boot
		var bootSuccess = await _simCtl.BootAsync(device.Udid!);
		Assert.True(bootSuccess, "Failed to boot simulator");

		// Wait for ready
		var waitSuccess = await _simCtl.WaitForBootedAsync(device.Udid!, TimeSpan.FromSeconds(300));
		Assert.True(waitSuccess, "Failed to wait for simulator to boot");

		// Verify booted
		sims = await _simCtl.GetSimulatorsAsync();
		var booted = sims.FirstOrDefault(s => s.Udid == device.Udid);
		Assert.NotNull(booted);
		Assert.True(booted.IsBooted);
		_testOutputHelper.WriteLine($"Booted simulator: Name={booted.Name}, State={booted.State}");
	}

	[Fact]
	public async Task DeleteNonExistentSimulator_ReturnsFalse()
	{
		var success = await _simCtl.DeleteAsync("nonexistent-simulator-" + Guid.NewGuid().ToString("N"));
		Assert.False(success);
	}
}
