namespace AppleDev.Test;

public class XCRunTests
{
    [Fact]
    public void LocateXCRun()
    {
        var xcrun = new XCRun();
        var path = xcrun.Locate();

        Assert.NotNull(path);
        Assert.True(path.Exists);
    }
}