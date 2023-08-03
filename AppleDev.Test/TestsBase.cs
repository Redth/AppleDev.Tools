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
			var codeBase = typeof(TestsBase).Assembly.CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
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
