using Spectre.Console;
using ynab.Account;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that shows all open accounts with their balances, split into
/// on-budget and tracking sections.
/// </summary>
internal class ViewAccountsBudgetAction : IBudgetAction
{
    private readonly IAccountQueryService _accountQueryService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public ViewAccountsBudgetAction(
        IAccountQueryService accountQueryService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _accountQueryService = accountQueryService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "View accounts";
    public int Order => 6;

    public async Task ExecuteAsync()
    {
        var accounts = await _accountQueryService.GetBudgetAccounts(_budgetContext.SelectedBudget);
        var openAccounts = accounts
            .Where(account => account is { Closed: false, Deleted: false })
            .ToList();

        if (openAccounts.Count == 0)
        {
            _console.Markup("[yellow]No open accounts found[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        RenderAccountSection("On Budget", openAccounts.Where(account => account.OnBudget));
        RenderAccountSection("Tracking", openAccounts.Where(account => !account.OnBudget));

        BudgetActionHelpers.WaitForEnter(_console);
    }

    private void RenderAccountSection(string title, IEnumerable<Account> accounts)
    {
        var sectionAccounts = accounts.ToList();
        if (sectionAccounts.Count == 0)
        {
            return;
        }

        var table = new Table()
            .Title(title, Style.Parse("italic blue"))
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow);

        table.AddColumn(new TableColumn("Account").LeftAligned());
        table.AddColumn(new TableColumn("Type").LeftAligned());
        table.AddColumn(new TableColumn("Balance").RightAligned());
        table.AddColumn(new TableColumn("Cleared").RightAligned());
        table.AddColumn(new TableColumn("Uncleared").RightAligned());
        table.AddColumn(new TableColumn("Last Reconciled").RightAligned());

        var total = 0m;
        foreach (var account in sectionAccounts)
        {
            var balanceDollars = account.Balance / 1000;
            total += balanceDollars;

            var balanceColor = account.Balance < 0 ? "red" : "green";

            table.AddRow(
                new Markup($"[bold white]{Markup.Escape(account.Name)}[/]"),
                new Markup($"[grey]{Markup.Escape(account.Type)}[/]"),
                new Markup($"[{balanceColor}]{Markup.Escape(_valueFormatter.Format(balanceDollars))}[/]"),
                new Markup(Markup.Escape(_valueFormatter.Format(account.ClearedBalance / 1000))),
                new Markup(Markup.Escape(_valueFormatter.Format(account.UnclearedBalance / 1000))),
                new Markup($"[grey]{Markup.Escape(account.LastReconciledAt?.ToString("yyyy-MM-dd") ?? "never")}[/]")
            );
        }

        table.ShowFooters().AddRow(
            new Markup("[bold]Total[/]"), new Markup(""),
            new Markup($"[bold]{Markup.Escape(_valueFormatter.Format(total))}[/]"),
            new Markup(""), new Markup(""), new Markup("")
        );

        _console.Write(table);
    }
}
