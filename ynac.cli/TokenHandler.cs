using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace ynac;

internal static class TokenHandler
{
    internal static void MaybeSaveToken(string? tokenFromCommandLine)
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

    private static string HandleMissingToken(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token) && token != Constants.DefaultTokenString)
        {
            return token;
        }
        
        AnsiConsole.Markup("[yellow]:warning: YNAB Api token not found in[/] [white underline]config.ini[/]\n");

        var prompt = new TextPrompt<string>("[white]Please enter your YNAB Api token: [/]")
            .PromptStyle("grey");

        token = AnsiConsole.Prompt(prompt);

        return token;
    }

    //TODO: add token validity check before persisting
    public static string EnsureTokenPersisted(string? settingsApiToken, IConfigurationRoot configurationRoot)
    {
        var token = settingsApiToken ?? configurationRoot[Constants.YnabApiTokenConfigPath];
        var tokenBeforePrompt = token;

        token = HandleMissingToken(token);

        if ((!string.IsNullOrWhiteSpace(settingsApiToken) && settingsApiToken != Constants.DefaultTokenString) || token != tokenBeforePrompt)
        {
            MaybeSaveToken(token);
        }

        return token;
    }
}
