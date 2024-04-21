namespace YnabApi.Account
{
    public class AccountQueryService(IYnabApi _ynabApi)
    {
        public async Task<IReadOnlyCollection<Account>> GetBudgetAccounts(Budget.Budget selectedBudget)
        {
            var response = await _ynabApi.GetBudgetAccountsAsync(selectedBudget.Id);

            return response.Data.Accounts;
        }
    }
}
