namespace AppleDev.Test;

public class XcodeTests
{
    [SkippableFact]
    public async Task LocateXcode()
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
        var xcode = new AppleDev.Xcode();
        var path = await xcode.LocateAsync().ConfigureAwait(false);

        Assert.NotNull(path);
        Assert.True(path.Exists);
    }
    
}