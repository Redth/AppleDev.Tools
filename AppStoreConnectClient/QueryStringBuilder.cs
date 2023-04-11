namespace AppleAppStoreConnect;

public class QueryStringBuilder
{
	List<string> qs = new List<string>();

	public void Include(string? include, params string[] validValues)
	{
		if (!string.IsNullOrEmpty(include))
		{
			if (validValues is not null && validValues.Length > 0 && !validValues.Contains(include))
				throw new ArgumentException($"Invalid include value '{include}'.  Possible values are: " + string.Join(", ", validValues));

			qs.Add($"include={include}");
		}
	}

	public void Sort(string? sort, params string[] validValues)
	{
		if (!string.IsNullOrEmpty(sort))
		{
			if (validValues is not null && validValues.Length > 0 && !validValues.Contains(sort))
				throw new ArgumentException($"Invalid sort value '{sort}'.  Possible values are: " + string.Join(", ", validValues));

			qs.Add($"sort={sort}");
		}
	}

	public void Limit(string paramName, string? type, int max, int? limit)
	{
		if (limit is not null)
		{
			if (limit > max || limit < 0)
				throw new ArgumentOutOfRangeException(paramName, "Invalid Limit");

			if (string.IsNullOrEmpty(type))
				qs.Add($"limit={limit}");
			else
				qs.Add($"limit[{type}]={limit}");
		}
	}


	public void Filter<TEnum>(string field, TEnum[]? values) where TEnum : struct
	{
		if (values is not null && values.Length > 0)
		{
			var valuesStr = new List<string>();
			foreach (var v in values)
			{
				var name = Enum.GetName(typeof(TEnum), v);
				if (!string.IsNullOrEmpty(name))
					valuesStr.Add(name);
			}
			
			qs.Add($"filter[{field}]={string.Join(",", valuesStr)}");
		}
	}

	public void Filter(string field, string[]? values)
	{
		if (values is not null && values.Length > 0)
		{
			qs.Add($"filter[{field}]={string.Join(",", values)}");
		}
	}

	public void Fields(string field, string[]? values)
	{
		if (values is not null && values.Length > 0)
		{
			qs.Add($"fields[{field}]={string.Join(",", values)}");
		}
	}

	public override string ToString()
		=> string.Join("&", qs);
}
