using System.Text.Json;
using System.Text.RegularExpressions;
using AppleDev.Tool;
using Xunit;

namespace AppleDev.Tool.Tests;

public class OutputHelperTests
{
	record TestItem(string Name, int Value);

	static ColumnInfo<TestItem>[] TestColumns => new[]
	{
		new ColumnInfo<TestItem>("Name", x => x.Name),
		new ColumnInfo<TestItem>("Value", x => x.Value.ToString()),
	};

	string CaptureStdout(Action action)
	{
		var originalOut = Console.Out;
		using var sw = new StringWriter();
		Console.SetOut(sw);
		try
		{
			action();
			return sw.ToString();
		}
		finally
		{
			Console.SetOut(originalOut);
		}
	}

	static bool ContainsAnsiEscapes(string text) =>
		Regex.IsMatch(text, @"\x1b\[");

	[Fact]
	public void Output_JsonFormat_EmptyList_OutputsEmptyJsonArray()
	{
		var items = Enumerable.Empty<TestItem>();

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.Json, false, TestColumns));

		Assert.Equal("[]", output.Trim());
	}

	[Fact]
	public void Output_JsonFormat_WithItems_OutputsValidJsonArray()
	{
		var items = new[] { new TestItem("foo", 1), new TestItem("bar", 2) };

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.Json, false, TestColumns));

		var parsed = JsonSerializer.Deserialize<TestItem[]>(output.Trim());
		Assert.NotNull(parsed);
		Assert.Equal(2, parsed!.Length);
		Assert.Equal("foo", parsed[0].Name);
		Assert.Equal(2, parsed[1].Value);
	}

	[Fact]
	public void Output_JsonFormat_EmptyList_NoAnsiEscapes()
	{
		var items = Enumerable.Empty<TestItem>();

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.Json, false, TestColumns));

		Assert.False(ContainsAnsiEscapes(output),
			$"JSON output contains ANSI escape codes: {output}");
	}

	[Fact]
	public void Output_JsonFormat_WithItems_NoAnsiEscapes()
	{
		var items = new[] { new TestItem("test", 42) };

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.Json, false, TestColumns));

		Assert.False(ContainsAnsiEscapes(output),
			$"JSON output contains ANSI escape codes: {output}");
	}

	[Fact]
	public void Output_JsonPrettyFormat_EmptyList_OutputsValidJson()
	{
		var items = Enumerable.Empty<TestItem>();

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.JsonPretty, false, TestColumns));

		var parsed = JsonSerializer.Deserialize<TestItem[]>(output.Trim());
		Assert.NotNull(parsed);
		Assert.Empty(parsed!);
	}

	[Fact]
	public void Output_XmlFormat_WithItems_NoAnsiEscapes()
	{
		// XmlSerializer requires public types, so use a public list of strings
		var items = new List<string> { "test-value" };

		var output = CaptureStdout(() =>
			OutputHelper.Output(items, OutputFormat.Xml));

		Assert.False(ContainsAnsiEscapes(output),
			$"XML output contains ANSI escape codes: {output}");
	}

	[Fact]
	public void Output_SingleItem_JsonFormat_OutputsValidJson()
	{
		var item = new TestItem("single", 99);

		var output = CaptureStdout(() =>
			OutputHelper.Output(item, OutputFormat.Json, false, TestColumns));

		var parsed = JsonSerializer.Deserialize<TestItem>(output.Trim());
		Assert.NotNull(parsed);
		Assert.Equal("single", parsed!.Name);
		Assert.Equal(99, parsed.Value);
	}

	[Fact]
	public void Output_SingleItem_JsonFormat_NoAnsiEscapes()
	{
		var item = new TestItem("single", 99);

		var output = CaptureStdout(() =>
			OutputHelper.Output(item, OutputFormat.Json, false, TestColumns));

		Assert.False(ContainsAnsiEscapes(output),
			$"JSON output contains ANSI escape codes: {output}");
	}
}
