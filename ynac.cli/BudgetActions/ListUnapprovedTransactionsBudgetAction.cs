using Spectre.Console;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

internal class ListUnapprovedTransactionsBudgetAction : IBudgetAction
{
    private readonly ITransactionQueryService _transactionQueryService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _ansiConsoleService;
    private readonly IValueFormatter _valueFormatter;

    public ListUnapprovedTransactionsBudgetAction(
        ITransactionQueryService transactionQueryService,
        IBudgetContext budgetContext,
        IAnsiConsoleService ansiConsoleService,
        IValueFormatter valueFormatter)
    {
        _transactionQueryService = transactionQueryService;
        _budgetContext = budgetContext;
        _ansiConsoleService = ansiConsoleService;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "Manage Unapproved Transactions";
    public int Order => 1;

    public void Execute()
    {
        if (_budgetContext.CurrentBudget == null)
        {
            _ansiConsoleService.Markup("[red]No budget selected[/]\n");
            return;
        }

        var transactions = _transactionQueryService.GetUnapprovedTransactions(_budgetContext.CurrentBudget).GetAwaiter().GetResult();

        if (transactions.Count == 0)
        {
            _ansiConsoleService.Markup("[green]No unapproved transactions found![/]\n");
            _ansiConsoleService.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
            return;
        }

        while (true)
        {
            // Clear and display header
            AnsiConsole.Clear();
            _ansiConsoleService.WriteHeaderRule($"[bold]Unapproved Transactions ({transactions.Count})[/]");

            // Create interactive table
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Yellow)
                .Expand();

            table.AddColumn(new TableColumn("Date").LeftAligned());
            table.AddColumn(new TableColumn("Payee").LeftAligned());
            table.AddColumn(new TableColumn("Category").LeftAligned());
            table.AddColumn(new TableColumn("Account").LeftAligned());
            table.AddColumn(new TableColumn("Amount").RightAligned());
            table.AddColumn(new TableColumn("Memo").LeftAligned());

            foreach (var transaction in transactions)
            {
                var amount = transaction.Amount / 1000m;
                var amountColor = amount >= 0 ? "green" : "red";
                var categoryName = transaction.CategoryName ?? "[grey](Uncategorized)[/]";
                var memo = string.IsNullOrWhiteSpace(transaction.Memo) ? "" : transaction.Memo;

                table.AddRow(
                    transaction.Date,
                    transaction.PayeeName ?? "[grey](No payee)[/]",
                    categoryName,
                    transaction.AccountName,
                    $"[{amountColor}]{_valueFormatter.Format(amount)}[/]",
                    memo
                );
            }

            _ansiConsoleService.Write(table);

            // Prompt for action
            var choices = new List<string> { "← Back to budget" };
            choices.AddRange(transactions.Select((t, i) => $"Approve: {t.Date} - {t.PayeeName ?? "(No payee)"} - {_valueFormatter.Format(t.Amount / 1000m)}"));

            var choice = _ansiConsoleService.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[yellow]Select a transaction to approve or go back:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to see more options)[/]")
                    .AddChoices(choices)
            );

            if (choice.StartsWith("← Back"))
            {
                break;
            }

            // Extract transaction index
            var selectedIndex = choices.IndexOf(choice) - 1;
            var selectedTransaction = transactions.ElementAt(selectedIndex);

            // Confirmation
            var confirm = _ansiConsoleService.Prompt(
                new ConfirmationPrompt($"Are you sure you want to approve this transaction?\n  {selectedTransaction.Date} - {selectedTransaction.PayeeName} - {_valueFormatter.Format(selectedTransaction.Amount / 1000m)}")
            );

            if (confirm)
            {
                try
                {
                    var result = _transactionQueryService.ApproveTransaction(_budgetContext.CurrentBudget, selectedTransaction.Id).GetAwaiter().GetResult();
                    
                    if (result != null)
                    {
                        _ansiConsoleService.Markup("[green]✓ Transaction approved successfully![/]\n");
                        
                        // Refresh the list
                        transactions = _transactionQueryService.GetUnapprovedTransactions(_budgetContext.CurrentBudget).GetAwaiter().GetResult();
                        
                        if (transactions.Count == 0)
                        {
                            _ansiConsoleService.Markup("[green]All transactions approved![/]\n");
                            _ansiConsoleService.WriteLine("\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        }
                        
                        // Small delay to show success message
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        _ansiConsoleService.Markup("[red]Failed to approve transaction[/]\n");
                        _ansiConsoleService.WriteLine("\nPress any key to continue...");
                        Console.ReadKey(true);
                    }
                }
                catch (Exception ex)
                {
                    _ansiConsoleService.Markup($"[red]Error: {ex.Message}[/]\n");
                    _ansiConsoleService.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }

        AnsiConsole.Clear();
    }
}
