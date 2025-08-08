using Spectre.Console;

namespace ynac;

internal static class TokenHandler
{
    public static void MaybeSaveToken(string? tokenFromCommandLine)
    {
        if (string.IsNullOrWhiteSpace(tokenFromCommandLine) || tokenFromCommandLine == Constants.DefaultTokenString)
        {
            return;
        }

        var configFile = Constants.ConfigFilePath;
        var lines = File.Exists(configFile) ? File.ReadAllLines(configFile).ToList() : new List<string>();

        var ynabApiSectionIndex = lines.FindIndex(line => line.Trim().Equals(Constants.YnabApiSectionHeader, StringComparison.OrdinalIgnoreCase));

        if (ynabApiSectionIndex == -1)
        {
            lines.Add(Constants.YnabApiSectionHeader);
            lines.Add($"{Constants.TokenString}={tokenFromCommandLine}");
        }
        else
        {
            var tokenIndex = -1;
            for (var i = ynabApiSectionIndex + 1; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith("[")) // Reached another section
                {
                    break;
                }
                if (lines[i].Trim().StartsWith($"{Constants.TokenString}=", StringComparison.OrdinalIgnoreCase))
                {
                    tokenIndex = i;
                    break;
                }
            }

            if (tokenIndex != -1)
            {
                lines[tokenIndex] = $"{Constants.TokenString}={tokenFromCommandLine}";
            }
            else
            {
                lines.Insert(ynabApiSectionIndex + 1, $"{Constants.TokenString}={tokenFromCommandLine}");
            }
        }

        File.WriteAllLines(configFile, lines);
        AnsiConsole.WriteLine($"[YNAC] Token peristed to {Constants.ConfigFileName}.");
    }

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
