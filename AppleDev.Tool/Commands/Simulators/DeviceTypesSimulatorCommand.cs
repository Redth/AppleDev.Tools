using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlDeviceType>;

namespace AppleDev.Tool.Commands;

public class DeviceTypesSimulatorCommand : AsyncCommand<DeviceTypesSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeviceTypesSimulatorCommandSettings settings)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			var deviceTypes = await simctl.GetSimulatorGroupsAsync(data.CancellationToken);

			if (deviceTypes == null || !deviceTypes.Any())
			{
				AnsiConsole.MarkupLine("[yellow]No device types found[/]");
				return this.ExitCode();
			}

			OutputHelper.Output(deviceTypes, settings.Format, settings.Verbose,
				new Col("Name", d => d.Name),
				new Col("Identifier", d => d.Identifier),
				new Col("Product Family", d => d.ProductFamily),
				new Col("Bundle Path", d => d.BundlePath, true),
				new Col("Min Runtime Version", d => d.MinRuntimeVersion.ToString(), true),
				new Col("Max Runtime Version", d => d.MaxRuntimeVersion.ToString(), true));

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class DeviceTypesSimulatorCommandSettings : FormattableOutputCommandSettings
{
}
