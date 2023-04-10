namespace AppleDev.Tool;

class ColumnInfo<T>
{
	public ColumnInfo(string title, Func<T, string?> getValue, bool verbose = false)
	{
		Title = title;
		Value = getValue;
		Verbose = verbose;
	}
	
	public readonly string Title;
	public readonly Func<T, string?> Value;

	public readonly bool Verbose;
}
