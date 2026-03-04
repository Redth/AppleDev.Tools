using Xunit.Abstractions;

namespace AppleDev.Test;

[Collection("SimCtl")]
public class SimCtlCreateWithUdidTests : IAsyncLifetime
{
	private readonly ITestOutputHelper _testOutputHelper;
	private readonly SimCtl _simCtl;
	private readonly string _testSimName;
	private string? _createdUdid;

	public SimCtlCreateWithUdidTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
		_simCtl = new SimCtl(new XUnitLogger<SimCtl>(testOutputHelper));
		_testSimName = $"Test-Create-{DateTime.Now:yyyyMMdd-HHmmss}";
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync()
	{
		var udid = _createdUdid ?? _testSimName;
		try
		{
			await _simCtl.ShutdownAsync(udid);
		}
		catch { }

		try
		{
			await _simCtl.DeleteAsync(udid);
		}
		catch (Exception ex)
		{
			_testOutputHelper.WriteLine($"Cleanup failed: {ex.Message}");
		}
	}

	[Fact]
	public async Task CreateWithUdidAsync_ReturnsValidUdid()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		var udid = await _simCtl.CreateWithUdidAsync(_testSimName, iPhoneType.Identifier!);
		_createdUdid = udid;

		Assert.NotNull(udid);
		Assert.NotEmpty(udid);
		Assert.True(Guid.TryParse(udid, out _), $"UDID '{udid}' is not a valid UUID");
		_testOutputHelper.WriteLine($"Created simulator '{_testSimName}' with UDID: {udid}");
	}

	[Fact]
	public async Task GetSimulatorAsync_ReturnsDevice()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		var udid = await _simCtl.CreateWithUdidAsync(_testSimName, iPhoneType.Identifier!);
		_createdUdid = udid;
		Assert.NotNull(udid);

		var device = await _simCtl.GetSimulatorAsync(udid);

		Assert.NotNull(device);
		Assert.Equal(udid, device.Udid);
		Assert.Equal(_testSimName, device.Name);
		Assert.NotNull(device.DeviceTypeIdentifier);
		Assert.NotNull(device.State);
		_testOutputHelper.WriteLine($"GetSimulatorAsync returned: Name={device.Name}, State={device.State}, Type={device.DeviceTypeIdentifier}");
	}

	[Fact]
	public async Task GetSimulatorAsync_NonExistentUdid_ReturnsNull()
	{
		var device = await _simCtl.GetSimulatorAsync(Guid.NewGuid().ToString());
		Assert.Null(device);
	}

	[Fact]
	public async Task CreateAndBoot_FullLifecycle()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		// Create
		var udid = await _simCtl.CreateWithUdidAsync(_testSimName, iPhoneType.Identifier!);
		_createdUdid = udid;
		Assert.NotNull(udid);

		// Boot
		var bootSuccess = await _simCtl.BootAsync(udid);
		Assert.True(bootSuccess, "Failed to boot simulator");

		// Wait for ready
		var waitSuccess = await _simCtl.WaitForBootedAsync(udid, TimeSpan.FromSeconds(300));
		Assert.True(waitSuccess, "Failed to wait for simulator to boot");

		// Verify booted via GetSimulatorAsync
		var device = await _simCtl.GetSimulatorAsync(udid);
		Assert.NotNull(device);
		Assert.True(device.IsBooted);
		_testOutputHelper.WriteLine($"Booted simulator: Name={device.Name}, State={device.State}");
	}

	[Fact]
	public async Task DeleteNonExistentSimulator_ReturnsFalse()
	{
		var success = await _simCtl.DeleteAsync("nonexistent-simulator-" + Guid.NewGuid().ToString("N"));
		Assert.False(success);
	}
}
