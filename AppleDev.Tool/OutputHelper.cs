using System.Collections;
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

			AnsiConsole.WriteLine($"[red]{message}[/]");
			AnsiConsole.WriteLine(result.StdErr);
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
				Console.WriteLine(JsonSerialize(item));
			else if (format == OutputFormat.Xml)
				Console.WriteLine(XmlSerialize(item));
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
				Console.WriteLine(JsonSerialize(items));
			else if (format == OutputFormat.Xml)
				Console.WriteLine(XmlSerialize(items));
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
				Console.WriteLine(JsonSerialize<T>(item));
			else if (format == OutputFormat.Xml)
				Console.WriteLine(XmlSerialize<T>(item));
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
		var options = new System.Text.Json.JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};
		return System.Text.Json.JsonSerializer.Serialize(obj, options);
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