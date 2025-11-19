using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListDevicesCommand : AsyncCommand<ListDevicesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListDevicesCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var xcdevice = new XCDevice();
		var devices = await xcdevice.GetDevicesAsync(data.CancellationToken, TimeSpan.FromSeconds(settings.Timeout)).ConfigureAwait(false);

		if (settings.DevicesOnly)
			devices = devices.Where(d => !d.Simulator).ToList();

		OutputHelper.Output(devices, settings.Format, settings.Verbose,
			new ColumnInfo<Device>("Name", d => d.Name),
			new ColumnInfo<Device>("Identifier", d => d.Identifier),
			new ColumnInfo<Device>("Platform", d => d.Platform),
			new ColumnInfo<Device>("OS Version", d => d.OperatingSystemVersion),
			new ColumnInfo<Device>("Name", d => d.Architecture, true),
			new ColumnInfo<Device>("Interface", d => d.Interface, true),
			new ColumnInfo<Device>("Model Name", d => d.ModelName, true),
			new ColumnInfo<Device>("Model Code", d => d.ModelCode, true));

		return this.ExitCode();
	}
}
public class ListDevicesCommandSettings : FormattableOutputCommandSettings
{
	[Description("Timeout to search for more network devices in seconds")]
	[DefaultValue(5)]
	[CommandOption("-t|--timeout <seconds>")]
	public int Timeout { get; set; } = 5;

	[Description("Show devices only, no simulators")]
	[CommandOption("-d|--devices-only")]
	public bool DevicesOnly { get; set; } = false;

	public override ValidationResult Validate()
	{
		if (Timeout <= 0)
			return ValidationResult.Error("Invalide --timeout value, must be greater than 0");

		return base.Validate();
	}
}