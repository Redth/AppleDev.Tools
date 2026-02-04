using System.ComponentModel;
using System.Text.Json;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbAccessibilityCommand : AsyncCommand<IdbAccessibilityCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbAccessibilityCommandSettings settings, CancellationToken cancellationToken)
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
			Point? point = (settings.X.HasValue && settings.Y.HasValue)
				? new Point(settings.X.Value, settings.Y.Value)
				: null;

			var info = await client.GetAccessibilityInfoAsync(point, AccessibilityFormat.Nested, data.CancellationToken).ConfigureAwait(false);

			// Output the JSON accessibility info
			if (settings.Json || !string.IsNullOrEmpty(info.Json))
			{
				AnsiConsole.WriteLine(info.Json);
			}
			else
			{
				AnsiConsole.MarkupLine($"[bold]Accessibility Info[/]");
				AnsiConsole.WriteLine(info.Json);
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

public class IdbAccessibilityCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("X coordinate (optional)")]
	[CommandOption("--point-x <X>")]
	public double? X { get; set; }

	[Description("Y coordinate (optional)")]
	[CommandOption("--point-y <Y>")]
	public double? Y { get; set; }

	[Description("Output as JSON")]
	[CommandOption("-j|--json")]
	public bool Json { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		// Both X and Y must be provided together, or neither
		if ((X.HasValue && !Y.HasValue) || (!X.HasValue && Y.HasValue))
			return ValidationResult.Error("Both --point-x and --point-y must be provided together, or neither");

		return ValidationResult.Success();
	}
}
