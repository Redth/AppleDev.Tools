using Spectre.Console.Cli;

static class Extensions
{
    public static CommandContextData GetData(this CommandContext ctx)
        => (ctx.Data as CommandContextData) ?? new CommandContextData();

    public static int ExitCode(this ICommand _, bool success = true, int errorExitCode = 1)
        => success ? 0 : errorExitCode;
}