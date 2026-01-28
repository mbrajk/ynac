using Spectre.Console;

namespace ynac.ErrorHandling;

public class InteractiveErrorWriter : IErrorWriter
{
    public void WriteError(string message)
        => AnsiConsole.MarkupLine($"[red]{message}[/]");

    public void WriteAuthError(string message)
    {
        AnsiConsole.MarkupLine("\n[red bold]Authentication Error:[/]");
        AnsiConsole.MarkupLine($"[red]{message}[/]\n");
        AnsiConsole.MarkupLine("[yellow]To fix this issue:[/]");
        AnsiConsole.MarkupLine($"  1. Get a valid API token from https://app.ynab.com/settings/developer");
        AnsiConsole.MarkupLine($"  2. Run: [cyan]ynac --api-token=YOUR_TOKEN[/]");
        AnsiConsole.MarkupLine($"  3. Or update the token in: [cyan]{Constants.ConfigFilePath}[/]\n");
    }

    public void WriteApiError(string message)
    {
        AnsiConsole.MarkupLine("\n[red bold]API Error:[/]");
        AnsiConsole.MarkupLine($"[red]{message}[/]\n");
        AnsiConsole.MarkupLine("[yellow]Troubleshooting steps:[/]");
        AnsiConsole.MarkupLine("  1. Check your internet connection");
        AnsiConsole.MarkupLine("  2. Verify the YNAB API is accessible at https://api.ynab.com");
        AnsiConsole.MarkupLine("  3. Try again in a few moments\n");
    }
}
