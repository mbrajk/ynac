using Spectre.Console;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that lists recent transactions for the selected budget.
/// </summary>
internal class ListTransactionsBudgetAction : IBudgetAction
{
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public ListTransactionsBudgetAction(
        ITransactionQueryService transactionQueryService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _transactionQueryService = transactionQueryService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "List transactions";
    public int Order => 1;

    public async Task ExecuteAsync()
    {
        var daysBack = _console.Prompt(
            new SelectionPrompt<int>()
                .Title("Show transactions from the last:")
                .AddChoices(7, 30, 90)
                .UseConverter(days => $"{days} days"));

        var sinceDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-daysBack);
        var transactions = await _transactionQueryService.GetTransactions(_budgetContext.SelectedBudget, sinceDate);

        if (transactions.Count == 0)
        {
            _console.Markup($"[yellow]No transactions in the last {daysBack} days[/]\n");
        }
        else
        {
            _console.Write(BudgetActionHelpers.BuildTransactionTable(transactions, _valueFormatter));
        }

        BudgetActionHelpers.WaitForEnter(_console);
    }
}
