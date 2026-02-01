namespace AppleDev.FbIdb.Models;

/// <summary>
/// Describes a target (simulator or device).
/// </summary>
public class TargetDescription
{
	/// <summary>
	/// The unique device identifier.
	/// </summary>
	public string Udid { get; set; } = string.Empty;

	/// <summary>
	/// The device name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The current state (e.g., "Booted", "Shutdown").
	/// </summary>
	public string State { get; set; } = string.Empty;

	/// <summary>
	/// The target type (e.g., "simulator", "device").
	/// </summary>
	public string TargetType { get; set; } = string.Empty;

	/// <summary>
	/// The OS version.
	/// </summary>
	public string OsVersion { get; set; } = string.Empty;

	/// <summary>
	/// The CPU architecture.
	/// </summary>
	public string Architecture { get; set; } = string.Empty;

	/// <summary>
	/// Screen dimensions.
	/// </summary>
	public ScreenDimensions? Screen { get; set; }
}

/// <summary>
/// Screen dimensions information.
/// </summary>
public class ScreenDimensions
{
	/// <summary>
	/// Width in pixels.
	/// </summary>
	public ulong Width { get; set; }

	/// <summary>
	/// Height in pixels.
	/// </summary>
	public ulong Height { get; set; }

	/// <summary>
	/// Screen density.
	/// </summary>
	public double Density { get; set; }

	/// <summary>
	/// Width in points.
	/// </summary>
	public ulong WidthPoints { get; set; }

	/// <summary>
	/// Height in points.
	/// </summary>
	public ulong HeightPoints { get; set; }
}
