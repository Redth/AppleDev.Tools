namespace AppleDev;

public class ProcessResult
{
	public ProcessResult(bool success, string stdout = "", string stderr = "")
	{
		Success = success;
		StdOut = stdout;
		StdErr = stderr;
	}

	public readonly bool Success;

	public readonly string StdErr;
	public readonly string StdOut;
}