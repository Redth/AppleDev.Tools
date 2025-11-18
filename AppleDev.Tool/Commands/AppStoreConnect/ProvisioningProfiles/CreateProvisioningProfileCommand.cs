using System.ComponentModel;
using AppleAppStoreConnect;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CreateProvisioningProfileCommand : AsyncCommand<CreateProvisioningProfileCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CreateProvisioningProfileCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var config = new AppStoreConnectConfiguration(settings.GetKeyId(), settings.GetIssuerId(), settings.GetPrivateKeyBase64());
		
		var appStoreConnect = new AppStoreConnectClient(config);

		// Resolve Bundle ID (accept either resource ID or bundle identifier)
		string bundleIdResourceId = settings.BundleId;
		
		// Check if it looks like a bundle identifier (contains dots) rather than a resource ID
		if (settings.BundleId.Contains('.'))
		{
			AnsiConsole.MarkupLine($"[yellow]Resolving bundle identifier '[/][cyan]{settings.BundleId}[/][yellow]'...[/]");
			
			try
			{
				var bundleIdsResponse = await appStoreConnect.ListBundleIdsAsync(
					limit: 200,
					cancellationToken: data.CancellationToken);
				
				var matchingBundleId = bundleIdsResponse.Data
					?.FirstOrDefault(b => b.Attributes.Identifier.Equals(settings.BundleId, StringComparison.OrdinalIgnoreCase));
				
				if (matchingBundleId == null)
				{
					AnsiConsole.MarkupLine($"[red]Error:[/] No Bundle ID found with identifier '{settings.BundleId}'");
					AnsiConsole.MarkupLine($"[yellow]Tip:[/] Use 'apple bundleids list' to see available Bundle IDs");
					return this.ExitCode(false);
				}
				
				bundleIdResourceId = matchingBundleId.Id;
				AnsiConsole.MarkupLine($"[green]✓ Resolved to resource ID:[/] {bundleIdResourceId}");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error resolving bundle identifier:[/] {ex.Message}");
				return this.ExitCode(false);
			}
		}

		// Parse certificate IDs
		var certificateIds = settings.Certificates.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		
		if (certificateIds.Length == 0)
		{
			AnsiConsole.MarkupLine("[red]Error:[/] At least one certificate ID is required.");
			return this.ExitCode(false);
		}

		// Parse device IDs (optional, not needed for App Store profiles)
		string[]? deviceIds = null;
		
		if (settings.AllDevices)
		{
			// Fetch all enabled devices for the appropriate platform
			AnsiConsole.MarkupLine("[yellow]Fetching all enabled devices...[/]");
			
			var platform = GetPlatformFromProfileType(settings.ProfileType);
			var devicesResponse = await appStoreConnect.ListDevicesAsync(
				filterPlatform: new[] { platform },
				filterStatus: new[] { DeviceStatus.ENABLED },
				cancellationToken: data.CancellationToken);
			
			if (devicesResponse.Data != null && devicesResponse.Data.Any())
			{
				deviceIds = devicesResponse.Data.Select(d => d.Id).ToArray();
				AnsiConsole.MarkupLine($"[green]Found {deviceIds.Length} enabled device(s) for {platform}[/]");
			}
			else
			{
				AnsiConsole.MarkupLine($"[yellow]Warning:[/] No enabled devices found for {platform}");
			}
		}
		else if (!string.IsNullOrEmpty(settings.Devices))
		{
			deviceIds = settings.Devices.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		}

		// Validate that development/adhoc profiles have devices
		var requiresDevices = settings.ProfileType == ProfileType.IOS_APP_DEVELOPMENT ||
		                      settings.ProfileType == ProfileType.IOS_APP_ADHOC ||
		                      settings.ProfileType == ProfileType.MAC_APP_DEVELOPMENT ||
		                      settings.ProfileType == ProfileType.TVOS_APP_DEVELOPMENT ||
		                      settings.ProfileType == ProfileType.TVOS_APP_ADHOC ||
		                      settings.ProfileType == ProfileType.MAC_CATALYST_APP_DEVELOPMENT;

		if (requiresDevices && (deviceIds == null || deviceIds.Length == 0))
		{
			AnsiConsole.MarkupLine("[yellow]Warning:[/] Development and Ad Hoc profiles typically require device IDs.");
			AnsiConsole.MarkupLine("[yellow]Continuing without devices...[/]");
		}

		try
		{
			// Create the profile
			var result = await appStoreConnect.CreateProfileAsync(
				settings.Name,
				settings.ProfileType,
				bundleIdResourceId,
				certificateIds,
				deviceIds);

			if (result.Data != null)
			{
				AnsiConsole.MarkupLine($"[green]✓ Profile created successfully:[/] {result.Data.Attributes.Name}");
				AnsiConsole.MarkupLine($"  UUID: {result.Data.Attributes.Uuid}");
				AnsiConsole.MarkupLine($"  Type: {result.Data.Attributes.ProfileType}");
				AnsiConsole.MarkupLine($"  Platform: {result.Data.Attributes.Platform}");

				// Optional: Download the profile
				if (settings.Download && !string.IsNullOrEmpty(result.Data.Attributes.ProfileContent))
				{
					var filename = $"{result.Data.Attributes.Name.Replace(" ", "_")}.mobileprovision";
					await File.WriteAllBytesAsync(filename, Convert.FromBase64String(result.Data.Attributes.ProfileContent));
					AnsiConsole.MarkupLine($"[green]✓ Downloaded to:[/] {Path.GetFullPath(filename)}");
				}

				return this.ExitCode(true);
			}
			else
			{
				AnsiConsole.MarkupLine("[red]Error:[/] Failed to create profile - no data returned.");
				return this.ExitCode(false);
			}
		}
		catch (AppleApiException apiEx)
		{
			AnsiConsole.MarkupLine($"[red]Failed to create profile (HTTP {apiEx.HttpStatusCode})[/]");
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

	private static Platform GetPlatformFromProfileType(ProfileType profileType)
	{
		return profileType switch
		{
			ProfileType.IOS_APP_DEVELOPMENT => Platform.IOS,
			ProfileType.IOS_APP_STORE => Platform.IOS,
			ProfileType.IOS_APP_ADHOC => Platform.IOS,
			ProfileType.IOS_APP_INHOUSE => Platform.IOS,
			ProfileType.MAC_APP_DEVELOPMENT => Platform.MAC_OS,
			ProfileType.MAC_APP_STORE => Platform.MAC_OS,
			ProfileType.MAC_APP_DIRECT => Platform.MAC_OS,
			ProfileType.MAC_CATALYST_APP_DEVELOPMENT => Platform.MAC_OS,
			ProfileType.MAC_CATALYST_APP_STORE => Platform.MAC_OS,
			ProfileType.MAC_CATALYST_APP_DIRECT => Platform.MAC_OS,
			ProfileType.TVOS_APP_DEVELOPMENT => Platform.IOS, // tvOS uses IOS platform in ASC
			ProfileType.TVOS_APP_STORE => Platform.IOS,
			ProfileType.TVOS_APP_ADHOC => Platform.IOS,
			ProfileType.TVOS_APP_INHOUSE => Platform.IOS,
			_ => Platform.IOS
		};
	}
}

