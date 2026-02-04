using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class CopyClipboardSimulatorCommand : AsyncCommand<CopyClipboardSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, CopyClipboardSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		AnsiConsole.MarkupLine($"Copying text to clipboard on simulator [cyan]{settings.Target}[/]...");

		var success = await simctl.CopyToPasteboardAsync(settings.Target, settings.Text, data.CancellationToken).ConfigureAwait(false);

		if (success)
		{
			AnsiConsole.MarkupLine($"[green]✓ Text copied to clipboard successfully[/]");
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to copy text to clipboard");
			return this.ExitCode(false);
		}
	}
}

public class CopyClipboardSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Text to copy to the clipboard")]
	[CommandArgument(1, "<text>")]
	public string Text { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		if (string.IsNullOrEmpty(Text))
			return ValidationResult.Error("Text is required");

		return ValidationResult.Success();
	}
}

public class PasteClipboardSimulatorCommand : AsyncCommand<PasteClipboardSimulatorCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, PasteClipboardSimulatorCommandSettings settings, CancellationToken cancellationToken)
	{
		var data = context.GetData();
		var simctl = new SimCtl();

		var content = await simctl.GetPasteboardAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (content != null)
		{
			if (settings.Raw)
			{
				Console.Write(content);
			}
			else
			{
				AnsiConsole.MarkupLine($"[green]Clipboard contents:[/]");
				AnsiConsole.WriteLine(content);
			}
			return this.ExitCode();
		}
		else
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Failed to get clipboard contents");
			return this.ExitCode(false);
		}
	}
}

public class PasteClipboardSimulatorCommandSettings : CommandSettings
{
	[Description("Simulator UDID or name (e.g., 'booted')")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Output raw text without formatting")]
	[CommandOption("-r|--raw")]
	public bool Raw { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator is required");

		return ValidationResult.Success();
	}
}
