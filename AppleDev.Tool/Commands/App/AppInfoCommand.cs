using System.ComponentModel;
using AppleDev;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class AppInfoCommand : Command<AppInfoCommandSettings>
{
	public override int Execute(CommandContext context, AppInfoCommandSettings settings, CancellationToken cancellationToken)
	{
		try
		{
			var reader = new AppBundleReader(settings.AppPath);
			var infoPlist = reader.ReadInfoPlist();

			var appInfo = new
			{
				BundleIdentifier = infoPlist.CFBundleIdentifier ?? "Unknown",
				DisplayName = infoPlist.CFBundleDisplayName ?? infoPlist.CFBundleName ?? "Unknown",
				BundleVersion = infoPlist.CFBundleVersion ?? "Unknown",
				ShortVersionString = infoPlist.CFBundleShortVersionString ?? "Unknown",
				PlatformName = infoPlist.DTPlatformName ?? "Unknown",
				PlatformVersion = infoPlist.DTPlatformVersion ?? "Unknown",
				SDKName = infoPlist.DTSDKName ?? "Unknown",
				InfoPlistPath = reader.InfoPlistFile ?? "Unknown"
			};

			if (settings.Format == OutputFormat.Json)
			{
				var json = System.Text.Json.JsonSerializer.Serialize(appInfo, new System.Text.Json.JsonSerializerOptions 
				{ 
					WriteIndented = true 
				});
				AnsiConsole.WriteLine(json);
			}
			else if (settings.Verbose)
			{
				AnsiConsole.MarkupLine($"[cyan]Bundle Identifier:[/] {appInfo.BundleIdentifier}");
				AnsiConsole.MarkupLine($"[cyan]Display Name:[/] {appInfo.DisplayName}");
				AnsiConsole.MarkupLine($"[cyan]Bundle Version:[/] {appInfo.BundleVersion}");
				AnsiConsole.MarkupLine($"[cyan]Short Version String:[/] {appInfo.ShortVersionString}");
				AnsiConsole.MarkupLine($"[cyan]Platform Name:[/] {appInfo.PlatformName}");
				AnsiConsole.MarkupLine($"[cyan]Platform Version:[/] {appInfo.PlatformVersion}");
				AnsiConsole.MarkupLine($"[cyan]SDK Name:[/] {appInfo.SDKName}");
				AnsiConsole.MarkupLine($"[cyan]Info.plist Path:[/] {appInfo.InfoPlistPath}");
			}
			else
			{
				var table = new Table();
				table.AddColumn("Property");
				table.AddColumn("Value");

				table.AddRow("Bundle Identifier", appInfo.BundleIdentifier);
				table.AddRow("Display Name", appInfo.DisplayName);
				table.AddRow("Bundle Version", appInfo.BundleVersion);
				table.AddRow("Short Version String", appInfo.ShortVersionString);
				table.AddRow("Platform Name", appInfo.PlatformName);
				table.AddRow("Platform Version", appInfo.PlatformVersion);
				table.AddRow("SDK Name", appInfo.SDKName);

				AnsiConsole.Write(table);
			}

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
			return this.ExitCode(false);
		}
	}
}

public class AppInfoCommandSettings : FormattableOutputCommandSettings
{
	[Description("Path to .app bundle")]
	[CommandArgument(0, "<app-path>")]
	public string AppPath { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(AppPath))
		{
			return ValidationResult.Error("App path is required");
		}

		if (!Directory.Exists(AppPath) && !File.Exists(AppPath))
		{
			return ValidationResult.Error($"App bundle not found: {AppPath}");
		}

		return ValidationResult.Success();
	}
}
