using Spectre.Console;

namespace ynac;

internal static class TokenHandler
{
    private const string DefaultTokenString = "put-your-token-here";

    public static string HandleMissingToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || token == DefaultTokenString)
        {
            AnsiConsole.Markup("[yellow]:warning: YNAB Api token not found in[/] [white underline]config.ini[/]\n");

            var prompt = new TextPrompt<string>("[white]Please enter your YNAB Api token: [/]")
                .PromptStyle("grey");
           
            token = AnsiConsole.Prompt<string>(prompt); 
        }

        return token;
    }
}