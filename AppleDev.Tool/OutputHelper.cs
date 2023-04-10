using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using Spectre.Console;

namespace AppleDev.Tool;

[TypeConverter(typeof(OutputFormatTypeConverter))]
public enum OutputFormat
{
	None,
	Json,
	Xml
}

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

static class OutputHelper
{
	internal static void Output<T>(IEnumerable<T>? items, OutputFormat? format, bool verbose, string[] columns, Func<T, string?[]> getRow, string[] extraColumns, Func<T, string?[]> getExtraRow)
	{
		var allColumns = columns;
		if (verbose)
			allColumns = columns.Concat(extraColumns).ToArray();

		var allRows = getRow;
		if (verbose)
		{
			allRows = (t) =>
				getRow(t).Concat(getExtraRow(t)).ToArray();
		}
		
		Output<T>(items, format, allColumns, allRows);
	}
	
	internal static void Output<T>(IEnumerable<T>? items, OutputFormat? format, string[] columns,
		Func<T, string?[]> getRow)
	{
		if (items is null)
			return;

		if ((format ?? OutputFormat.None) == OutputFormat.None)
		{
			OutputTable<T>(items, columns, getRow);
		}
		else
		{
			if (format == OutputFormat.Json)
				AnsiConsole.WriteLine(JsonSerialize<IEnumerable<T>>(items));
			else if (format == OutputFormat.Xml)
				AnsiConsole.WriteLine(XmlSerialize<IEnumerable<T>>(items));
		}
	}

	internal static void Output<T>(T item, OutputFormat? format, string[] properties, Func<T, string?[]> getValues)
	{
		if ((format ?? OutputFormat.None) == OutputFormat.None)
		{
			OutputObject<T>(item, properties, getValues);
		}
		else
		{
			if (format == OutputFormat.Json)
				AnsiConsole.WriteLine(JsonSerialize<T>(item));
			else if (format == OutputFormat.Xml)
				AnsiConsole.WriteLine(XmlSerialize<T>(item));
		}
	}

	internal static void OutputTable<T>(IEnumerable<T> items, string[] columns, Func<T, string?[]> getRow)
	{
		var table = new Table();

		foreach (var c in columns)
			table.AddColumn(c);

		foreach (var i in items)
		{
			var row = getRow(i);
			table.AddRow(row.Select(r => r ?? string.Empty).ToArray());
		}

		// Render the table to the console
		AnsiConsole.Write(table);
	}

	internal static void OutputObject<T>(T item, string[] properties, Func<T, string?[]> getValues)
	{
		var table = new Table();
		var values = getValues(item);

		table.AddColumn("Property");
		table.AddColumn("Value");

		for (int i = 0; i < properties.Length; i++)
		{
			var name = properties[i];
			var val = values[i];

			table.AddRow(name, val ?? string.Empty);
		}

		// Render the table to the console
		AnsiConsole.Write(table);
	}

	internal static void Output<T>(T data, OutputFormat outputFormat)
	{
		var r = string.Empty;
		switch (outputFormat)
		{
			case OutputFormat.None:
				if (data is IEnumerable)
				{
					var enumerator = ((IEnumerable)data).GetEnumerator();
					while (enumerator.MoveNext())
						r += (enumerator.Current?.ToString() ?? string.Empty) + Environment.NewLine;
				}
				else
				{
					r = data?.ToString() ?? string.Empty;
				}
				break;
			case OutputFormat.Json:
				r = JsonSerialize<T>(data);
				break;
			case OutputFormat.Xml:
				r = XmlSerialize<T>(data);
				break;
		}

		Console.Write(r);
	}

	internal static void Output(this Exception ex)
	{
		AnsiConsole.WriteLine();

		AnsiConsole.WriteException(ex);
	}

	static string JsonSerialize<T>(T obj)
	{
		return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

		//var s = new DataContractJsonSerializerSettings();
		//s.UseSimpleDictionaryFormat = true;

		//var js = new DataContractJsonSerializer(typeof(T), s);
		//using (var ms = new MemoryStream())
		//{
		//	js.WriteObject(ms, obj);
		//	ms.Position = 0;
		//	using (var sr = new StreamReader(ms))
		//		return sr.ReadToEnd();
		//}
	}

	static string XmlSerialize<T>(T obj)
	{
		var xml = new XmlSerializer(typeof(T));

		using (var textWriter = new StringWriter())
		{
			xml.Serialize(textWriter, obj);
			return textWriter.ToString();
		}
	}
}