using AppleDev.Tool.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
	config
	.AddBranch("simulator", sim =>
	{
		sim.AddCommand<ListSimulatorsCommand>("list")
			.WithDescription("Lists Simulators")
			.WithExample(new[] { "simulator", "list" })
			.WithExample(new[] { "simulator", "list", "--available" })
			.WithExample(new[] { "simulator", "list", "--booted" });
	});

	config.AddBranch("device", sdkBranch =>
	{
		
	});

	config.AddBranch("keychain", sdkBranch =>
	{
		
	});
});

try
{
	app.Run(args);
}
catch (Exception ex)
{
	AnsiConsole.WriteException(ex);
}