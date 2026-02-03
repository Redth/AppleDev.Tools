using AppleDev.FbIdb;
using Spectre.Console;

namespace AppleDev.Tool.Services;

/// <summary>
/// Service for lazy-loading IDB client connections.
/// The IDB companion process is only started when actually needed.
/// </summary>
public class IdbClientService : IAsyncDisposable
{
	private readonly Dictionary<string, IdbClient> _clients = new();
	private readonly object _lock = new();

	/// <summary>
	/// Gets or creates an IDB client for the specified simulator UDID.
	/// The client is connected and ready to use.
	/// </summary>
	/// <param name="udid">The simulator UDID</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A connected IdbClient instance</returns>
	public async Task<IdbClient?> GetClientAsync(string udid, CancellationToken cancellationToken = default)
	{
		// Check if idb_companion is available
		if (!IdbCompanionLocator.TryFindIdbCompanion(out var companionPath))
		{
			AnsiConsole.MarkupLine("[yellow]Warning:[/] idb_companion not found.");
			AnsiConsole.MarkupLine("Install it with: [cyan]brew tap facebook/fb && brew install idb-companion[/]");
			return null;
		}

		lock (_lock)
		{
			if (_clients.TryGetValue(udid, out var existingClient))
			{
				return existingClient;
			}
		}

		try
		{
			var options = new IdbCompanionOptions
			{
				CompanionPath = companionPath
			};

			var client = new IdbClient(udid, options);
			await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

			lock (_lock)
			{
				// Double-check in case another thread created it
				if (_clients.TryGetValue(udid, out var existingClient))
				{
					// Dispose the one we just created
					_ = client.DisposeAsync();
					return existingClient;
				}

				_clients[udid] = client;
			}

			return client;
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error connecting to IDB:[/] {Markup.Escape(ex.Message)}");
			return null;
		}
	}

	/// <summary>
	/// Checks if idb_companion is available on the system.
	/// </summary>
	/// <returns>True if idb_companion is available</returns>
	public static bool IsIdbAvailable()
	{
		return IdbCompanionLocator.TryFindIdbCompanion(out _);
	}

	/// <summary>
	/// Checks if we're running on macOS (required for IDB).
	/// </summary>
	/// <returns>True if running on macOS</returns>
	public static bool IsMacOS()
	{
		return OperatingSystem.IsMacOS();
	}

	/// <summary>
	/// Validates that IDB commands can run, displaying appropriate error messages if not.
	/// </summary>
	/// <returns>True if IDB commands can run</returns>
	public static bool ValidateCanRun()
	{
		if (!IsMacOS())
		{
			AnsiConsole.MarkupLine("[red]Error:[/] IDB commands are only available on macOS");
			return false;
		}

		if (!IsIdbAvailable())
		{
			AnsiConsole.MarkupLine("[yellow]Warning:[/] idb_companion not found.");
			AnsiConsole.MarkupLine("Install it with: [cyan]brew tap facebook/fb && brew install idb-companion[/]");
			return false;
		}

		return true;
	}

	public async ValueTask DisposeAsync()
	{
		List<IdbClient> clientsToDispose;
		lock (_lock)
		{
			clientsToDispose = _clients.Values.ToList();
			_clients.Clear();
		}

		foreach (var client in clientsToDispose)
		{
			try
			{
				await client.DisposeAsync().ConfigureAwait(false);
			}
			catch
			{
				// Ignore disposal errors
			}
		}
	}
}
