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

public class StringEnumTypeConverter<TEnum> : System.ComponentModel.TypeConverter where TEnum : struct
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		var str = value.ToString();

		if (Enum.TryParse<TEnum>(str, true, out var e))
		{
			return e;
		}

		return default(TEnum);
	}
}

public class FileInfoTypeConverter : System.ComponentModel.TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		try
		{
			return new FileInfo(value.ToString());
		} catch { }

		return null;
	}
}

public class DirectoryInfoTypeConverter : System.ComponentModel.TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		try
		{
			return new DirectoryInfo(value.ToString());
		} catch { }

		return null;
	}
}
