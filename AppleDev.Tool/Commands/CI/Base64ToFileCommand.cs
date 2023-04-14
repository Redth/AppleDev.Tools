using System.ComponentModel;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class Base64ToFileCommand : AsyncCommand<Base64ToFileCommandSettings>
{
	public override Task<int> ExecuteAsync(CommandContext context, Base64ToFileCommandSettings settings)
	{
		var b = Convert.FromBase64String(settings.Base64);
		
		if (settings.OutputFile is not null)
			File.WriteAllBytes(settings.OutputFile.FullName, b);
		
		return Task.FromResult(this.ExitCode());
	}

	string FromTextFile(FileInfo file)
	{
		var key = File.ReadAllText(file.FullName) ?? string.Empty;

		return Regex.Replace(key, @"\s+", "");
	}

	string FromBinaryFile(FileInfo file)
	{
		var c = File.ReadAllBytes(file.FullName);

		var b64 = Convert.ToBase64String(c);

		return Regex.Replace(b64, @"\s+", "");
	}
}
public class Base64ToFileCommandSettings : CommandSettings
{
	[Description("Base64 string value or environment variable name with base64 string value")]
	[CommandOption("--base64")]
	public string Base64 { get; set; } = string.Empty;
	
	[Description("Output file")]
	[CommandOption("--output-file <file>")]
	[TypeConverter(typeof(FileInfoTypeConverter))]
	public FileInfo? OutputFile { get; set; }

	public override ValidationResult Validate()
	{
		if (string.IsNullOrWhiteSpace(Base64))
			return ValidationResult.Error("--base64 value is required");

		try
		{
			var _ = Convert.FromBase64String(Base64);
		}
		catch
		{
			return ValidationResult.Error("--base64 value is invalid");
		}
		
		if (OutputFile is null)
			return ValidationResult.Error("--output-file is required");
		
		return base.Validate();
	}
}