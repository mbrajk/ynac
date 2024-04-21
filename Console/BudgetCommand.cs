using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace ynac;

public sealed class BudgetCommand : AsyncCommand<BudgetCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The budget name to filter to. Will find the first budget that contains this string")]
        [CommandArgument(0, "[budgetFilter]")]
        public string? BudgetFilter { get; init; }

        [Description("The category name to filter to. Will find the first category containing this string")]
        [CommandArgument(0, "[budgetFilter]")]
        public string? CategoryFilter { get; init; }
		
        [Description("Open the budget on the web. If a budget filter is applied, the found budget will be opened. If a filter is not applied, the budget will be opened after one is selected.")]
        [CommandOption("-o|--open")]
        [DefaultValue(false)]
        public bool Open { get; init; }
        
        [Description("Show goal progress indicators in the view (work in progress, may not properly reflect some goals. Additionally, text is not formatted properly)")]
        [CommandOption("-g|--show-goals")]
        [DefaultValue(false)]
        public bool ShowGoals { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var services = Setup.BuildServiceProvider();
        var ynabConsole = services.GetRequiredService<IYnabConsole>();
        await ynabConsole.RunAsync(settings);
        return 0;
    }
}