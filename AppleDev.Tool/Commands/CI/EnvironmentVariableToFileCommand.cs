using System.ComponentModel;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class EnvironmentVariableToFileCommand : AsyncCommand<EnvironmentVariableToFileCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, EnvironmentVariableToFileCommandSettings settings)
    {
        var v = Environment.GetEnvironmentVariable(settings.EnvironmentVariable);
        
        if (!string.IsNullOrEmpty(v) && settings.OutputFile is not null)
            File.WriteAllText(settings.OutputFile.FullName, v);
        
        return Task.FromResult(this.ExitCode());
    }
}
public class EnvironmentVariableToFileCommandSettings : CommandSettings
{
    [Description("Environment variable name")]
    [CommandOption("-e|--environment-variable|--env-var")]
    public string EnvironmentVariable { get; set; } = string.Empty;
	
    [Description("Output file")]
    [CommandOption("--output-file <file>")]
    [TypeConverter(typeof(FileInfoTypeConverter))]
    public FileInfo? OutputFile { get; set; }

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(EnvironmentVariable))
            return ValidationResult.Error("--env-var value is required");
		
        if (OutputFile is null)
            return ValidationResult.Error("--output-file is required");
		
        return base.Validate();
    }
}