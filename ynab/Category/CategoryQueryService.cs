namespace ynab.Category
{
    public class CategoryQueryService(IBudgetApi budgetApi) : ICategoryQueryService
    {
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategoriesAsync(Budget.Budget budget)
        {
            var response = await budgetApi.GetBudgetCategoriesAsync(budget.BudgetId);
            
            return response.Data?.Groups ?? new []{ new CategoryGroup() };
        }
    }
}
