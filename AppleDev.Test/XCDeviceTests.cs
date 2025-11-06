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

    [SkippableFact]
    public async Task LocateXCDevice()
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
        var path = await _xcdevice.LocateAsync().ConfigureAwait(false);
        
        Assert.NotNull(path);
        Assert.True(path.Exists);
    }

    [SkippableFact]
    public async Task GetAnyDevices()
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
        var devices = await _xcdevice.GetDevicesAsync().ConfigureAwait(false);
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [SkippableTheory]
    [InlineData(2)]
    [InlineData(5)]
    public async Task GetAnyDevicesWithTimeout(int timeoutSeconds)
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
        var ts = TimeSpan.FromSeconds(timeoutSeconds);
        var devices = await _xcdevice.GetDevicesAsync(timeout: ts).ConfigureAwait(false);
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [SkippableFact]
    public async Task ObserveAnyDevices()
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
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
        }).ConfigureAwait(false);

        Assert.True(await tcs.Task || wasAtLeastListeningAtAll, "No devices were attached/detached, nor was xcdevice listening for devices.");
    }
}