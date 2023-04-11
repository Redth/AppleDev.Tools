using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;

namespace AppleDev.Tool;

public class OutputFormatTypeConverter : System.ComponentModel.TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		var str = value.ToString();
		if (str?.Equals("json", StringComparison.OrdinalIgnoreCase) ?? false)
			return OutputFormat.Json;

		if (str?.Equals("xml", StringComparison.OrdinalIgnoreCase) ?? false)
			return OutputFormat.Xml;


		return OutputFormat.None;
	}
}
