using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.XcodeInfo>;

namespace AppleDev.Tool.Commands;

public class ListXcodeCommand : AsyncCommand<ListXcodeCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListXcodeCommandSettings settings)
	{
		var data = context.GetData();
		var xcode = new Xcode();

		try
		{
			var xcodeInstalls = await xcode.LocateAllAsync(data.CancellationToken);

			if (xcodeInstalls == null || !xcodeInstalls.Any())
			{
				AnsiConsole.MarkupLine("[yellow]No Xcode installations found[/]");
				return this.ExitCode();
			}

			OutputHelper.Output(xcodeInstalls.ToList(), settings.Format, settings.Verbose,
				new Col("Path", d => d.Path),
				new Col("Version", d => d.Version?.ToString()),
				new Col("Selected", d => d.Selected ? "✓" : ""));

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class ListXcodeCommandSettings : FormattableOutputCommandSettings
{
}
