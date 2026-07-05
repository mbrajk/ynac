using Spectre.Console;
using ynab.Category;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// A single selectable category flattened out of its category group.
/// </summary>
internal sealed record CategoryChoice(Guid Id, string GroupName, string CategoryName)
{
    public override string ToString() => $"{GroupName}: {CategoryName}";
}

internal static class BudgetActionHelpers
{
    /// <summary>
    /// Blocks until the user presses enter, so table output can be read before the budget re-renders.
    /// </summary>
    internal static void WaitForEnter(IAnsiConsoleService console)
    {
        console.Prompt(new TextPrompt<string>("[grey italic]press enter to continue[/]").AllowEmpty());
    }

    /// <summary>
    /// Runs a write action safely when amounts are hidden: offers to reveal amounts for the
    /// duration of the action and restores the hidden state afterwards. Acting on amounts
    /// the user cannot see is risky, so reveal is offered up front.
    /// </summary>
    internal static async Task RunWithAmountsRevealOffer(IValueFormatter valueFormatter, IAnsiConsoleService console, Func<Task> action)
    {
        var wasMasked = valueFormatter.GetMasked();
        if (wasMasked)
        {
            var reveal = console.Prompt(new ConfirmationPrompt("Amounts are currently hidden. Reveal them for this action?"));
            if (reveal)
            {
                valueFormatter.SetMasked(false);
            }
        }

        try
        {
            await action();
        }
        finally
        {
            if (wasMasked)
            {
                valueFormatter.SetMasked(true);
            }
        }
    }

    internal static IReadOnlyList<CategoryChoice> FlattenCategories(IEnumerable<CategoryGroup> categoryGroups)
    {
        return categoryGroups
            .Where(group => !group.Deleted && !group.Hidden)
            .SelectMany(group => group.Categories
                .Where(category => !category.Deleted && !category.Hidden)
                .Select(category => new CategoryChoice(category.Id, group.Name, category.Name)))
            .ToList();
    }

    internal static string DescribeTransaction(Transaction transaction, IValueFormatter valueFormatter)
    {
        var payee = string.IsNullOrWhiteSpace(transaction.PayeeName) ? "(no payee)" : transaction.PayeeName;
        var category = string.IsNullOrWhiteSpace(transaction.CategoryName) ? "(uncategorized)" : transaction.CategoryName;

        return Markup.Escape($"{transaction.Date:yyyy-MM-dd}  {payee}  [{category}]  {valueFormatter.Format(transaction.Amount / 1000)}  ({transaction.AccountName})");
    }

    internal static Table BuildTransactionTable(IEnumerable<Transaction> transactions, IValueFormatter valueFormatter)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Yellow);

        table.AddColumn(new TableColumn("Date").LeftAligned());
        table.AddColumn(new TableColumn("Account").LeftAligned());
        table.AddColumn(new TableColumn("Payee").LeftAligned());
        table.AddColumn(new TableColumn("Category").LeftAligned());
        table.AddColumn(new TableColumn("Memo").LeftAligned());
        table.AddColumn(new TableColumn("Amount").RightAligned());
        table.AddColumn(new TableColumn("Cleared").Centered());
        table.AddColumn(new TableColumn("Approved").Centered());

        var total = 0m;
        foreach (var transaction in transactions)
        {
            var amountDollars = transaction.Amount / 1000;
            total += amountDollars;

            var amountColor = transaction.Amount < 0 ? "red" : "green";
            var clearedMarker = transaction.Cleared switch
            {
                TransactionClearedStatus.Reconciled => "[green]R[/]",
                TransactionClearedStatus.Cleared => "[green]C[/]",
                _ => "[grey]U[/]",
            };

            table.AddRow(
                new Markup(Markup.Escape(transaction.Date.ToString("yyyy-MM-dd"))),
                new Markup(Markup.Escape(transaction.AccountName)),
                new Markup(Markup.Escape(transaction.PayeeName ?? string.Empty)),
                new Markup(transaction.CategoryName is null ? "[yellow italic]uncategorized[/]" : Markup.Escape(transaction.CategoryName)),
                new Markup($"[grey]{Markup.Escape(Truncate(transaction.Memo, 30))}[/]"),
                new Markup($"[{amountColor}]{Markup.Escape(valueFormatter.Format(amountDollars))}[/]"),
                new Markup(clearedMarker),
                new Markup(transaction.Approved ? "[green]Y[/]" : "[yellow]N[/]")
            );
        }

        table.ShowFooters().AddRow(
            new Markup("[bold]Total[/]"), new Markup(""), new Markup(""), new Markup(""), new Markup(""),
            new Markup($"[bold]{Markup.Escape(valueFormatter.Format(total))}[/]"), new Markup(""), new Markup("")
        );

        return table;
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }
}
