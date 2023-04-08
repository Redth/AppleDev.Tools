using Xunit.Abstractions;

namespace AppleDev.Test;

public class XCDeviceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XCDeviceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task LocateXCDevice()
    {
        var xcd = new XCDevice();
        var path = await xcd.LocateAsync().ConfigureAwait(false);
        
        Assert.NotNull(path);
        Assert.True(path.Exists);
    }

    [Fact]
    public async Task GetAnyDevices()
    {
        var xcd = new XCDevice();

        var devices = await xcd.GetDevicesAsync().ConfigureAwait(false);
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public async Task GetAnyDevicesWithTimeout(int timeoutSeconds)
    {
        var xcd = new XCDevice();

        var ts = TimeSpan.FromSeconds(timeoutSeconds);
        var devices = await xcd.GetDevicesAsync(timeout: ts).ConfigureAwait(false);
        
        Assert.NotNull(devices);
        Assert.NotEmpty(devices);
    }
    
    [Fact]
    public async Task ObserveAnyDevices()
    {
        var xcd = new XCDevice();

        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var tcs = new TaskCompletionSource<bool>();

        ct.Token.Register(() =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(false);
        });
        
        await xcd.ObserveAsync(ct.Token, XCDevice.XCDeviceType.Both, (id, added) =>
        {
            tcs.TrySetResult(true);
            if (!ct.IsCancellationRequested)
                ct.Cancel();

            return Task.CompletedTask;
        }, line =>
        {
            _testOutputHelper.WriteLine(line);
            return Task.CompletedTask;
        }).ConfigureAwait(false);

        Assert.True(await tcs.Task);
    }
}