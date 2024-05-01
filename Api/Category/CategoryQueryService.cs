namespace YnabApi.Category
{
    public class CategoryQueryService(IYnabApi ynabApi) : ICategoryQueryService
    {
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategoriesAsync(Budget.Budget budget)
        {
            var response = await ynabApi.GetBudgetCategoriesAsync(budget.BudgetId);
            
            return response.Data?.Groups ?? new []{ new CategoryGroup() };
        }
    }
}
