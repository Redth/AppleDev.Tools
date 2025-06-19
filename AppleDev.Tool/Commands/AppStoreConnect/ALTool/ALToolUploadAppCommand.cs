using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using AppleAppStoreConnect;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class ALToolUploadAppCommand : AsyncCommand<ALToolUploadAppCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, ALToolUploadAppCommandSettings settings)
	{
		var data = context.GetData();

		var altool = new ALTool();

		await altool.InstallPrivateKey(settings.KeyId, settings.GetPrivateKeyBase64()).ConfigureAwait(false);

		var result = await altool.UploadAppAsync(
			settings.AppPath!,
			settings.AppType,
			settings.KeyId,
			settings.IssuerId,
			data.CancellationToken)
			.ConfigureAwait(false);

		OutputHelper.Output(result, settings.Format, verbose: settings.Verbose);

		return this.ExitCode(result.Success);
	}
}

public class ALToolUploadAppCommandSettings : FormattableOutputAppStoreConnectCommand
{
	[Description("Type of Apple certificate")]
	[CommandOption("-t|--app-type <type>")]
	[DefaultValue(ALToolAppType.iOS)]
	[TypeConverter(typeof(StringEnumTypeConverter<ALToolAppType>))]
	public ALToolAppType AppType { get; set; } = ALToolAppType.iOS;

	[Description("App Path")]
	[CommandOption("-p|--app-path <app-path>")]
	public string? AppPath { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(AppPath) || (!Directory.Exists(AppPath) && !File.Exists(AppPath)))
			return ValidationResult.Error("App Path is required and must be a valid directory or file path");

		return base.Validate();
	}
}
