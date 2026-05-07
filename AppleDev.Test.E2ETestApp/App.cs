namespace E2ETestApp;

public class App : Application
{
	public App()
	{
		Console.WriteLine("E2E_APP_STARTED_SUCCESSFULLY");

		var envTestValue = Environment.GetEnvironmentVariable("E2E_TEST_VALUE");
		Console.WriteLine($"E2E_ENV_RECEIVED:{envTestValue}");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new ContentPage());
	}
}
