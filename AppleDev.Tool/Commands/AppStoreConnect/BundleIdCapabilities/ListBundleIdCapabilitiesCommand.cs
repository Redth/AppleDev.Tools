using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListBundleIdCapabilitiesCommand : AsyncCommand<ListBundleIdCapabilitiesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListBundleIdCapabilitiesCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var response = await appStoreConnect.ListBundleIdCapabilitiesAsync(
			settings.BundleIdResourceId,
			cancellationToken: data.CancellationToken)
			.ConfigureAwait(false);

		var capabilities = response.Data?.ToList() ?? new List<BundleIdCapability>();

		OutputHelper.Output(capabilities, settings.Format, settings.Verbose,
			new ColumnInfo<BundleIdCapability>("CapabilityType", c => c.Attributes.CapabilityTypeValue),
			new ColumnInfo<BundleIdCapability>("Id", c => c.Id));

		return this.ExitCode();
	}
}

public class ListBundleIdCapabilitiesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("The resource ID of the bundle ID (use 'bundleids list' to find it)")]
	[CommandArgument(0, "<bundle-id-resource-id>")]
	public string BundleIdResourceId { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(BundleIdResourceId))
			return ValidationResult.Error("Bundle ID resource ID is required");
		
		return base.Validate();
	}
}
