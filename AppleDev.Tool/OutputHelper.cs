using System.Collections;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Spectre.Console;

namespace AppleDev.Tool;

static class OutputHelper
{
	internal static void OutputFailure(this ProcessResult result, string? message = null, bool verbose = false)
	{
		if (verbose)
		{
			AnsiConsole.WriteLine(result.StdOut);
		}

		if (!result.Success)
		{
			message ??= "Failed";

			AnsiConsole.WriteLine(result.StdErr);
			AnsiConsole.WriteLine($"[red]{message}[/]");
		}
	}

	class ResultInfo
	{
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("output")]
		public string? Output { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }
	}

	internal static void OutputResult(this ProcessResult result, OutputFormat format = OutputFormat.None, string? message = null, bool verbose = false)
	{
		if (format == OutputFormat.Json || format == OutputFormat.Xml)
		{
			var obj = new ResultInfo
			{
				Success = result.Success,
				Output = result.Success ? result.StdOut : result.StdOut + Environment.NewLine + result.StdErr,
				Message = message
			};

			if (format == OutputFormat.Json)
				AnsiConsole.WriteLine(JsonSerialize(obj));
			else if (format == OutputFormat.Xml)
				AnsiConsole.WriteLine(XmlSerialize(obj));

			return;
		}

		if (verbose)
		{
			AnsiConsole.WriteLine(result.StdOut);
		}

		if (!result.Success)
		{
			message ??= "Failed";

			AnsiConsole.WriteLine(result.StdErr);
			AnsiConsole.WriteLine($"[red]{message}[/]");
		}
	}


	internal static void Output<T>(T item, OutputFormat? format, bool verbose, params ColumnInfo<T>[] columns)
	{
		if ((format ?? OutputFormat.None) == OutputFormat.None)
		{
			OutputObject<T>(item, verbose, columns);
		}
		else
		{
			if (format == OutputFormat.Json)
				AnsiConsole.WriteLine(JsonSerialize(item));
			else if (format == OutputFormat.Xml)
				AnsiConsole.WriteLine(XmlSerialize(item));
		}
	}

	internal static void Output<T>(IEnumerable<T> items, OutputFormat? format, bool verbose, params ColumnInfo<T>[] columns)
	{
		if ((format ?? OutputFormat.None) == OutputFormat.None)
		{
			OutputTable<T>(items, verbose, columns);
		}
		else
		{
			if (format == OutputFormat.Json)
				AnsiConsole.WriteLine(JsonSerialize(items));
			else if (format == OutputFormat.Xml)
				AnsiConsole.WriteLine(XmlSerialize(items));
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

	
	internal static void OutputTable<T>(IEnumerable<T> items, bool verbose, params ColumnInfo<T>[] columns)
	{
		var table = new Table();

		foreach (var c in columns)
			table.AddColumn(c.Title);

		foreach (var i in items)
		{
			var rowValues = new List<string>();

			foreach (var c in columns)
			{
				if (!c.Verbose || verbose)
					rowValues.Add(c.Value(i) ?? string.Empty);
			}
			table.AddRow(rowValues.ToArray());
		}

		// Render the table to the console
		AnsiConsole.Write(table);
	}

	internal static void OutputObject<T>(T item, bool verbose, params ColumnInfo<T>[] columns)
	{
		var table = new Table();
		

		table.AddColumn("Property");
		table.AddColumn("Value");

		foreach (var col in columns)
		{
			if (!col.Verbose || verbose)
				table.AddRow(col.Title, col.Value(item) ?? string.Empty);
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