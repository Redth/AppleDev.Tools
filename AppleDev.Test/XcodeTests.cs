namespace AppleDev.Test;

public class XcodeTests
{
    [Fact]
    public async Task LocateXcode()
    {
        var xcode = new AppleDev.Xcode();
        var path = await xcode.LocateAsync();

        Assert.NotNull(path);
        Assert.True(path.Exists);
    }
    
}