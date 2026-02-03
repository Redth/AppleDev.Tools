using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbCrashesListCommand : AsyncCommand<IdbCrashesListCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbCrashesListCommandSettings settings, CancellationToken cancellationToken)
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
			var crashes = await client.ListCrashLogsAsync(settings.BundleId, settings.Since, null, data.CancellationToken).ConfigureAwait(false);

			if (crashes.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]No crash logs found[/]");
			}
			else
			{
				var table = new Table();
				table.AddColumn("Name");
				table.AddColumn("Bundle ID");
				table.AddColumn("Process Name");
				table.AddColumn("Date");

				foreach (var crash in crashes)
				{
					table.AddRow(
						crash.Name ?? "",
						crash.BundleId ?? "",
						crash.ProcessName ?? "",
						crash.CrashTime?.ToString("g") ?? "");
				}

				AnsiConsole.Write(table);
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

public class IdbCrashesListCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Filter crashes by bundle ID")]
	[CommandOption("-b|--bundle-id <BUNDLE_ID>")]
	public string? BundleId { get; set; }

	[Description("Filter crashes since date (ISO 8601 format)")]
	[CommandOption("--since <DATE>")]
	public DateTime? Since { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		return ValidationResult.Success();
	}
}

public class IdbCrashesGetCommand : AsyncCommand<IdbCrashesGetCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbCrashesGetCommandSettings settings, CancellationToken cancellationToken)
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
			var crashLog = await client.GetCrashLogAsync(settings.Name, data.CancellationToken).ConfigureAwait(false);

			if (crashLog is null)
			{
				AnsiConsole.MarkupLine($"[yellow]Crash log not found: {settings.Name}[/]");
				return this.ExitCode(false);
			}

			if (!string.IsNullOrEmpty(settings.Output))
			{
				await File.WriteAllTextAsync(settings.Output, crashLog.Contents, data.CancellationToken).ConfigureAwait(false);
				AnsiConsole.MarkupLine($"[green]✓ Crash log saved to {settings.Output}[/]");
			}
			else
			{
				AnsiConsole.WriteLine(crashLog.Contents ?? "");
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

public class IdbCrashesGetCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Crash log name")]
	[CommandArgument(1, "<name>")]
	public string Name { get; set; } = string.Empty;

	[Description("Output file path")]
	[CommandOption("-o|--output <PATH>")]
	public string? Output { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(Name))
			return ValidationResult.Error("Crash log name is required");

		return ValidationResult.Success();
	}
}

public class IdbCrashesDeleteCommand : AsyncCommand<IdbCrashesDeleteCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbCrashesDeleteCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Deleting crash logs...");

		try
		{
			var deleted = await client.DeleteCrashLogsAsync(settings.BundleId, null, settings.Before, settings.Name, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Deleted {deleted.Count} crash log(s)[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbCrashesDeleteCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Filter crashes by bundle ID")]
	[CommandOption("-b|--bundle-id <BUNDLE_ID>")]
	public string? BundleId { get; set; }

	[Description("Filter crashes by name")]
	[CommandOption("-n|--name <NAME>")]
	public string? Name { get; set; }

	[Description("Delete crashes before date (ISO 8601 format)")]
	[CommandOption("--before <DATE>")]
	public DateTime? Before { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrEmpty(BundleId) && string.IsNullOrEmpty(Name) && !Before.HasValue)
			return ValidationResult.Error("Specify --bundle-id, --name, or --before");

		return ValidationResult.Success();
	}
}
