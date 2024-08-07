using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ynac.Commands;

public sealed class BudgetCommand : AsyncCommand<BudgetCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BudgetCommandSettings settings)
    {
        if (settings.Open && settings.PullLastUsed)
        {
            AnsiConsole.Markup("[red]Cannot use both --open and --last-used flags together. --open flag will be ignored[/]\n"); 
            settings.Open = false;
        }
                
        var configurationRoot =  new ConfigurationBuilder()
            .AddIniFile(Constants.ConfigFileLocation) 
            .AddEnvironmentVariables()
            .Build();
                
        var token = configurationRoot[Constants.YnabApiSectionTokenKey];
        token = TokenHandler.HandleMissingToken(token);
                
        var ynacProvider = YnacConsoleProvider.BuildYnacServices(token);
                
        var ynacConsole = ynacProvider.GetRequiredService<IYnacConsole>();
        await ynacConsole.RunAsync(settings);
        return 0; 
    }
}

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
}
