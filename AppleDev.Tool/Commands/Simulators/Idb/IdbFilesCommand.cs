using System.ComponentModel;
using AppleDev.FbIdb;
using AppleDev.FbIdb.Models;
using AppleDev.Tool.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class IdbFilesListCommand : AsyncCommand<IdbFilesListCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbFilesListCommandSettings settings, CancellationToken cancellationToken)
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
			var container = new FileContainer { BundleId = settings.BundleId };
			var files = await client.ListFilesAsync(settings.Path ?? "/", container, data.CancellationToken).ConfigureAwait(false);

			if (files.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]No files found[/]");
			}
			else
			{
				foreach (var file in files)
				{
					AnsiConsole.WriteLine(file.Path ?? "(unknown)");
				}
			}

			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbFilesListCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Path within the app's container (default: /)")]
	[CommandArgument(2, "[path]")]
	public string? Path { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		return ValidationResult.Success();
	}
}

public class IdbFilesPushCommand : AsyncCommand<IdbFilesPushCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbFilesPushCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		if (!File.Exists(settings.Source) && !Directory.Exists(settings.Source))
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] Source path not found: {settings.Source}");
			return this.ExitCode(false);
		}

		AnsiConsole.MarkupLine($"Pushing [cyan]{settings.Source}[/] to [cyan]{settings.Destination}[/]...");

		try
		{
			var container = new FileContainer { BundleId = settings.BundleId };
			await client.PushAsync(settings.Source, settings.Destination, container, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ File pushed successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbFilesPushCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Local source path")]
	[CommandArgument(2, "<source>")]
	public string Source { get; set; } = string.Empty;

	[Description("Remote destination path within the app's container")]
	[CommandArgument(3, "<destination>")]
	public string Destination { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		if (string.IsNullOrWhiteSpace(Source))
			return ValidationResult.Error("Source path is required");

		if (string.IsNullOrWhiteSpace(Destination))
			return ValidationResult.Error("Destination path is required");

		return ValidationResult.Success();
	}
}

public class IdbFilesPullCommand : AsyncCommand<IdbFilesPullCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbFilesPullCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Pulling [cyan]{settings.Source}[/] to [cyan]{settings.Destination}[/]...");

		try
		{
			var container = new FileContainer { BundleId = settings.BundleId };
			await client.PullAsync(settings.Source, settings.Destination, container, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ File pulled successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbFilesPullCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Remote source path within the app's container")]
	[CommandArgument(2, "<source>")]
	public string Source { get; set; } = string.Empty;

	[Description("Local destination path")]
	[CommandArgument(3, "<destination>")]
	public string Destination { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		if (string.IsNullOrWhiteSpace(Source))
			return ValidationResult.Error("Source path is required");

		if (string.IsNullOrWhiteSpace(Destination))
			return ValidationResult.Error("Destination path is required");

		return ValidationResult.Success();
	}
}

public class IdbFilesMkdirCommand : AsyncCommand<IdbFilesMkdirCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbFilesMkdirCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Creating directory [cyan]{settings.Path}[/]...");

		try
		{
			var container = new FileContainer { BundleId = settings.BundleId };
			await client.MakeDirAsync(settings.Path, container, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Directory created successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbFilesMkdirCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Path to create")]
	[CommandArgument(2, "<path>")]
	public string Path { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		if (string.IsNullOrWhiteSpace(Path))
			return ValidationResult.Error("Path is required");

		return ValidationResult.Success();
	}
}

public class IdbFilesRemoveCommand : AsyncCommand<IdbFilesRemoveCommandSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, IdbFilesRemoveCommandSettings settings, CancellationToken cancellationToken)
	{
		if (!IdbClientService.ValidateCanRun())
			return this.ExitCode(false);

		var data = context.GetData();

		await using var idbService = new IdbClientService();
		var client = await idbService.GetClientAsync(settings.Target, data.CancellationToken).ConfigureAwait(false);

		if (client is null)
			return this.ExitCode(false);

		AnsiConsole.MarkupLine($"Removing [cyan]{settings.Path}[/]...");

		try
		{
			var container = new FileContainer { BundleId = settings.BundleId };
			await client.RemoveAsync(new[] { settings.Path }, container, data.CancellationToken).ConfigureAwait(false);
			AnsiConsole.MarkupLine($"[green]✓ Removed successfully[/]");
			return this.ExitCode();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
			return this.ExitCode(false);
		}
	}
}

public class IdbFilesRemoveCommandSettings : CommandSettings
{
	[Description("Simulator UDID")]
	[CommandArgument(0, "<target>")]
	public string Target { get; set; } = string.Empty;

	[Description("Bundle identifier of the app")]
	[CommandArgument(1, "<bundle-id>")]
	public string BundleId { get; set; } = string.Empty;

	[Description("Path to remove")]
	[CommandArgument(2, "<path>")]
	public string Path { get; set; } = string.Empty;

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Target))
			return ValidationResult.Error("Target simulator UDID is required");

		if (string.IsNullOrWhiteSpace(BundleId))
			return ValidationResult.Error("Bundle identifier is required");

		if (string.IsNullOrWhiteSpace(Path))
			return ValidationResult.Error("Path is required");

		return ValidationResult.Success();
	}
}
