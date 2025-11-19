using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlApp>;

namespace AppleDev.Tool.Commands;

public class ListSimulatorAppsCommand : AsyncCommand<ListSimulatorAppsCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListSimulatorAppsCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			var apps = await simctl.GetAppsAsync(settings.Target, data.CancellationToken);

			if (apps == null || !apps.Any())
			{
				AnsiConsole.MarkupLine($"[yellow]No apps found on simulator {settings.Target}[/]");
				return this.ExitCode();
			}

			OutputHelper.Output(apps, settings.Format, settings.Verbose,
				new Col("Bundle ID", d => d.CFBundleIdentifier),
				new Col("Display Name", d => d.CFBundleDisplayName ?? d.CFBundleName),
				new Col("Version", d => d.CFBundleVersion),
				new Col("Type", d => d.ApplicationType, true),
				new Col("Path", d => d.Path, true));

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class ListSimulatorAppsCommandSettings : FormattableOutputCommandSettings
{
	[Description("Simulator UDID or name (e.g., 'My iPhone 16', 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
		{
			return ValidationResult.Error("Target simulator is required");
		}

		return ValidationResult.Success();
	}
}
