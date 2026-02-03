using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbSwipeCommand : AsyncCommand<IdbSwipeCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbSwipeCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Swiping from [cyan]({settings.X1}, {settings.Y1})[/] to [cyan]({settings.X2}, {settings.Y2})[/] on [cyan]{settings.Target}[/]...");

		try
		{
			var start = new Point(settings.X1, settings.Y1);
			var end = new Point(settings.X2, settings.Y2);
			var duration = (settings.Duration ?? 500) / 1000.0;
			await client.SwipeAsync(start, end, duration, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Swipe completed successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbSwipeCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Start X coordinate")]
	[CommandArgument(1, "<x1>")]
	public double X1 { get; set; }

	[Description("Start Y coordinate")]
	[CommandArgument(2, "<y1>")]
	public double Y1 { get; set; }

	[Description("End X coordinate")]
	[CommandArgument(3, "<x2>")]
	public double X2 { get; set; }

	[Description("End Y coordinate")]
	[CommandArgument(4, "<y2>")]
	public double Y2 { get; set; }

	[Description("Swipe duration in milliseconds (default: 500)")]
	[CommandOption("-d|--duration <MS>")]
	public int? Duration { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		return ValidationResult.Success();
	}
}
