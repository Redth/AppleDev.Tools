using AppleDev.Tool.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;
using System.Runtime.Serialization;

static class Extensions
{
    public static CommandContextData GetData(this CommandContext ctx)
        => (ctx.Data as CommandContextData) ?? new CommandContextData();

    public static int ExitCode(this ICommand _, bool success = true, int errorExitCode = 1)
        => success ? 0 : errorExitCode;


	public static FileInfo GetOutputFile(this IOutputCommandSettings command, string prefix = "", string extension = "")
	{
		if (!string.IsNullOrEmpty(extension))
			extension = "." + extension;

		if (!string.IsNullOrEmpty(prefix))
			prefix += "-";

		// Output specified
		if (!string.IsNullOrWhiteSpace(command.Output))
		{
			// Output is a directory
			if (Path.EndsInDirectorySeparator(command.Output))
			{
				// Is the directory valid?
				var invalidPathChars = Path.GetInvalidPathChars();
				if (!command.Output.Any(c => invalidPathChars.Contains(c)))
					return new FileInfo(Path.Combine(command.Output, $"{prefix}{DateTime.UtcNow.ToLongTimeString()}{extension}"));
			}
			else // Output is a file
			{
				var invalidFileChars = Path.GetInvalidFileNameChars();
				if (!command.Output.Any(c => invalidFileChars.Contains(c)))
					return new FileInfo(command.Output);
			}
		}

		// Output wasn't specified, or was not valid, use a temp file
		var tmp = new FileInfo(Path.GetTempFileName());

		return new FileInfo(Path.Combine(tmp.Directory!.FullName, $"{prefix}{tmp.Name}{extension}"));
	}

	public static bool IsValid(this IOutputCommandSettings command, out ValidationResult validationResult)
	{
		if (!string.IsNullOrWhiteSpace(command.Output))
		{
			if (Path.EndsInDirectorySeparator(command.Output))
			{
				var invalidPathChars = Path.GetInvalidPathChars();
				if (command.Output.Any(c => invalidPathChars.Contains(c)))
				{
					validationResult = ValidationResult.Error("Invalid path specified for --output");
					return false;
				}
			}
			else
			{
				var invalidFileChars = Path.GetInvalidFileNameChars();
				if (command.Output.Any(c => invalidFileChars.Contains(c)))
				{
					validationResult = ValidationResult.Error("Invalid path specified for --output");
					return false;
				}
			}
		}

		validationResult = ValidationResult.Success();
		return true;
	}


	public static string? GetEnumMemberValue<T>(this T? value) where T : struct
	{
		if (value is null)
			return null;

		return typeof(T)
			?.GetTypeInfo()
			?.DeclaredMembers?.SingleOrDefault(x => x is not null && x.Name is not null && x.Name.Equals(value.ToString()))
			?.GetCustomAttribute<EnumMemberAttribute>()
			?.Value?.ToString()
				?? value.ToString();
	}

	public static string? GetEnumMemberValue<T>(this T value) where T : struct
	{
		return typeof(T)
			?.GetTypeInfo()
			?.DeclaredMembers?.SingleOrDefault(x => x is not null && x.Name is not null && x.Name.Equals(value.ToString()))
			?.GetCustomAttribute<EnumMemberAttribute>()
			?.Value?.ToString()
				?? value.ToString();
	}
}