public class CreateProvisioningProfileCommandSettings : AppStoreConnectApiCommandSettings
{
	[Description("Profile name")]
	[CommandOption("--name <name>")]
	public string Name { get; set; } = string.Empty;

	[Description("Profile type (e.g., IOS_APP_DEVELOPMENT, IOS_APP_STORE)")]
	[CommandOption("--type <type>")]
	[TypeConverter(typeof(StringEnumTypeConverter<ProfileType>))]
	public ProfileType ProfileType { get; set; } = ProfileType.IOS_APP_DEVELOPMENT;

	[Description("Bundle ID resource ID or bundle identifier (e.g., 'ABC123' or 'com.example.app')")]
	[CommandOption("--bundle-id <id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Certificate IDs (comma-separated)")]
	[CommandOption("--certificates <ids>")]
	public string Certificates { get; set; } = string.Empty;

	[Description("Device IDs (comma-separated, for dev/adhoc profiles)")]
	[CommandOption("--devices <ids>")]
	public string? Devices { get; set; }

	[Description("Include all enabled devices for the platform (alternative to --devices)")]
	[CommandOption("--all-devices")]
	public bool AllDevices { get; set; } = false;

	[Description("Download profile after creation")]
	[CommandOption("--download")]
	public bool Download { get; set; } = false;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrEmpty(Name))
		{
			return ValidationResult.Error("Profile name is required (--name)");
		}

		if (string.IsNullOrEmpty(BundleId))
		{
			return ValidationResult.Error("Bundle ID is required (--bundle-id)");
		}

		if (string.IsNullOrEmpty(Certificates))
		{
			return ValidationResult.Error("At least one certificate ID is required (--certificates)");
		}

		if (AllDevices && !string.IsNullOrEmpty(Devices))
		{
			return ValidationResult.Error("Cannot specify both --all-devices and --devices");
		}

		return base.Validate();
	}
}
