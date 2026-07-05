using Spectre.Console;
using ynab.ScheduledTransaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that shows upcoming scheduled transactions ordered by next occurrence.
/// </summary>
internal class ViewScheduledTransactionsBudgetAction : IBudgetAction
{
    private readonly IScheduledTransactionQueryService _scheduledTransactionQueryService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public ViewScheduledTransactionsBudgetAction(
        IScheduledTransactionQueryService scheduledTransactionQueryService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _scheduledTransactionQueryService = scheduledTransactionQueryService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "View scheduled transactions";
    public int Order => 7;

    public async Task ExecuteAsync()
    {
        var scheduledTransactions = await _scheduledTransactionQueryService.GetScheduledTransactions(_budgetContext.SelectedBudget);

        if (scheduledTransactions.Count == 0)
        {
            _console.Markup("[yellow]No scheduled transactions[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Yellow);

        table.AddColumn(new TableColumn("Next Date").LeftAligned());
        table.AddColumn(new TableColumn("Frequency").LeftAligned());
        table.AddColumn(new TableColumn("Account").LeftAligned());
        table.AddColumn(new TableColumn("Payee").LeftAligned());
        table.AddColumn(new TableColumn("Category").LeftAligned());
        table.AddColumn(new TableColumn("Amount").RightAligned());

        foreach (var scheduledTransaction in scheduledTransactions)
        {
            var amountDollars = scheduledTransaction.Amount / 1000;
            var amountColor = scheduledTransaction.Amount < 0 ? "red" : "green";

            table.AddRow(
                new Markup(Markup.Escape(scheduledTransaction.DateNext.ToString("yyyy-MM-dd"))),
                new Markup($"[grey]{Markup.Escape(scheduledTransaction.Frequency)}[/]"),
                new Markup(Markup.Escape(scheduledTransaction.AccountName)),
                new Markup(Markup.Escape(scheduledTransaction.PayeeName ?? string.Empty)),
                new Markup(Markup.Escape(scheduledTransaction.CategoryName ?? string.Empty)),
                new Markup($"[{amountColor}]{Markup.Escape(_valueFormatter.Format(amountDollars))}[/]")
            );
        }

        _console.Write(table);
        BudgetActionHelpers.WaitForEnter(_console);
    }
}
