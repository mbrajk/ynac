using ynac.YnabApi.Category;

namespace ynac.YnabApi.Budget
{
    public class BudgetQueryService(IYnabApi _ynabApi) : IBudgetQueryService
    {
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(
            YnabApi.Budget.Budget selectedBudget
        )
        {
            var response = await _ynabApi.GetBudgetCategoriesAsync(selectedBudget.Id);

            return response.Data.Groups;
        }

        public async Task<IReadOnlyCollection<Account.Account>> GetBudgetAccounts(YnabApi.Budget.Budget selectedBudget)
        {
            var response = await _ynabApi.GetBudgetAccountsAsync(selectedBudget.Id);

            return response.Data.Accounts;
        }

        public async Task<IReadOnlyCollection<YnabApi.Budget.Budget>> GetBudgets()
        {
            var response = await _ynabApi.GetBudgetsAsync();

            return response.Data.Budgets;
        }
        
        public async Task<BudgetMonth> GetBudgetMonth(Guid budgetId, DateOnly date)
        {
            var dateModified = date;

            if (date.Year > DateTime.UtcNow.Year)
            {
                dateModified = new DateOnly(DateTime.UtcNow.Year, date.Month, 1);
            }

            var dateString = dateModified.ToString("yyyy-MM-01");
            var response = await _ynabApi.GetBudgetMonthAsync(budgetId, dateString);
        
            return response.Data.Budget;
        }
    }
}
