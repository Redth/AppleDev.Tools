using System.ComponentModel;
using System.Text.Json.Serialization;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListBundleIdsCommand : AsyncCommand<ListBundleIdsCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListBundleIdsCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var bundleIds = await appStoreConnect.ListBundleIdsAsync(
			filterId: settings.Ids.Length > 0 ? settings.Ids : null,
			filterSeedId: settings.SeedIds.Length > 0 ? settings.SeedIds : null,
			filterIdentifier: settings.Identifiers.Length > 0 ? settings.Identifiers : null,
			filterPlatform: settings.Platforms.Length > 0 ? settings.Platforms : null,
			//include: "app, bundleIdCapabilities",
			limit: 200)
			.ConfigureAwait(false);
		
		
		var bundleIdResults = new List<BundleId>();

		foreach (var b in bundleIds.Data)
		{
			bundleIdResults.Add(b);
		}

		OutputHelper.Output(bundleIdResults, settings.Format, settings.Verbose,
			new ColumnInfo<BundleId>("Name", r => r.Attributes.Name),
			new ColumnInfo<BundleId>("Platform", r => r.Attributes.PlatformValue),
			new ColumnInfo<BundleId>("Identifier", r => r.Attributes.Identifier),
			new ColumnInfo<BundleId>("SeedId", r => r.Attributes.SeedId),
			new ColumnInfo<BundleId>("Id", r => r.Id));

		return this.ExitCode();
	}
}



public class ListBundleIdsCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter to platform")]
	[CommandOption("--platform <platform>")]
	[DefaultValue(Platform.IOS)]
	[TypeConverter(typeof(StringEnumTypeConverter<Platform>))]
	public Platform[] Platforms { get; set; } = new Platform[] { Platform.IOS, Platform.MAC_OS };

	[Description("Filter by ID")]
	[CommandOption("--id <id>")]
	public string[] Ids { get; set; } = new string[0];

	[Description("Filter by Identifier")]
	[CommandOption("--identifier <id>")]
	public string[] Identifiers { get; set; } = new string[0];

	[Description("Filter by Seed ID")]
	[CommandOption("--seed-id <seedid>")]
	public string[] SeedIds { get; set; } = new string[0];
}
