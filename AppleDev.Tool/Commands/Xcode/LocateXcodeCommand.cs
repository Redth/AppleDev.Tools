using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class LocateXcodeCommand : AsyncCommand<LocateXcodeCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, LocateXcodeCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var xcode = new Xcode();

		try
		{
			if (settings.Best)
			{
				var bestXcode = await xcode.LocateBestAsync(data.CancellationToken);
				
				if (bestXcode == null)
				{
					AnsiConsole.MarkupLine("[yellow]No Xcode installation found[/]");
					return this.ExitCode(false);
				}

				if (settings.Format == OutputFormat.Json)
				{
					OutputHelper.Output(new[] { bestXcode }, settings.Format, settings.Verbose,
						new ColumnInfo<XcodeInfo>("Path", d => d.Path),
						new ColumnInfo<XcodeInfo>("Version", d => d.Version?.ToString()),
						new ColumnInfo<XcodeInfo>("Selected", d => d.Selected ? "true" : "false"));
				}
				else
				{
					AnsiConsole.MarkupLine($"[cyan]Path:[/] {bestXcode.Path}");
					AnsiConsole.MarkupLine($"[cyan]Version:[/] {bestXcode.Version}");
					AnsiConsole.MarkupLine($"[cyan]Selected:[/] {(bestXcode.Selected ? "Yes" : "No")}");
				}

				return this.ExitCode();
			}
			else
			{
				var selectedXcode = await xcode.LocateAsync(data.CancellationToken);
				
				if (selectedXcode == null)
				{
					AnsiConsole.MarkupLine("[yellow]No Xcode selected (use 'xcode-select' to set one)[/]");
					return this.ExitCode(false);
				}

				if (settings.Format == OutputFormat.Json)
				{
					AnsiConsole.WriteLine($"{{\"path\": \"{selectedXcode.FullName}\"}}");
				}
				else
				{
					AnsiConsole.MarkupLine($"[cyan]{selectedXcode.FullName}[/]");
				}

				return this.ExitCode();
			}
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class LocateXcodeCommandSettings : FormattableOutputCommandSettings
{
	[Description("Show best available Xcode (highest version if none selected)")]
	[CommandOption("--best")]
	[DefaultValue(false)]
	public bool Best { get; set; }
}
