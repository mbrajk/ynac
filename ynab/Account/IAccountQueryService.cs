namespace ynab.Account;

public interface IAccountQueryService
{
    Task<IReadOnlyCollection<Account>> GetBudgetAccounts(Budget.Budget selectedBudget);
}