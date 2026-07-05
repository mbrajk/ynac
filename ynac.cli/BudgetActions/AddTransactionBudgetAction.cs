using Spectre.Console;
using ynab.Account;
using ynab.Budget;
using ynab.Transaction;
using ynac.BudgetSelection;

namespace ynac.BudgetActions;

/// <summary>
/// Budget action that creates a new transaction from interactive prompts.
/// </summary>
internal class AddTransactionBudgetAction : IBudgetAction
{
    private readonly IAccountQueryService _accountQueryService;
    private readonly IBudgetQueryService _budgetQueryService;
    private readonly ITransactionCommandService _transactionCommandService;
    private readonly IBudgetContext _budgetContext;
    private readonly IAnsiConsoleService _console;
    private readonly IValueFormatter _valueFormatter;

    public AddTransactionBudgetAction(
        IAccountQueryService accountQueryService,
        IBudgetQueryService budgetQueryService,
        ITransactionCommandService transactionCommandService,
        IBudgetContext budgetContext,
        IAnsiConsoleService console,
        IValueFormatter valueFormatter)
    {
        _accountQueryService = accountQueryService;
        _budgetQueryService = budgetQueryService;
        _transactionCommandService = transactionCommandService;
        _budgetContext = budgetContext;
        _console = console;
        _valueFormatter = valueFormatter;
    }

    public string DisplayName => "Add transaction";
    public int Order => 4;

    public Task ExecuteAsync() =>
        BudgetActionHelpers.RunWithAmountsRevealOffer(_valueFormatter, _console, ExecuteCoreAsync);

    private async Task ExecuteCoreAsync()
    {
        var accounts = await _accountQueryService.GetBudgetAccounts(_budgetContext.SelectedBudget);
        var openAccounts = accounts
            .Where(account => account is { OnBudget: true, Closed: false, Deleted: false })
            .ToList();

        if (openAccounts.Count == 0)
        {
            _console.Markup("[red]No open budget accounts found[/]\n");
            BudgetActionHelpers.WaitForEnter(_console);
            return;
        }

        var account = _console.Prompt(
            new SelectionPrompt<Account>()
                .Title("Account:")
                .PageSize(15)
                .MoreChoicesText("more..")
                .AddChoices(openAccounts)
                .UseConverter(a => Markup.Escape(a.Name)));

        var payeeName = _console.Prompt(
            new TextPrompt<string>("Payee (existing name matches, new name creates):").AllowEmpty());

        var categoryGroups = await _budgetQueryService.GetBudgetCategories(options =>
        {
            options.SelectedBudget = _budgetContext.SelectedBudget;
        });

        var noCategory = new CategoryChoice(Guid.Empty, "None", "leave uncategorized");
        var categoryChoices = new List<CategoryChoice> { noCategory };
        categoryChoices.AddRange(BudgetActionHelpers.FlattenCategories(categoryGroups));

        var category = _console.Prompt(
            new SelectionPrompt<CategoryChoice>()
                .Title("Category:")
                .PageSize(15)
                .MoreChoicesText("more..")
                .AddChoices(categoryChoices)
                .UseConverter(choice => Markup.Escape(choice.ToString())));

        var direction = _console.Prompt(
            new SelectionPrompt<string>()
                .Title("Direction:")
                .AddChoices("Outflow", "Inflow"));

        var amountDollars = _console.Prompt(
            new TextPrompt<decimal>("Amount:")
                .Validate(amount => amount > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Amount must be greater than zero[/]")));

        var dateText = _console.Prompt(
            new TextPrompt<string>("Date (yyyy-MM-dd):")
                .DefaultValue(DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"))
                .Validate(text => DateOnly.TryParse(text, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Enter a valid date in yyyy-MM-dd format[/]")));

        var memo = _console.Prompt(new TextPrompt<string>("Memo:").AllowEmpty());

        var cleared = _console.Prompt(new ConfirmationPrompt("Mark as cleared?") { DefaultValue = false });

        var amountMilliunits = Math.Round(amountDollars * 1000, MidpointRounding.AwayFromZero);
        if (direction == "Outflow")
        {
            amountMilliunits = -amountMilliunits;
        }

        var summary = $"{dateText}  {(string.IsNullOrWhiteSpace(payeeName) ? "(no payee)" : payeeName)}  " +
                      $"[{category.CategoryName}]  {_valueFormatter.Format(amountMilliunits / 1000)}  ({account.Name})";
        var confirmed = _console.Prompt(new ConfirmationPrompt($"Create transaction? {Markup.Escape(summary)}"));
        if (!confirmed)
        {
            return;
        }

        var created = await _transactionCommandService.CreateTransaction(_budgetContext.SelectedBudget, new SaveTransaction
        {
            AccountId = account.Id,
            Date = DateOnly.Parse(dateText),
            Amount = amountMilliunits,
            PayeeName = string.IsNullOrWhiteSpace(payeeName) ? null : payeeName,
            CategoryId = category.Id == Guid.Empty ? null : category.Id,
            Memo = string.IsNullOrWhiteSpace(memo) ? null : memo,
            Cleared = cleared ? TransactionClearedStatus.Cleared : TransactionClearedStatus.Uncleared,
            Approved = true,
        });

        if (created == null)
        {
            _console.Markup("[red]Transaction was not created[/]\n");
        }
        else
        {
            _budgetContext.DataStale = true;
            _console.Markup("[green]Transaction created[/]\n");
        }

        BudgetActionHelpers.WaitForEnter(_console);
    }
}
