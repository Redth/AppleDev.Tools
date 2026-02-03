using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbKeyCommand : AsyncCommand<IdbKeyCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbKeyCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		try
		{
			if (settings.KeyCode.HasValue)
			{
				AnsiConsole.MarkupLine($"Sending key code [cyan]{settings.KeyCode}[/] on [cyan]{settings.Target}[/]...");
				await client.SendKeyAsync((ulong)settings.KeyCode.Value, data.CancellationToken).ConfigureAwait(false);
			}
			else
			{
				AnsiConsole.MarkupLine($"[red]Error:[/] --key-code must be specified");
				return this.ExitCode(false);
			}

			AnsiConsole.MarkupLine($"[green]✓ Key input sent successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbKeyCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Key code to send")]
	[CommandArgument(1, "<key-code>")]
	public ulong? KeyCode { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (!KeyCode.HasValue)
			return ValidationResult.Error("Key code is required");

		return ValidationResult.Success();
	}
}
