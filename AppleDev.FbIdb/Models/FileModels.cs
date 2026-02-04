namespace AppleDev.FbIdb.Models;

/// <summary>
/// File information from file operations.
/// </summary>
public class IdbFileInfo
{
	/// <summary>
	/// The file path.
	/// </summary>
	public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Directory listing result.
/// </summary>
public class DirectoryListing
{
	/// <summary>
	/// The parent directory.
	/// </summary>
	public IdbFileInfo? Parent { get; set; }

	/// <summary>
	/// Files in the directory.
	/// </summary>
	public IReadOnlyList<IdbFileInfo> Files { get; set; } = Array.Empty<IdbFileInfo>();
}

/// <summary>
/// File container types for file operations.
/// </summary>
public enum FileContainerKind
{
	/// <summary>
	/// No specific container.
	/// </summary>
	None = 0,

	/// <summary>
	/// Application container.
	/// </summary>
	Application = 1,

	/// <summary>
	/// Root filesystem.
	/// </summary>
	Root = 2,

	/// <summary>
	/// Media directory.
	/// </summary>
	Media = 3,

	/// <summary>
	/// Crashes directory.
	/// </summary>
	Crashes = 4,

	/// <summary>
	/// Provisioning profiles.
	/// </summary>
	ProvisioningProfiles = 5,

	/// <summary>
	/// MDM profiles.
	/// </summary>
	MdmProfiles = 6,

	/// <summary>
	/// Springboard icons.
	/// </summary>
	SpringboardIcons = 7,

	/// <summary>
	/// Wallpaper.
	/// </summary>
	Wallpaper = 8,

	/// <summary>
	/// Disk images.
	/// </summary>
	DiskImages = 9,

	/// <summary>
	/// Group container.
	/// </summary>
	GroupContainer = 10,

	/// <summary>
	/// Application container (by bundle ID).
	/// </summary>
	ApplicationContainer = 11,

	/// <summary>
	/// Auxiliary files.
	/// </summary>
	Auxiliary = 12,

	/// <summary>
	/// XCTest bundles.
	/// </summary>
	Xctest = 13,

	/// <summary>
	/// Dynamic libraries.
	/// </summary>
	Dylib = 14,

	/// <summary>
	/// Debug symbols.
	/// </summary>
	Dsym = 15,

	/// <summary>
	/// Frameworks.
	/// </summary>
	Framework = 16,

	/// <summary>
	/// Symbols.
	/// </summary>
	Symbols = 17
}

/// <summary>
/// File container specification for file operations.
/// </summary>
public class FileContainer
{
	/// <summary>
	/// The kind of container.
	/// </summary>
	public FileContainerKind Kind { get; set; }

	/// <summary>
	/// The bundle identifier (for app-specific containers).
	/// </summary>
	public string? BundleId { get; set; }

	/// <summary>
	/// Creates a container for the specified kind.
	/// </summary>
	public static FileContainer Create(FileContainerKind kind, string? bundleId = null)
		=> new() { Kind = kind, BundleId = bundleId };

	/// <summary>
	/// Creates an application container.
	/// </summary>
	public static FileContainer App(string bundleId)
		=> new() { Kind = FileContainerKind.Application, BundleId = bundleId };
}
