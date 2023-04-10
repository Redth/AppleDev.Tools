using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace AppleDev.Tool;

public class EnumMemberTypeConverter<T> : System.ComponentModel.TypeConverter
	where T: struct
{
	public EnumMemberTypeConverter()
	{
		// Loads all the enum members with the [EnumMemberValue] attribute
		// and keeps a mapping of the attribute value to the enum member name
		var members = typeof(T).GetTypeInfo().DeclaredMembers;

		foreach (var m in members)
		{
			var memberValue = m.GetCustomAttribute<EnumMemberAttribute>()?.Value;

			if (!string.IsNullOrEmpty(memberValue))
			{
				memberValueMappings[memberValue] = m.Name;
			}
		}
	}

	Dictionary<string, string> memberValueMappings = new();

	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		=> sourceType == typeof(string);

	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		var str = value.ToString();

		if (memberValueMappings.TryGetValue(str, out var name)
			&& Enum.TryParse<T>(name, out var v))
		{
			return v;
		}

		return default(T);
	}
}
