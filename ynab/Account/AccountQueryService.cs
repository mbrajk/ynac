namespace ynab.Account
{
    public class AccountQueryService(IBudgetApi budgetApi) : IAccountQueryService
    {
        public async Task<IReadOnlyCollection<Account>> GetBudgetAccounts(Budget.Budget selectedBudget)
        {
            var response = await budgetApi.GetBudgetAccountsAsync(selectedBudget.BudgetId);

            return response.Data?.Accounts ?? [new Account()];
        }
    }
}
