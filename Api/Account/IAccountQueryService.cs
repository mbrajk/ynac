namespace YnabApi.Account;

public interface IAccountQueryService
{
    Task<IReadOnlyCollection<Account>> GetBudgetAccounts(Budget.Budget selectedBudget);
}