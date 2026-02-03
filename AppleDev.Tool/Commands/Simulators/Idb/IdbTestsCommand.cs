using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbTestsListCommand : AsyncCommand<IdbTestsListCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbTestsListCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		try
		{
			var tests = await client.ListTestsAsync(settings.TestBundle, null, data.CancellationToken).ConfigureAwait(false);

			if (tests.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]No tests found in bundle[/]");
			}
			else
			{
				foreach (var test in tests)
				{
					AnsiConsole.WriteLine(test);
				}
				AnsiConsole.MarkupLine($"\n[green]Total: {tests.Count} test(s)[/]");
			}

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbTestsListCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Test bundle name or path")]
	[CommandArgument(1, "<test-bundle>")]
	public string TestBundle { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(TestBundle))
			return ValidationResult.Error("Test bundle is required");

		return ValidationResult.Success();
	}
}

public class IdbTestsRunCommand : AsyncCommand<IdbTestsRunCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbTestsRunCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Running tests from [cyan]{settings.TestBundle}[/]...");

		try
		{
			var request = new XctestRunRequest
			{
				TestBundleId = settings.TestBundle
			};

			if (!string.IsNullOrEmpty(settings.Filter))
			{
				request.TestsToRun = new List<string> { settings.Filter };
			}

			if (!string.IsNullOrEmpty(settings.Skip))
			{
				request.TestsToSkip = new List<string> { settings.Skip };
			}

			if (settings.Timeout.HasValue)
			{
				request.Timeout = (ulong)settings.Timeout.Value;
			}

			var result = await client.RunTestsAsync(request, data.CancellationToken).ConfigureAwait(false);

			var passed = 0;
			var failed = 0;

			foreach (var testResult in result.Results)
			{
				var isPassed = testResult.Status == TestStatus.Passed;
				var status = isPassed ? "[green]PASS[/]" : "[red]FAIL[/]";
				AnsiConsole.MarkupLine($"{status} {testResult.ClassName}.{testResult.MethodName}");

				if (testResult.FailureInfo != null && !isPassed)
				{
					AnsiConsole.MarkupLine($"  [dim]{testResult.FailureInfo.FailureMessage}[/]");
				}

				if (isPassed)
					passed++;
				else
					failed++;
			}

			AnsiConsole.WriteLine();
			if (failed > 0)
			{
				AnsiConsole.MarkupLine($"[red]Results: {passed} passed, {failed} failed[/]");
				return this.ExitCode(false);
			}
			else
			{
				AnsiConsole.MarkupLine($"[green]Results: {passed} passed, {failed} failed[/]");
				return this.ExitCode();
			}
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbTestsRunCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Test bundle name or path")]
	[CommandArgument(1, "<test-bundle>")]
	public string TestBundle { get; set; } = string.Empty;

	[Description("Test filter (e.g., 'MyTestClass/testMethod')")]
	[CommandOption("-f|--filter <FILTER>")]
	public string? Filter { get; set; }

	[Description("Tests to skip")]
	[CommandOption("-s|--skip <TESTS>")]
	public string? Skip { get; set; }

	[Description("Test timeout in seconds")]
	[CommandOption("-t|--timeout <SECONDS>")]
	public int? Timeout { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(TestBundle))
			return ValidationResult.Error("Test bundle is required");

		return ValidationResult.Success();
	}
}
