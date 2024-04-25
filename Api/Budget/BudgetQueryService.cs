using YnabApi.Account;
using YnabApi.Category;

namespace YnabApi.Budget
{
    public class BudgetQueryService(IYnabApi ynabApi) : IBudgetQueryService
    {
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(
            Budget selectedBudget
        )
        {
            var response = await ynabApi.GetBudgetCategoriesAsync(selectedBudget.Id);

            return response.Data?.Groups ?? new []{ new CategoryGroup() };
        }

        public async Task<IReadOnlyCollection<Budget>> GetBudgets()
        {
            var response = await ynabApi.GetBudgetsAsync();

            return response.Data?.Budgets ?? new []{ new Budget() };
        }
        
        public async Task<BudgetMonth> GetBudgetMonth(Guid budgetId, DateOnly date)
        {
            var dateModified = date;

            if (date.Year > DateTime.UtcNow.Year)
            {
                dateModified = new DateOnly(DateTime.UtcNow.Year, date.Month, 1);
            }

            var dateString = dateModified.ToString("yyyy-MM-01");
            var response = await ynabApi.GetBudgetMonthAsync(budgetId, dateString);
        
            return response.Data?.Budget ?? new BudgetMonth();
        }
    }
}
