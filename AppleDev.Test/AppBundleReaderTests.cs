using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AppleDev.Test;

public class AppBundleReaderTests : TestsBase
{
	public AppBundleReaderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
	{
		AppFile = Path.Combine(TestDataDirectory, "MauiApp7.app");
	}

	public readonly string AppFile;

	[Fact]
	public void ReadBundleId()
	{
		var reader = new AppBundleReader(AppFile);

		var infoplist = reader.ReadInfoPlist();

		var v = infoplist.CFBundleIdentifier;

		Assert.Equal("com.companyname.mauiapp12345", v);
	}


	[Fact]
	public void ReadBundleVersion()
	{
		var reader = new AppBundleReader(AppFile);

		var infoplist = reader.ReadInfoPlist();

		var v = infoplist.CFBundleVersion;

		Assert.Equal("1", v);
	}

	[Fact]
	public void ReadBundleShortVersion()
	{
		var reader = new AppBundleReader(AppFile);

		var infoplist = reader.ReadInfoPlist();

		var v = infoplist.CFBundleShortVersionString;

		Assert.Equal("1.0", v);
	}
}
