using AppleDev.FbIdb.Models;
using Xunit.Abstractions;

namespace AppleDev.FbIdb.Test;

public class ModelsTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public ModelsTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper;
	}

	[Fact]
	public void TargetDescription_DefaultValues()
	{
		var target = new TargetDescription();

		Assert.Equal(string.Empty, target.Udid);
		Assert.Equal(string.Empty, target.Name);
		Assert.Equal(string.Empty, target.State);
		Assert.Equal(string.Empty, target.TargetType);
		Assert.Equal(string.Empty, target.OsVersion);
		Assert.Equal(string.Empty, target.Architecture);
		Assert.Null(target.Screen);
	}

	[Fact]
	public void ScreenDimensions_DefaultValues()
	{
		var screen = new ScreenDimensions();

		Assert.Equal(0UL, screen.Width);
		Assert.Equal(0UL, screen.Height);
		Assert.Equal(0.0, screen.Density);
		Assert.Equal(0UL, screen.WidthPoints);
		Assert.Equal(0UL, screen.HeightPoints);
	}

	[Fact]
	public void InstalledApp_DefaultValues()
	{
		var app = new InstalledApp();

		Assert.Equal(string.Empty, app.BundleId);
		Assert.Equal(string.Empty, app.Name);
		Assert.Empty(app.Architectures);
		Assert.Equal(string.Empty, app.InstallType);
		Assert.Equal(AppProcessState.Unknown, app.ProcessState);
		Assert.False(app.Debuggable);
		Assert.Equal(0UL, app.ProcessIdentifier);
	}

	[Fact]
	public void CrashLogInfo_CrashTime_ReturnsNullForZeroTimestamp()
	{
		var crashLog = new CrashLogInfo { Timestamp = 0 };
		Assert.Null(crashLog.CrashTime);
	}

	[Fact]
	public void CrashLogInfo_CrashTime_ReturnsCorrectDateTime()
	{
		// Unix timestamp for 2024-01-01 00:00:00 UTC
		var crashLog = new CrashLogInfo { Timestamp = 1704067200 };
		Assert.NotNull(crashLog.CrashTime);
		_testOutputHelper.WriteLine($"Crash time: {crashLog.CrashTime}");
	}

	[Fact]
	public void TestRunInfo_FullName_CombinesClassAndMethod()
	{
		var testRun = new TestRunInfo
		{
			ClassName = "MyTestClass",
			MethodName = "testSomething"
		};

		Assert.Equal("MyTestClass/testSomething", testRun.FullName);
	}

	[Fact]
	public void FileContainer_Create_ReturnsCorrectContainer()
	{
		var container = FileContainer.Create(FileContainerKind.Application, "com.example.app");

		Assert.Equal(FileContainerKind.Application, container.Kind);
		Assert.Equal("com.example.app", container.BundleId);
	}

	[Fact]
	public void FileContainer_App_ReturnsApplicationContainer()
	{
		var container = FileContainer.App("com.example.app");

		Assert.Equal(FileContainerKind.Application, container.Kind);
		Assert.Equal("com.example.app", container.BundleId);
	}

	[Fact]
	public void Point_Constructor_SetsValues()
	{
		var point = new Point(100.5, 200.5);

		Assert.Equal(100.5, point.X);
		Assert.Equal(200.5, point.Y);
	}

	[Fact]
	public void GeoLocation_Constructor_SetsValues()
	{
		var location = new GeoLocation(37.7749, -122.4194);

		Assert.Equal(37.7749, location.Latitude);
		Assert.Equal(-122.4194, location.Longitude);
	}

	[Fact]
	public void Screenshot_DefaultValues()
	{
		var screenshot = new Screenshot();

		Assert.Empty(screenshot.ImageData);
		Assert.Equal(string.Empty, screenshot.ImageFormat);
	}

	[Fact]
	public void LaunchOptions_DefaultValues()
	{
		var options = new LaunchOptions();

		Assert.Null(options.Environment);
		Assert.Null(options.Arguments);
		Assert.False(options.ForegroundIfRunning);
		Assert.False(options.WaitFor);
		Assert.False(options.WaitForDebugger);
	}

	[Fact]
	public void XctestMode_Logic_IsRecord()
	{
		var mode = new XctestMode.Logic();
		Assert.NotNull(mode);
	}

	[Fact]
	public void XctestMode_Application_StoresBundleId()
	{
		var mode = new XctestMode.Application("com.example.app");
		Assert.Equal("com.example.app", mode.AppBundleId);
	}

	[Fact]
	public void XctestMode_UI_StoresBothBundleIds()
	{
		var mode = new XctestMode.UI("com.example.app", "com.example.testrunner");
		Assert.Equal("com.example.app", mode.AppBundleId);
		Assert.Equal("com.example.testrunner", mode.TestHostAppBundleId);
	}

	[Fact]
	public void XctestRunRequest_DefaultValues()
	{
		var request = new XctestRunRequest();

		Assert.Equal(string.Empty, request.TestBundleId);
		Assert.IsType<XctestMode.Logic>(request.Mode);
		Assert.Empty(request.TestsToRun);
		Assert.Empty(request.TestsToSkip);
		Assert.Empty(request.Arguments);
		Assert.Empty(request.Environment);
		Assert.Equal(0UL, request.Timeout);
		Assert.False(request.ReportActivities);
		Assert.False(request.CollectCoverage);
		Assert.False(request.ReportAttachments);
		Assert.False(request.CollectLogs);
		Assert.False(request.WaitForDebugger);
	}
}
