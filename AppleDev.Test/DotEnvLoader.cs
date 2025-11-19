using System;
using System.IO;

namespace AppleDev.Test;

/// <summary>
/// Loads environment variables from a .env file in the project root
/// </summary>
public static class DotEnvLoader
{
	private static bool _loaded = false;
	private static readonly object _lock = new object();

	/// <summary>
	/// Loads the .env file from the project root directory
	/// </summary>
	public static void Load()
	{
		lock (_lock)
		{
			if (_loaded)
				return;

			_loaded = true;

			// Navigate up from the test assembly directory to find the .env file
			var directory = new DirectoryInfo(TestsBase.TestAssemblyDirectory);
			while (directory != null && directory.Parent != null)
			{
				var envFilePath = Path.Combine(directory.FullName, ".env");
				if (File.Exists(envFilePath))
				{
					LoadFromFile(envFilePath);
					return;
				}
				directory = directory.Parent;
			}
		}
	}

	private static void LoadFromFile(string filePath)
	{
		try
		{
			foreach (var line in File.ReadAllLines(filePath))
			{
				// Skip empty lines and comments
				var trimmedLine = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
					continue;

				// Parse KEY=VALUE format
				var parts = trimmedLine.Split('=', 2);
				if (parts.Length == 2)
				{
					var key = parts[0].Trim();
					var value = parts[1].Trim();

					// Remove quotes if present
					if (value.StartsWith("\"") && value.EndsWith("\""))
						value = value.Substring(1, value.Length - 2);
					else if (value.StartsWith("'") && value.EndsWith("'"))
						value = value.Substring(1, value.Length - 2);

					// Only set if not already set (environment variables take precedence)
					if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
					{
						Environment.SetEnvironmentVariable(key, value);
					}
				}
			}
		}
		catch (Exception ex)
		{
			// Silently fail - .env is optional
			Console.WriteLine($"Warning: Failed to load .env file: {ex.Message}");
		}
	}
}

