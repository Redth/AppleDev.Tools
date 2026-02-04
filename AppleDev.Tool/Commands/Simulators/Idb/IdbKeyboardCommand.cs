using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbKeyboardCommand : AsyncCommand<IdbKeyboardCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbKeyboardCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		var action = settings.Enable ? "Enabling" : "Disabling";
		AnsiConsole.MarkupLine($"{action} hardware keyboard on [cyan]{settings.Target}[/]...");

		try
		{
			await client.SetHardwareKeyboardAsync(settings.Enable, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Hardware keyboard {(settings.Enable ? "enabled" : "disabled")}[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbKeyboardCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Enable hardware keyboard")]
	[CommandOption("-e|--enable")]
	public bool Enable { get; set; }

	[Description("Disable hardware keyboard")]
	[CommandOption("-d|--disable")]
	public bool Disable { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (Enable == Disable)
			return ValidationResult.Error("Specify either --enable or --disable");

		return ValidationResult.Success();
	}
}
