using System.Collections.Generic;
using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;
using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlDeviceType>;

namespace AppleDev.Tool.Commands;

public class DeviceTypesSimulatorCommand : AsyncCommand<DeviceTypesSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DeviceTypesSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		try
		{
			var deviceTypes = await simctl.GetSimulatorGroupsAsync(settings.IncludeScreenInfo, data.CancellationToken);

			if (deviceTypes == null || !deviceTypes.Any())
			{
				AnsiConsole.MarkupLine("[yellow]No device types found[/]");
				return this.ExitCode();
			}

			var columns = new List<Col>
			{
				new Col("Name", d => d.Name),
				new Col("Identifier", d => d.Identifier),
				new Col("Product Family", d => d.ProductFamily),
				new Col("Bundle Path", d => d.BundlePath, true),
				new Col("Min Runtime Version", d => d.MinRuntimeVersion.ToString(), true),
				new Col("Max Runtime Version", d => d.MaxRuntimeVersion.ToString(), true)
			};

			if (settings.IncludeScreenInfo)
			{
				columns.Add(new Col("Screen Size", d => d.Screen != null ? $"{d.Screen.Width}x{d.Screen.Height}" : "N/A"));
				columns.Add(new Col("Scale", d => d.Screen?.Scale.ToString() ?? "N/A"));
				columns.Add(new Col("Pixel Size", d => d.Screen != null ? $"{d.Screen.PixelWidth}x{d.Screen.PixelHeight}" : "N/A"));
				columns.Add(new Col("DPI", d => d.Screen != null ? $"{d.Screen.WidthDPI}x{d.Screen.HeightDPI}" : "N/A"));
			}

			if (settings.Verbose)
			{
				if (settings.IncludeScreenInfo)
				{
					columns.Add(new Col("Colorspace", d => d.Screen?.Colorspace ?? "N/A", true));
					columns.Add(new Col("Model ID", d => d.ModelIdentifier, true));
					columns.Add(new Col("Product Class", d => d.ProductClass, true));
				}
			}

			OutputHelper.Output(deviceTypes, settings.Format, settings.Verbose, columns.ToArray());

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
    [Description("Include screen information (size, scale, DPI, colorspace)")]
    [CommandOption("--include-screen-info")]
    [DefaultValue(false)]
    public bool IncludeScreenInfo { get; set; }
}
