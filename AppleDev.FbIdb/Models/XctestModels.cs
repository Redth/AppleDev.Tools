namespace AppleDev.FbIdb.Models;

/// <summary>
/// Information about an installed test bundle.
/// </summary>
public class TestBundle
{
	/// <summary>
	/// The test bundle name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The bundle identifier.
	/// </summary>
	public string BundleId { get; set; } = string.Empty;

	/// <summary>
	/// Supported architectures.
	/// </summary>
	public IReadOnlyList<string> Architectures { get; set; } = Array.Empty<string>();
}

/// <summary>
/// XCTest run result.
/// </summary>
public class XctestRunResult
{
	/// <summary>
	/// The overall status of the test run.
	/// </summary>
	public XctestRunStatus Status { get; set; }

	/// <summary>
	/// Individual test results.
	/// </summary>
	public IReadOnlyList<TestRunInfo> Results { get; set; } = Array.Empty<TestRunInfo>();

	/// <summary>
	/// Log output from the test run.
	/// </summary>
	public IReadOnlyList<string> LogOutput { get; set; } = Array.Empty<string>();

	/// <summary>
	/// Coverage data in JSON format, if collected.
	/// </summary>
	public string? CoverageJson { get; set; }
}

/// <summary>
/// XCTest run status.
/// </summary>
public enum XctestRunStatus
{
	/// <summary>
	/// Tests are running.
	/// </summary>
	Running = 0,

	/// <summary>
	/// Tests completed normally.
	/// </summary>
	TerminatedNormally = 1,

	/// <summary>
	/// Tests terminated abnormally.
	/// </summary>
	TerminatedAbnormally = 2
}

/// <summary>
/// Information about a single test run.
/// </summary>
public class TestRunInfo
{
	/// <summary>
	/// The test status.
	/// </summary>
	public TestStatus Status { get; set; }

	/// <summary>
	/// The test bundle name.
	/// </summary>
	public string BundleName { get; set; } = string.Empty;

	/// <summary>
	/// The test class name.
	/// </summary>
	public string ClassName { get; set; } = string.Empty;

	/// <summary>
	/// The test method name.
	/// </summary>
	public string MethodName { get; set; } = string.Empty;

	/// <summary>
	/// Test duration in seconds.
	/// </summary>
	public double Duration { get; set; }

	/// <summary>
	/// Failure information if the test failed.
	/// </summary>
	public TestFailureInfo? FailureInfo { get; set; }

	/// <summary>
	/// Test logs.
	/// </summary>
	public IReadOnlyList<string> Logs { get; set; } = Array.Empty<string>();

	/// <summary>
	/// Activity logs.
	/// </summary>
	public IReadOnlyList<TestActivity> ActivityLogs { get; set; } = Array.Empty<TestActivity>();

	/// <summary>
	/// The full test name (class + method).
	/// </summary>
	public string FullName => $"{ClassName}/{MethodName}";
}

/// <summary>
/// Test status.
/// </summary>
public enum TestStatus
{
	/// <summary>
	/// Test passed.
	/// </summary>
	Passed = 0,

	/// <summary>
	/// Test failed.
	/// </summary>
	Failed = 1,

	/// <summary>
	/// Test crashed.
	/// </summary>
	Crashed = 2
}

/// <summary>
/// Test failure information.
/// </summary>
public class TestFailureInfo
{
	/// <summary>
	/// The failure message.
	/// </summary>
	public string FailureMessage { get; set; } = string.Empty;

	/// <summary>
	/// The source file where the failure occurred.
	/// </summary>
	public string File { get; set; } = string.Empty;

	/// <summary>
	/// The line number of the failure.
	/// </summary>
	public ulong Line { get; set; }
}

/// <summary>
/// Test activity information.
/// </summary>
public class TestActivity
{
	/// <summary>
	/// Activity title.
	/// </summary>
	public string Title { get; set; } = string.Empty;

	/// <summary>
	/// Activity duration.
	/// </summary>
	public double Duration { get; set; }

	/// <summary>
	/// Activity UUID.
	/// </summary>
	public string Uuid { get; set; } = string.Empty;

	/// <summary>
	/// Activity type.
	/// </summary>
	public string ActivityType { get; set; } = string.Empty;

	/// <summary>
	/// Activity name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Sub-activities.
	/// </summary>
	public IReadOnlyList<TestActivity> SubActivities { get; set; } = Array.Empty<TestActivity>();
}
