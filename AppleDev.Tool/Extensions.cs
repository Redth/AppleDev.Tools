using AppleDev.Tool.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;
using System.Runtime.Serialization;

public enum CIType
{
	None,
	GitHub,
	Azure,
}

static class Extensions
{
	public static CommandContextData GetData(this CommandContext ctx)
		=> (ctx.Data as CommandContextData) ?? new CommandContextData();

	public static int ExitCode(this ICommand _, bool success = true, int errorExitCode = 1)
		=> success ? 0 : errorExitCode;


	public static CIType GetCI(this ICommand _)
	{
		if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
			return CIType.GitHub;
		else if (Environment.GetEnvironmentVariable("TF_BUILD")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
			return CIType.Azure;
		else
			return CIType.None;
	}

	public static void SetOutputVariable(this ICommand _, string key, string value, bool isSecret = false)
	{
		var ci = _.GetCI();

		if (ci == CIType.GitHub)
		{
			var file = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
			var content = System.IO.File.ReadAllText(file);
			System.IO.File.WriteAllText(file, content + $"{key}={value}" + "\n");
		}
		else if (ci == CIType.Azure)
		{
			var secret = isSecret ? ";isSecret=true" : string.Empty;
			AnsiConsole.WriteLine($"##vso[task.setvariable variable={key};isoutput=true{secret}]{value}");
		}
	}

	public static string? GetStringFromFileOrEnvironmentOrString(this CommandSettings _, string input)
	{
		try
		{
			if (File.Exists(input))
				return File.ReadAllText(input);
		}
		catch { }

		try
		{
			var envVar = Environment.GetEnvironmentVariable(input);
			if (!string.IsNullOrWhiteSpace(envVar))
				return envVar;
		}
		catch { }

		try
		{
			return input;
		}
		catch { }

		return null;
	}

	public static byte[]? GetBytesFromFileOrEnvironmentOrBase64String(this CommandSettings _, string input)
	{
		try {
			if (File.Exists(input))
				return File.ReadAllBytes(input);
		} catch { }

		try
		{
			var envVar = Environment.GetEnvironmentVariable(input);
			if (!string.IsNullOrWhiteSpace(envVar))
				return Convert.FromBase64String(envVar);
		} catch { }

		try
		{
			return Convert.FromBase64String(input);
		}
		catch { }

		return null;
	}


	public static FileInfo GetOutputFile(this IOutputCommandSettings command, string prefix = "", string extension = "")
	{
		if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
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