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
		try
		{
			// Shutdown first (ignore failures)
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
	public async Task CreateWithUdidAsync_ReturnsValidUdid()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		var udid = await _simCtl.CreateWithUdidAsync(_testSimName, iPhoneType.Identifier!);
		_createdUdid = udid;

		Assert.NotNull(udid);
		Assert.NotEmpty(udid);
		_testOutputHelper.WriteLine($"Created simulator '{_testSimName}' with UDID: {udid}");

		// Verify it's a valid UUID format
		Assert.True(Guid.TryParse(udid, out _), $"UDID '{udid}' is not a valid UUID");

		// Verify the simulator exists
		var sims = await _simCtl.GetSimulatorsAsync(availableOnly: false);
		var created = sims.FirstOrDefault(s => s.Udid == udid);
		Assert.NotNull(created);
		Assert.Equal(_testSimName, created.Name);
	}

	[Fact]
	public async Task CreateWithUdidAsync_CanBootAndWait()
	{
		var deviceTypes = await _simCtl.GetSimulatorGroupsAsync();
		var iPhoneType = deviceTypes.FirstOrDefault(dt => dt.ProductFamily?.Contains("iPhone") == true);
		Assert.NotNull(iPhoneType);

		var udid = await _simCtl.CreateWithUdidAsync(_testSimName, iPhoneType.Identifier!);
		_createdUdid = udid;
		Assert.NotNull(udid);

		// Boot
		var bootSuccess = await _simCtl.BootAsync(udid);
		Assert.True(bootSuccess, "Failed to boot simulator");

		// Wait for ready (uses simctl bootstatus internally)
		var waitSuccess = await _simCtl.WaitForBootedAsync(udid, TimeSpan.FromSeconds(120));
		Assert.True(waitSuccess, "Failed to wait for simulator to boot");

		// Verify booted
		var sims = await _simCtl.GetSimulatorsAsync();
		var booted = sims.FirstOrDefault(s => s.Udid == udid);
		Assert.NotNull(booted);
		Assert.True(booted.IsBooted);
	}

	[Fact]
	public async Task DeleteNonExistentSimulator_ReturnsFalse()
	{
		var success = await _simCtl.DeleteAsync("nonexistent-simulator-" + Guid.NewGuid().ToString("N"));
		Assert.False(success);
	}
}
