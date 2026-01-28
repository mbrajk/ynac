using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using ynab;
using ynac.ErrorHandling;

namespace ynac.Commands;

public sealed class BudgetCommand : AsyncCommand<BudgetCommandSettings>
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "GetValue<bool> binds a primitive and does not rely on reflection-based member access; safe under trimming. Will ultimately migrate to source-generated binding later.")]
    public override async Task<int> ExecuteAsync(CommandContext context, BudgetCommandSettings settings, CancellationToken cancellationToken)
    {
        IErrorWriter errorWriter = settings.JsonOutput
            ? new PlainErrorWriter()
            : new InteractiveErrorWriter();

        if (settings.Open && settings.PullLastUsed)
        {
            AnsiConsole.Markup("[red]Cannot use both --open and --last-used flags together. --open flag will be ignored[/]\n");
            settings.Open = false;
        }

        if (settings.JsonOutput && settings.Open)
        {
            errorWriter.WriteError("Cannot use --json-output with --open");
            return 1;
        }

        var configurationRoot = new ConfigurationBuilder()
            .AddIniFile(Constants.ConfigFileName, settings.DebugSkipConfig)
            .AddEnvironmentVariables()
            .Build();

        var token = TokenHandler.EnsureTokenPersisted(settings.ApiToken, configurationRoot);

        var hideAmountsConfig = configurationRoot.GetValue<bool>(Constants.YnacHideAmountsConfigPath);
        var hideAmounts = settings.HideAmounts || hideAmountsConfig;

        if (settings.JsonOutput && hideAmounts)
        {
            errorWriter.WriteError("Cannot use --json-output with --hide-amounts");
            return 1;
        }

        var ynacConsoleSettings = new YnacConsoleSettings(token, hideAmounts, errorWriter);

        var ynacProvider = YnacConsoleProvider.BuildYnacServices(ynacConsoleSettings);

        var ynacConsole = ynacProvider.GetRequiredService<IYnacConsole>();

        try
        {
            await ynacConsole.RunAsync(settings);
        }
        catch (YnabAuthenticationException ex)
        {
            errorWriter.WriteAuthError(ex.Message);
            return 1;
        }
        catch (YnabApiException ex)
        {
            errorWriter.WriteApiError(ex.Message);
            return 1;
        }

        return 0;
    }
}

public record struct YnacConsoleSettings(string Token, bool HideAmounts, IErrorWriter ErrorWriter);

public sealed class BudgetCommandSettings : CommandSettings
{
    [Description("The budget name to filter to. If more than one budget matches the filter string, a selection prompt will be presented.")]
    [CommandArgument(0, "[budgetFilter]")]
    public string? BudgetFilter { get; init; }

    [Description("The category name to filter to. Will all first categorites containing this string")]
    [CommandArgument(1, "[categoryFilter]")]
    public string? CategoryFilter { get; init; }

    [Description("Open the budget on the web. If a budget filter is applied, the found budget will be opened. " +
                 "If a filter is not applied, the budget will be opened after one is selected. Cannot be used with" +
                 "--last-used flag.")]
    [CommandOption("-o|--open")]
    [DefaultValue(false)]
    public bool Open { get; set; }

    [Description("Show goal progress indicators in the view (work in progress, may not properly reflect some goals. " +
                 "Additionally, text formatting is not consistent with the non-goal display")]
    [CommandOption("-g|--show-goals")]
    [DefaultValue(false)]
    public bool ShowGoals { get; init; }

    [Description("Default to showing the last used budget, will ignore the budget filter, and the --open flag. " +
                 "Relies on the YNAB API to determine the last used budget. " +
                 "If the API token you are using has not accessed a budget previously, the command will fail. " +
                 "Cannot be used with the --open flag.")]
    [CommandOption("-u|--last-used")]
    [DefaultValue(false)]
    public bool PullLastUsed { get; init; }

    [Description("YNAB API Token. If provided on first run, the token will be saved to config.ini. You can " +
                 "also set the token in the config.ini file directly without having to use this flag.")]
    [CommandOption("--api-token")]
    public string? ApiToken { get; init; }

    [Description("Hide all monetary amounts in the output. This is useful for sharing screenshots or when privacy is a concern.")]
    [CommandOption("--hide-amounts")]
    [DefaultValue(false)]
    public bool HideAmounts { get; init; }

    [Description("Show hidden categories in the budget view. By default, hidden categories are filtered out.")]
    [CommandOption("--show-hidden-categories")]
    [DefaultValue(false)]
    public bool ShowHiddenCategories { get; init; }

    [Description("Output budget data as JSON to stdout instead of interactive mode. Cannot be combined with --open or --hide-amounts.")]
    [CommandOption("-j|--json-output")]
    [DefaultValue(false)]
    public bool JsonOutput { get; init; }

    [Description("Developer only: Skip config.ini file creation. Useful for testing environment variables.")]
    [CommandOption("--debug-skip-config", IsHidden = true)]
    [DefaultValue(false)]
    public bool DebugSkipConfig { get; init; }
}
