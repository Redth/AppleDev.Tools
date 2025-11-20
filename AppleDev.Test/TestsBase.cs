using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AppleDev.Test;

public abstract class TestsBase
{
	public TestsBase(ITestOutputHelper testOutputHelper)
	{
		TestOutputHelper = testOutputHelper;
	}

	public readonly ITestOutputHelper TestOutputHelper;

	public static string TestAssemblyDirectory
	{
		get
		{
			var location = typeof(TestsBase).Assembly.Location;
			var path = Path.GetDirectoryName(location);
			return path ?? string.Empty;
		}
	}

	public static string TestDataDirectory
	{
		get
		{
			return Path.Combine(TestAssemblyDirectory, "..", "..", "..", "testdata");
		}
	}
}
