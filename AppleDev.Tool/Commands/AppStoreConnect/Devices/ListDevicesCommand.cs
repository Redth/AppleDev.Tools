using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ListAscDevicesCommand : AsyncCommand<ListAscDevicesCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ListAscDevicesCommandSettings settings)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.KeyId, settings.IssuerId, settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		var devices = await appStoreConnect.ListDevicesAsync(
			filterId: settings.Ids.Length > 0 ? settings.Ids : null,
			filterName: settings.Names.Length > 0 ? settings.Names : null,
			filterPlatform: settings.Platforms.Length > 0 ? settings.Platforms : null,
			filterStatus: settings.Statuses.Length > 0 ? settings.Statuses : null,
			filterUdid: settings.Udids.Length > 0 ? settings.Udids : null,
			limit: 200,
			cancellationToken: data.CancellationToken)
			.ConfigureAwait(false);
		
		var deviceResults = new List<AppStoreConnectDevice>();

		foreach (var d in devices.Data)
		{
			deviceResults.Add(new AppStoreConnectDevice
			{
				Id = d.Id,
				Name = d.Attributes.Name,
				Platform = d.Attributes.PlatformValue,
				Status = d.Attributes.StatusValue,
				Udid = d.Attributes.Udid,
				DeviceClass = d.Attributes.DeviceClass,
				Model = d.Attributes.Model,
				AddedDate = d.Attributes.AddedDate
			});
		}

		OutputHelper.Output(deviceResults, settings.Format, settings.Verbose,
			new ColumnInfo<AppStoreConnectDevice>("Name", r => r.Name),
			new ColumnInfo<AppStoreConnectDevice>("Platform", r => r.Platform),
			new ColumnInfo<AppStoreConnectDevice>("Status", r => r.Status),
			new ColumnInfo<AppStoreConnectDevice>("UDID", r => r.Udid),
			new ColumnInfo<AppStoreConnectDevice>("Device Class", r => r.DeviceClass, true),
			new ColumnInfo<AppStoreConnectDevice>("Model", r => r.Model, true),
			new ColumnInfo<AppStoreConnectDevice>("Added Date", r => r.AddedDate?.ToString("yyyy-MM-dd"), true),
			new ColumnInfo<AppStoreConnectDevice>("Id", r => r.Id, true));

		return this.ExitCode();
	}

	class AppStoreConnectDevice
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Platform { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public string Udid { get; set; } = string.Empty;
		public string? DeviceClass { get; set; }
		public string? Model { get; set; }
		public DateTimeOffset? AddedDate { get; set; }
	}
}

public class ListAscDevicesCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Filter by device ID")]
	[CommandOption("--id <id>")]
	public string[] Ids { get; set; } = Array.Empty<string>();

	[Description("Filter by device name")]
	[CommandOption("--name <name>")]
	public string[] Names { get; set; } = Array.Empty<string>();

	[Description("Filter by platform")]
	[CommandOption("--platform <platform>")]
	[TypeConverter(typeof(StringEnumTypeConverter<Platform>))]
	public Platform[] Platforms { get; set; } = Array.Empty<Platform>();

	[Description("Filter by status")]
	[CommandOption("--status <status>")]
	[TypeConverter(typeof(StringEnumTypeConverter<DeviceStatus>))]
	public DeviceStatus[] Statuses { get; set; } = Array.Empty<DeviceStatus>();

	[Description("Filter by UDID")]
	[CommandOption("--udid <udid>")]
	public string[] Udids { get; set; } = Array.Empty<string>();
}
