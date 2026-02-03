using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AppleDev.FbIdb;

/// <summary>
/// Locates the idb_companion binary, supporting both bundled and custom paths.
/// </summary>
public class IdbCompanionLocator
{
	private readonly ILogger _logger;
	private readonly IdbCompanionOptions _options;

	/// <summary>
	/// Creates a new instance of IdbCompanionLocator.
	/// </summary>
	/// <param name="options">Configuration options.</param>
	/// <param name="logger">Optional logger.</param>
	public IdbCompanionLocator(IdbCompanionOptions? options = null, ILogger<IdbCompanionLocator>? logger = null)
	{
		_options = options ?? new IdbCompanionOptions();
		_logger = logger ?? NullLogger<IdbCompanionLocator>.Instance;
	}

	/// <summary>
	/// Locates the idb_companion binary.
	/// </summary>
	/// <returns>The path to the idb_companion binary.</returns>
	/// <exception cref="PlatformNotSupportedException">Thrown when not running on macOS.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the binary cannot be found.</exception>
	public string Locate()
	{
		ThrowIfNotMacOS();

		// Priority 1: Explicit path in options
		if (!string.IsNullOrEmpty(_options.CompanionPath))
		{
			_logger.LogDebug("Using companion path from options: {Path}", _options.CompanionPath);
			if (File.Exists(_options.CompanionPath))
			{
				return _options.CompanionPath;
			}
			throw new FileNotFoundException($"Specified idb_companion not found at: {_options.CompanionPath}");
		}

		// Priority 2: Environment variable
		var envPath = Environment.GetEnvironmentVariable(IdbCompanionOptions.CompanionPathEnvironmentVariable);
		if (!string.IsNullOrEmpty(envPath))
		{
			_logger.LogDebug("Using companion path from environment variable: {Path}", envPath);
			if (File.Exists(envPath))
			{
				return envPath;
			}
			throw new FileNotFoundException($"idb_companion from environment variable not found at: {envPath}");
		}

		// Priority 3: Bundled binary in runtimes folder
		var bundledPath = GetBundledBinaryPath();
		if (!string.IsNullOrEmpty(bundledPath) && File.Exists(bundledPath))
		{
			_logger.LogDebug("Using bundled companion: {Path}", bundledPath);
			return bundledPath;
		}

		// Priority 4: Check common installation paths
		var commonPaths = new[]
		{
			"/usr/local/bin/idb_companion",
			"/opt/homebrew/bin/idb_companion",
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".idb", "idb_companion")
		};

		var foundPath = commonPaths.Where(File.Exists).FirstOrDefault();
		if (foundPath is not null)
		{
			_logger.LogDebug("Found companion at common path: {Path}", foundPath);
			return foundPath;
		}

		throw new FileNotFoundException(
			"Could not locate idb_companion binary. Please either:\n" +
			"1. Install via Homebrew: brew tap facebook/fb && brew install idb-companion\n" +
			$"2. Set the {IdbCompanionOptions.CompanionPathEnvironmentVariable} environment variable\n" +
			"3. Provide the path via IdbCompanionOptions.CompanionPath");
	}

	/// <summary>
	/// Gets the path to the bundled binary based on the current runtime identifier.
	/// </summary>
	private string? GetBundledBinaryPath()
	{
		var assemblyLocation = Assembly.GetExecutingAssembly().Location;
		var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

		if (string.IsNullOrEmpty(assemblyDirectory))
			return null;

		// Determine the RID
		var rid = GetRuntimeIdentifier();
		
		// Check in the runtimes folder structure
		var runtimesPath = Path.Combine(assemblyDirectory, "runtimes", rid, "native", "idb_companion");
		if (File.Exists(runtimesPath))
		{
			return runtimesPath;
		}

		// Also check directly in the assembly directory (development scenario)
		var directPath = Path.Combine(assemblyDirectory, "idb_companion");
		if (File.Exists(directPath))
		{
			return directPath;
		}

		// Check parent directories for development builds
		var projectPath = Path.Combine(assemblyDirectory, "..", "..", "..", "..", "runtimes", rid, "native", "idb_companion");
		if (File.Exists(projectPath))
		{
			return Path.GetFullPath(projectPath);
		}

		return null;
	}

	/// <summary>
	/// Gets the runtime identifier for the current platform.
	/// </summary>
	private static string GetRuntimeIdentifier()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return RuntimeInformation.ProcessArchitecture switch
			{
				Architecture.Arm64 => "osx-arm64",
				Architecture.X64 => "osx-x64",
				_ => "osx-arm64" // Default to arm64 for newer Macs
			};
		}

		// IDB only supports macOS
		return "osx-arm64";
	}

	/// <summary>
	/// Throws if not running on macOS.
	/// </summary>
	private static void ThrowIfNotMacOS()
	{
		if (!OperatingSystem.IsMacOS())
		{
			throw new PlatformNotSupportedException(
				"IDB (iOS Development Bridge) is only supported on macOS. " +
				"The idb_companion binary requires macOS and Xcode to function.");
		}
	}

	/// <summary>
	/// Checks if the idb_companion binary can be located.
	/// </summary>
	/// <returns>True if the binary exists, false otherwise.</returns>
	public bool CanLocate()
	{
		try
		{
			Locate();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Attempts to find the idb_companion binary.
	/// </summary>
	/// <param name="path">The path to the binary if found.</param>
	/// <returns>True if found, false otherwise.</returns>
	public static bool TryFindIdbCompanion(out string? path)
	{
		path = null;

		if (!OperatingSystem.IsMacOS())
			return false;

		var locator = new IdbCompanionLocator();
		try
		{
			path = locator.Locate();
			return true;
		}
		catch
		{
			return false;
		}
	}
}
