using System.ComponentModel;
using Spectre.Console.Cli;

namespace AppleDev.Tool.Commands;

public class FormattableOutputCommandSettings : CommandSettings
{
    [Description("Output Format")]
    [CommandOption("-f|--format")]
    [DefaultValue(OutputFormat.None)]
    [TypeConverter(typeof(StringEnumTypeConverter<OutputFormat>))]
    public OutputFormat Format { get; set; }
    
    [Description("Verbose")]
    [CommandOption("-v|--verbose")]
    [DefaultValue(false)]
    public bool Verbose { get; set; }
}