using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class DisableBundleIdCapabilityCommand : AsyncCommand<DisableBundleIdCapabilityCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, DisableBundleIdCapabilityCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		try
		{
			var success = await appStoreConnect.DisableCapabilityAsync(
				settings.CapabilityId,
				data.CancellationToken)
				.ConfigureAwait(false);

			if (success)
			{
				AnsiConsole.MarkupLine($"[green]Successfully disabled capability '{settings.CapabilityId}'[/]");
				return this.ExitCode();
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Failed to disable capability '{settings.CapabilityId}'[/]");
				return this.ExitCode(false);
			}
		}
		catch (AppleApiException ex)
		{
			AnsiConsole.MarkupLine($"[red]Failed to disable capability: {ex.Message}[/]");
			foreach (var error in ex.Errors)
			{
				AnsiConsole.MarkupLine($"  [red]{error.Title}: {error.Detail}[/]");
			}
			return this.ExitCode(false);
		}
	}
}

public class DisableBundleIdCapabilityCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("The resource ID of the capability to disable (use 'bundleids capabilities list <bundle-id-resource-id>' to find it)")]
	[CommandArgument(0, "<capability-id>")]
	public string CapabilityId { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(CapabilityId))
			return ValidationResult.Error("Capability ID is required");
		
		return base.Validate();
	}
}
