using Spectre.Console;
using ynab.Budget;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that lists uncategorized transactions and assigns categories to the ones the user selects.
/// </summary>
internal class CategorizeTransactionsBudgetAction : IBudgetAction
{
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly ITransactionCommandService _transactionCommandService;
    private readonly IBudgetQueryService _budgetQueryService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public CategorizeTransactionsBudgetAction(
        ITransactionQueryService transactionQueryService,
        ITransactionCommandService transactionCommandService,
        IBudgetQueryService budgetQueryService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _transactionQueryService = transactionQueryService;
        _transactionCommandService = transactionCommandService;
        _budgetQueryService = budgetQueryService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "Categorize transactions";
    public int Order => 3;

    public Task ExecuteAsync() =>
        BudgetActionHelpers.RunWithAmountsRevealOffer(_valueFormatter, _console, ExecuteCoreAsync);

    private async Task ExecuteCoreAsync()
    {
        var uncategorized = await _transactionQueryService.GetTransactions(
            _budgetContext.SelectedBudget,
            filter: TransactionsFilter.Uncategorized);

        // transfers between budget accounts legitimately have no category and cannot be assigned one
        uncategorized = uncategorized.Where(transaction => transaction.TransferAccountId == null).ToList();

        if (uncategorized.Count == 0)
        {
            _console.Markup("[green]No uncategorized transactions[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var selected = _console.Prompt(
            new MultiSelectionPrompt<Transaction>()
                .Title($"[yellow]{uncategorized.Count}[/] uncategorized transaction(s). Select the ones to categorize:")
                .PageSize(15)
                .MoreChoicesText("more..")
                .InstructionsText("[grey](space to toggle, enter to accept)[/]")
                .NotRequired()
                .AddChoices(uncategorized)
                .UseConverter(transaction => BudgetActionHelpers.DescribeTransaction(transaction, _valueFormatter)));

        if (selected.Count == 0)
        {
            return;
        }

        var categoryGroups = await _budgetQueryService.GetBudgetCategories(options =>
        {
            options.SelectedBudget = _budgetContext.SelectedBudget;
        });

        var categoryChoices = BudgetActionHelpers.FlattenCategories(categoryGroups);
        if (categoryChoices.Count == 0)
        {
            _console.Markup("[red]No categories available[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var assignments = new List<(string TransactionId, Guid CategoryId)>();
        foreach (var transaction in selected)
        {
            var category = _console.Prompt(
                new SelectionPrompt<CategoryChoice>()
                    .Title($"Category for {BudgetActionHelpers.DescribeTransaction(transaction, _valueFormatter)}:")
                    .PageSize(15)
                    .MoreChoicesText("more..")
                    .AddChoices(categoryChoices)
                    .UseConverter(choice => Markup.Escape(choice.ToString())));

            assignments.Add((transaction.Id, category.Id));
        }

        var confirmed = _console.Prompt(new ConfirmationPrompt($"Categorize [yellow]{assignments.Count}[/] transaction(s)?"));
        if (!confirmed)
        {
            return;
        }

        var updated = await _transactionCommandService.CategorizeTransactions(_budgetContext.SelectedBudget, assignments);

        _budgetContext.DataStale = true;
        _console.Markup($"[green]Categorized {updated.Count} transaction(s)[/]\n");
        BudgetActionHelpers.WaitForEnter(_console);
    }
}
