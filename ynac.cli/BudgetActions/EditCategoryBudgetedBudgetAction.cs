using Spectre.Console;
using ynab.Budget;
using ynab.Category;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that edits the budgeted amount of a category for the current month.
/// </summary>
internal class EditCategoryBudgetedBudgetAction : IBudgetAction
{
    private readonly IBudgetQueryService _budgetQueryService;
    private readonly ICategoryCommandService _categoryCommandService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public EditCategoryBudgetedBudgetAction(
        IBudgetQueryService budgetQueryService,
        ICategoryCommandService categoryCommandService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _budgetQueryService = budgetQueryService;
        _categoryCommandService = categoryCommandService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "Edit budgeted amount";
    public int Order => 5;

    public Task ExecuteAsync() =>
        BudgetActionHelpers.RunWithAmountsRevealOffer(_valueFormatter, _console, ExecuteCoreAsync);

    private async Task ExecuteCoreAsync()
    {
        var categoryGroups = await _budgetQueryService.GetBudgetCategories(options =>
        {
            options.SelectedBudget = _budgetContext.SelectedBudget;
        });

        var categories = categoryGroups
            .Where(group => !group.Deleted && !group.Hidden)
            .SelectMany(group => group.Categories
                .Where(category => !category.Deleted && !category.Hidden)
                .Select(category => (Group: group, Category: category)))
            .ToList();

        if (categories.Count == 0)
        {
            _console.Markup("[red]No categories available[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var selected = _console.Prompt(
            new SelectionPrompt<(CategoryGroup Group, Category Category)>()
                .Title("Category to edit (current month):")
                .PageSize(15)
                .MoreChoicesText("more..")
                .AddChoices(categories)
                .UseConverter(choice => Markup.Escape(
                    $"{choice.Group.Name}: {choice.Category.Name}  " +
                    $"(budgeted {_valueFormatter.Format(choice.Category.Budgeted / 1000)}, " +
                    $"available {_valueFormatter.Format(choice.Category.Balance / 1000)})")));

        var newAmountDollars = _console.Prompt(
            new TextPrompt<decimal>($"New budgeted amount for {Markup.Escape(selected.Category.Name)}:")
                .DefaultValue(selected.Category.Budgeted / 1000));

        var confirmed = _console.Prompt(new ConfirmationPrompt(
            $"Set budgeted for [yellow]{Markup.Escape(selected.Category.Name)}[/] to " +
            $"[green]{Markup.Escape(_valueFormatter.Format(newAmountDollars))}[/] this month?"));
        if (!confirmed)
        {
            return;
        }

        var budgetedMilliunits = Math.Round(newAmountDollars * 1000, MidpointRounding.AwayFromZero);
        var updated = await _categoryCommandService.SetMonthCategoryBudgeted(
            _budgetContext.SelectedBudget,
            CategoryCommandService.CurrentMonth,
            selected.Category.Id,
            budgetedMilliunits);

        if (updated == null)
        {
            _console.Markup("[red]Budgeted amount was not updated[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        _budgetContext.DataStale = true;
        _console.Markup(
            $"[green]{Markup.Escape(updated.Name)} budgeted set to {Markup.Escape(_valueFormatter.Format(updated.Budgeted / 1000))}, " +
            $"available {Markup.Escape(_valueFormatter.Format(updated.Balance / 1000))}[/]\n");
    }
}
