using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbButtonCommand : AsyncCommand<IdbButtonCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbButtonCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Pressing [cyan]{settings.Button}[/] button on [cyan]{settings.Target}[/]...");

		try
		{
			var button = settings.Button.ToLowerInvariant() switch
			{
				"apple_pay" => HardwareButton.ApplePay,
				"home" => HardwareButton.Home,
				"lock" => HardwareButton.Lock,
				"side_button" => HardwareButton.SideButton,
				"siri" => HardwareButton.Siri,
				_ => throw new ArgumentException($"Unknown button: {settings.Button}")
			};

			await client.PressButtonAsync(button, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Button pressed successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbButtonCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Button to press: apple_pay, home, lock, side_button, siri")]
	[CommandArgument(1, "<button>")]
	public string Button { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(Button))
			return ValidationResult.Error("Button name is required");

		var validButtons = new[] { "apple_pay", "home", "lock", "side_button", "siri" };
		if (!validButtons.Contains(Button.ToLowerInvariant()))
			return ValidationResult.Error($"Invalid button. Valid options: {string.Join(", ", validButtons)}");

		return ValidationResult.Success();
	}
}
