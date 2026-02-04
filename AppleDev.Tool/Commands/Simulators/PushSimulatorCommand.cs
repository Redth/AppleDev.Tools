using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class PushSimulatorCommand : AsyncCommand<PushSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, PushSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		string? payloadJson = null;

		// Determine payload source
		if (!string.IsNullOrEmpty(settings.PayloadFile))
		{
			if (!File.Exists(settings.PayloadFile))
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] Payload file not found: {settings.PayloadFile}");
				return this.ExitCode(false);
			}
			payloadJson = await File.ReadAllTextAsync(settings.PayloadFile, data.CancellationToken).ConfigureAwait(false);
		}
		else if (!string.IsNullOrEmpty(settings.Json))
		{
			payloadJson = settings.Json;
		}
		else if (!string.IsNullOrEmpty(settings.Title) || !string.IsNullOrEmpty(settings.Body))
		{
			// Build a simple notification payload
			var payload = new Dictionary<string, object>
			{
				["aps"] = new Dictionary<string, object>
				{
					["alert"] = new Dictionary<string, object?>
					{
						["title"] = settings.Title,
						["body"] = settings.Body
					}.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
				}
			};

			if (settings.Badge.HasValue)
			{
				((Dictionary<string, object>)payload["aps"])["badge"] = settings.Badge.Value;
			}

			payloadJson = JsonSerializer.Serialize(payload);
		}

		if (string.IsNullOrEmpty(payloadJson))
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] No payload specified. Use --payload, --json, or --title/--body options.");
			return this.ExitCode(false);
		}

		AnsiConsole.MarkupLine($"Sending push notification to [cyan]{settings.BundleId}[/] on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.SendPushNotificationAsync(settings.Target, settings.BundleId, payloadJson, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Push notification sent successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to send push notification");
			return this.ExitCode(false);
		}
	}
}

public class PushSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app to receive the notification")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Path to JSON payload file")]
	[CommandOption("-p|--payload <PATH>")]
	public string? PayloadFile { get; set; }

	[Description("Inline JSON payload string")]
	[CommandOption("-j|--json <JSON>")]
	public string? Json { get; set; }

	[Description("Notification title (for simple notifications)")]
	[CommandOption("--title <TITLE>")]
	public string? Title { get; set; }

	[Description("Notification body (for simple notifications)")]
	[CommandOption("--body <BODY>")]
	public string? Body { get; set; }

	[Description("Badge count")]
	[CommandOption("--badge <NUMBER>")]
	public int? Badge { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		// At least one payload method must be specified
		var hasPayload = !string.IsNullOrEmpty(PayloadFile) ||
		                 !string.IsNullOrEmpty(Json) ||
		                 !string.IsNullOrEmpty(Title) ||
		                 !string.IsNullOrEmpty(Body);

		if (!hasPayload)
			return ValidationResult.Error("A payload is required. Use --payload, --json, or --title/--body");

		return ValidationResult.Success();
	}
}
