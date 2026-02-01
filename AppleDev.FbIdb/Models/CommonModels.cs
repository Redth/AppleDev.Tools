namespace AppleDev.FbIdb.Models;

/// <summary>
/// Result of an app installation.
/// </summary>
public class InstallResult
{
	/// <summary>
	/// The installed app or bundle name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The UUID of the installed item.
	/// </summary>
	public string Uuid { get; set; } = string.Empty;
}

/// <summary>
/// Installation destination type.
/// </summary>
public enum InstallDestination
{
	/// <summary>
	/// Install as an application.
	/// </summary>
	App = 0,

	/// <summary>
	/// Install as an XCTest bundle.
	/// </summary>
	Xctest = 1,

	/// <summary>
	/// Install as a dynamic library.
	/// </summary>
	Dylib = 2,

	/// <summary>
	/// Install as debug symbols.
	/// </summary>
	Dsym = 3,

	/// <summary>
	/// Install as a framework.
	/// </summary>
	Framework = 4
}

/// <summary>
/// App launch options.
/// </summary>
public class LaunchOptions
{
	/// <summary>
	/// Environment variables to set.
	/// </summary>
	public IDictionary<string, string>? Environment { get; set; }

	/// <summary>
	/// Arguments to pass to the app.
	/// </summary>
	public IList<string>? Arguments { get; set; }

	/// <summary>
	/// Whether to foreground the app if already running.
	/// </summary>
	public bool ForegroundIfRunning { get; set; }

	/// <summary>
	/// Wait for the app to launch.
	/// </summary>
	public bool WaitFor { get; set; }

	/// <summary>
	/// Wait for debugger to attach.
	/// </summary>
	public bool WaitForDebugger { get; set; }
}

/// <summary>
/// App launch result with debugger information.
/// </summary>
public class LaunchResult
{
	/// <summary>
	/// The process ID of the launched app.
	/// </summary>
	public ulong Pid { get; set; }

	/// <summary>
	/// The debug server host.
	/// </summary>
	public string? DebugHost { get; set; }

	/// <summary>
	/// The debug server port.
	/// </summary>
	public ulong DebugPort { get; set; }
}

/// <summary>
/// Permission types that can be approved or revoked.
/// </summary>
public enum Permission
{
	/// <summary>
	/// Photos library access.
	/// </summary>
	Photos = 0,

	/// <summary>
	/// Camera access.
	/// </summary>
	Camera = 1,

	/// <summary>
	/// Contacts access.
	/// </summary>
	Contacts = 2,

	/// <summary>
	/// URL scheme access.
	/// </summary>
	Url = 3,

	/// <summary>
	/// Location services.
	/// </summary>
	Location = 4,

	/// <summary>
	/// Notifications.
	/// </summary>
	Notification = 5,

	/// <summary>
	/// Microphone access.
	/// </summary>
	Microphone = 6
}

/// <summary>
/// Location coordinates.
/// </summary>
public class GeoLocation
{
	/// <summary>
	/// Latitude in degrees.
	/// </summary>
	public double Latitude { get; set; }

	/// <summary>
	/// Longitude in degrees.
	/// </summary>
	public double Longitude { get; set; }

	/// <summary>
	/// Creates a new location.
	/// </summary>
	public GeoLocation(double latitude, double longitude)
	{
		Latitude = latitude;
		Longitude = longitude;
	}
}

/// <summary>
/// Screenshot result.
/// </summary>
public class Screenshot
{
	/// <summary>
	/// The image data.
	/// </summary>
	public byte[] ImageData { get; set; } = Array.Empty<byte>();

	/// <summary>
	/// The image format (e.g., "png", "jpeg").
	/// </summary>
	public string ImageFormat { get; set; } = string.Empty;
}

/// <summary>
/// Video stream format.
/// </summary>
public enum VideoStreamFormat
{
	/// <summary>
	/// H.264 video.
	/// </summary>
	H264 = 0,

	/// <summary>
	/// RGBA raw video.
	/// </summary>
	Rgba = 1,

	/// <summary>
	/// MJPEG video.
	/// </summary>
	Mjpeg = 2,

	/// <summary>
	/// Minicap format.
	/// </summary>
	Minicap = 3,

	/// <summary>
	/// I420 YUV format.
	/// </summary>
	I420 = 4
}

/// <summary>
/// Video recording options.
/// </summary>
public class VideoRecordOptions
{
	/// <summary>
	/// Frames per second.
	/// </summary>
	public ulong Fps { get; set; } = 30;

	/// <summary>
	/// Video format.
	/// </summary>
	public VideoStreamFormat Format { get; set; } = VideoStreamFormat.H264;

	/// <summary>
	/// Compression quality (0.0 to 1.0).
	/// </summary>
	public double CompressionQuality { get; set; } = 0.8;

	/// <summary>
	/// Scale factor.
	/// </summary>
	public double ScaleFactor { get; set; } = 1.0;
}

/// <summary>
/// Accessibility information.
/// </summary>
public class AccessibilityInfo
{
	/// <summary>
	/// The accessibility tree as JSON.
	/// </summary>
	public string Json { get; set; } = string.Empty;
}

/// <summary>
/// Accessibility info format.
/// </summary>
public enum AccessibilityFormat
{
	/// <summary>
	/// Legacy flat format.
	/// </summary>
	Legacy = 0,

	/// <summary>
	/// Nested tree format.
	/// </summary>
	Nested = 1
}

/// <summary>
/// Point coordinates for touch/tap operations.
/// </summary>
public class Point
{
	/// <summary>
	/// X coordinate.
	/// </summary>
	public double X { get; set; }

	/// <summary>
	/// Y coordinate.
	/// </summary>
	public double Y { get; set; }

	/// <summary>
	/// Creates a new point.
	/// </summary>
	public Point(double x, double y)
	{
		X = x;
		Y = y;
	}
}

/// <summary>
/// Hardware button types.
/// </summary>
public enum HardwareButton
{
	/// <summary>
	/// Apple Pay button.
	/// </summary>
	ApplePay = 0,

	/// <summary>
	/// Home button.
	/// </summary>
	Home = 1,

	/// <summary>
	/// Lock/power button.
	/// </summary>
	Lock = 2,

	/// <summary>
	/// Side button.
	/// </summary>
	SideButton = 3,

	/// <summary>
	/// Siri button.
	/// </summary>
	Siri = 4
}

/// <summary>
/// Log source.
/// </summary>
public enum LogSource
{
	/// <summary>
	/// Target device/simulator logs.
	/// </summary>
	Target = 0,

	/// <summary>
	/// Companion process logs.
	/// </summary>
	Companion = 1
}

/// <summary>
/// Companion information.
/// </summary>
public class CompanionInfo
{
	/// <summary>
	/// The target UDID.
	/// </summary>
	public string Udid { get; set; } = string.Empty;

	/// <summary>
	/// Whether this is a local connection.
	/// </summary>
	public bool IsLocal { get; set; }
}
