using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class RegisterDeviceCommand : AsyncCommand<RegisterDeviceCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, RegisterDeviceCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		var appStoreConnect = new AppStoreConnectClient(config);

		try
		{
			var deviceAttributes = new DeviceAttributes
			{
				Name = settings.Name,
				Platform = settings.Platform,
				Udid = settings.Udid
			};

			var response = await appStoreConnect.RegisterDeviceAsync(
				deviceAttributes,
				data.CancellationToken).ConfigureAwait(false);

			AnsiConsole.MarkupLine($"[green]Successfully registered device '{settings.Name}'[/]");
			AnsiConsole.MarkupLine($"  ID: {response.Data.Id}");
			AnsiConsole.MarkupLine($"  Platform: {response.Data.Attributes.PlatformValue}");
			AnsiConsole.MarkupLine($"  UDID: {response.Data.Attributes.Udid}");
			AnsiConsole.MarkupLine($"  Status: {response.Data.Attributes.StatusValue}");
			return this.ExitCode();
		}
		catch (AppleApiException apiEx)
		{
			AnsiConsole.MarkupLine($"[red]Failed to register device '{settings.Name}' (HTTP {apiEx.HttpStatusCode})[/]");
			foreach (var err in apiEx.Errors)
			{
				AnsiConsole.MarkupLine($"  [yellow]{err.Code}[/]: {err.Detail} [dim]{err.Source?.Pointer}[/]");
			}
			return this.ExitCode(false);
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class RegisterDeviceCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Device name")]
	[CommandArgument(0, "<name>")]
	public string Name { get; set; } = string.Empty;

	[Description("Device UDID")]
	[CommandArgument(1, "<udid>")]
	public string Udid { get; set; } = string.Empty;

	[Description("Platform (IOS, MAC_OS)")]
	[CommandArgument(2, "<platform>")]
	[TypeConverter(typeof(StringEnumTypeConverter<Platform>))]
	public Platform Platform { get; set; } = Platform.IOS;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Name))
			return ValidationResult.Error("Device name is required");
		
		if (string.IsNullOrWhiteSpace(Udid))
			return ValidationResult.Error("Device UDID is required");
		
		return base.Validate();
	}
}
