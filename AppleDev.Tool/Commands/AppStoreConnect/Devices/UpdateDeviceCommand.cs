using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class UpdateDeviceCommand : AsyncCommand<UpdateDeviceCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, UpdateDeviceCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var deviceAttributes = new DeviceAttributes();
		
		if (!string.IsNullOrWhiteSpace(settings.Name))
			deviceAttributes.Name = settings.Name;
		
		if (settings.Status.HasValue)
			deviceAttributes.Status = settings.Status.Value;

		var response = await appStoreConnect.ModifyDeviceAsync(
			settings.Id,
			deviceAttributes,
			data.CancellationToken)
			.ConfigureAwait(false);

		if (response.Data != null)
		{
			AnsiConsole.MarkupLine($"[green]Successfully updated device '{settings.Id}'[/]");
			AnsiConsole.MarkupLine($"  Name: {response.Data.Attributes.Name}");
			AnsiConsole.MarkupLine($"  Status: {response.Data.Attributes.StatusValue}");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Failed to update device '{settings.Id}'[/]");
			return this.ExitCode(false);
		}
	}
}

public class UpdateDeviceCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Device ID to update")]
	[CommandArgument(0, "<id>")]
	public string Id { get; set; } = string.Empty;

	[Description("New device name")]
	[CommandOption("--name <name>")]
	public string? Name { get; set; }

	[Description("New device status (ENABLED, DISABLED)")]
	[CommandOption("--status <status>")]
	[TypeConverter(typeof(StringEnumTypeConverter<DeviceStatus>))]
	public DeviceStatus? Status { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Id))
			return ValidationResult.Error("Device ID is required");
		
		if (string.IsNullOrWhiteSpace(Name) && !Status.HasValue)
			return ValidationResult.Error("At least one of --name or --status must be specified");
		
		return base.Validate();
	}
}
