using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbTapCommand : AsyncCommand<IdbTapCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbTapCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Tapping at [cyan]({settings.X}, {settings.Y})[/] on [cyan]{settings.Target}[/]...");

		try
		{
			await client.TapAsync(settings.X, settings.Y, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Tap completed successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbTapCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("X coordinate")]
	[CommandArgument(1, "<x>")]
	public double X { get; set; }

	[Description("Y coordinate")]
	[CommandArgument(2, "<y>")]
	public double Y { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (X < 0)
			return ValidationResult.Error("X coordinate must be non-negative");

		if (Y < 0)
			return ValidationResult.Error("Y coordinate must be non-negative");

		return ValidationResult.Success();
	}
}
