using System.ComponentModel;

namespace AppleDev.Tool;

[TypeConverter(typeof(OutputFormatTypeConverter))]
public enum OutputFormat
{
	None,
	Json,
	Xml
}
