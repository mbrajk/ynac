namespace YnabApi.Account
{
    public class AccountQueryService(IYnabApi ynabApi) : IAccountQueryService
    {
        public async Task<IReadOnlyCollection<Account>> GetBudgetAccounts(Budget.Budget selectedBudget)
        {
            var response = await ynabApi.GetBudgetAccountsAsync(selectedBudget.BudgetId);

            return response.Data?.Accounts ?? new []{ new Account() };
        }
    }
}
