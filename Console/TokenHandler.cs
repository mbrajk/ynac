using Spectre.Console;

namespace ynac;

internal static class TokenHandler
{
    // cannot be made private as it is used in Program.cs so that warning is disabled
    // ReSharper disable once MemberCanBePrivate.Global

    public static string HandleMissingToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || token == Constants.DefaultTokenString)
        {
            AnsiConsole.Markup("[yellow]:warning: YNAB Api token not found in[/] [white underline]config.ini[/]\n");

            var prompt = new TextPrompt<string>("[white]Please enter your YNAB Api token: [/]")
                .PromptStyle("grey");
           
            token = AnsiConsole.Prompt(prompt); 
        }

        return token;
    }
}