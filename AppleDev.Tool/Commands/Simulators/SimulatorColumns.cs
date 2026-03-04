using Col = AppleDev.Tool.ColumnInfo<AppleDev.SimCtlDevice>;

namespace AppleDev.Tool.Commands;

static class SimulatorColumns
{
	internal static Col[] ForDevice(bool verbose = false)
	{
		var columns = new List<Col>
		{
			new("Name", d => d.Name),
			new("UDID", d => d.Udid),
			new("State", d => d.State),
			new("Device Type", d => d.DeviceType?.Name ?? d.DeviceTypeIdentifier),
			new("Runtime", d => d.Runtime?.Name),
			new("Runtime Version", d => d.Runtime?.Version, true),
			new("Data Path", d => d.DataPath, true),
			new("Log Path", d => d.LogPath, true),
		};

		return columns.ToArray();
	}
}
