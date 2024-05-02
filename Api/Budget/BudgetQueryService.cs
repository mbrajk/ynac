using YnabApi.Account;
using YnabApi.Category;

namespace YnabApi.Budget
{
    public class BudgetQueryService(IYnabApi ynabApi) : IBudgetQueryService
    {
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(
            Budget selectedBudget,
            string? categoryFilter = null
        )
        {
            var response = await ynabApi.GetBudgetCategoriesAsync(selectedBudget.BudgetId);

            // first group is the "Internal Master Category" used by YNAB, so we skip it
            var filteredGroups = response.Data?.Groups?
                .Where(group => !group.Hidden)
                .Where(group => !group.Deleted)
                .Skip(1);

            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                filteredGroups = filteredGroups?
                    .Where(group => group.Name.Contains(categoryFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            return filteredGroups?.ToList() ?? [new CategoryGroup()];
        }

        public async Task<IReadOnlyCollection<Budget>> GetBudgets()
        {
            var response = await ynabApi.GetBudgetsAsync();

            return response.Data?.Budgets ?? [ new Budget() ];
        }
        
        public async Task<BudgetMonth> GetBudgetMonth(Budget budget, DateOnly date)
        {
            var dateModified = date;

            if (date.Year > DateTime.UtcNow.Year)
            {
                dateModified = new DateOnly(DateTime.UtcNow.Year, date.Month, 1);
            }

            var dateString = dateModified.ToString("yyyy-MM-01");
            var response = await ynabApi.GetBudgetMonthAsync(budget.BudgetId, dateString);
        
            return response.Data?.Budget ?? new BudgetMonth();
        } 
        
        public Task<BudgetMonth> GetCurrentMonthBudget(Budget budget)
        {
            return GetBudgetMonth(budget, DateOnly.FromDateTime(DateTime.UtcNow));
        }
    }
}
