namespace AppleDev.Test;

public class XCRunTests
{
    [SkippableFact]
    public void LocateXCRun()
    {
        Skip.IfNot(OperatingSystem.IsMacOS(), "Test requires macOS");
        var xcrun = new XCRun();
        var path = xcrun.Locate();

        Assert.NotNull(path);
        Assert.True(path.Exists);
    }
}