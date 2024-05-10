using Spectre.Console;

namespace ynac;

internal static class TokenHandler
{
    // warning is suppressed as this is used in Program.cs but
    // preprocessor directive causes it to appear unused in debug
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