using Xunit.Abstractions;

namespace AppleDev.Test;

public class XCDeviceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly XCDevice _xcdevice;

    public XCDeviceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _xcdevice = new XCDevice(new XUnitLogger<XCDevice>(testOutputHelper));
    }

    [Fact]
    public async Task LocateXCDevice()
    {
        var path = await _xcdevice.LocateAsync();
        
        Assert.NotNull(path);
        Assert.True(path.Exists);
    }

    [Fact]
    public async Task GetAnyDevices()
    {
        var devices = await _xcdevice.GetDevicesAsync();
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public async Task GetAnyDevicesWithTimeout(int timeoutSeconds)
    {
        var ts = TimeSpan.FromSeconds(timeoutSeconds);
        var devices = await _xcdevice.GetDevicesAsync(timeout: ts);
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [Fact]
    public async Task ObserveAnyDevices()
    {
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var tcs = new TaskCompletionSource<bool>();

        var wasAtLeastListeningAtAll = false;

        ct.Token.Register(() =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(false);
        });
        
        await _xcdevice.ObserveAsync(ct.Token, XCDevice.XCDeviceType.Both, (id, added) =>
        {
            tcs.TrySetResult(true);
            if (!ct.IsCancellationRequested)
                ct.Cancel();

            return Task.CompletedTask;
        }, line =>
        {
            // We might not observe changes on a test since we aren't adding/removing devices
            // but we can tell if stdout showed that xcdevice was listening for devices
            if (line.Contains("Listening for all devices, on both interfaces."))
                wasAtLeastListeningAtAll = true;

            _testOutputHelper.WriteLine(line);
            return Task.CompletedTask;
        });

        Assert.True(await tcs.Task || wasAtLeastListeningAtAll, "No devices were attached/detached, nor was xcdevice listening for devices.");
    }
}