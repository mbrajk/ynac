using Spectre.Console;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that lists unapproved transactions and approves the ones the user selects.
/// </summary>
internal class ApproveTransactionsBudgetAction : IBudgetAction
{
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly ITransactionCommandService _transactionCommandService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public ApproveTransactionsBudgetAction(
        ITransactionQueryService transactionQueryService,
        ITransactionCommandService transactionCommandService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _transactionQueryService = transactionQueryService;
        _transactionCommandService = transactionCommandService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "Approve transactions";
    public int Order => 2;

    public Task ExecuteAsync() =>
        BudgetActionHelpers.RunWithAmountsRevealOffer(_valueFormatter, _console, ExecuteCoreAsync);

    private async Task ExecuteCoreAsync()
    {
        var unapproved = await _transactionQueryService.GetTransactions(
            _budgetContext.SelectedBudget,
            filter: TransactionsFilter.Unapproved);

        if (unapproved.Count == 0)
        {
            _console.Markup("[green]No transactions waiting for approval[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var selected = _console.Prompt(
            new MultiSelectionPrompt<Transaction>()
                .Title($"[yellow]{unapproved.Count}[/] unapproved transaction(s). Select the ones to approve:")
                .PageSize(15)
                .MoreChoicesText("more..")
                .InstructionsText("[grey](space to toggle, enter to accept)[/]")
                .NotRequired()
                .AddChoices(unapproved)
                .UseConverter(transaction => BudgetActionHelpers.DescribeTransaction(transaction, _valueFormatter)));

        if (selected.Count == 0)
        {
            return;
        }

        var confirmed = _console.Prompt(new ConfirmationPrompt($"Approve [yellow]{selected.Count}[/] transaction(s)?"));
        if (!confirmed)
        {
            return;
        }

        var approved = await _transactionCommandService.ApproveTransactions(
            _budgetContext.SelectedBudget,
            selected.Select(transaction => transaction.Id).ToList());

        _console.Markup($"[green]Approved {approved.Count} transaction(s)[/]\n");
        BudgetActionHelpers.WaitForEnter(_console);
    }
}
