namespace AppleDev;

public class XcodeInfo
{
	public XcodeInfo(string path, bool selected, Version version)
	{
		Path = path;
		Selected = selected;
		Version = version;
	}

	public string Path { get; private set; } = "";

	public bool Selected { get; private set; }

	public Version Version { get; private set; } = new();
}