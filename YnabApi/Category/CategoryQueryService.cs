namespace ynac.YnabApi.Category
{
    public class CategoryQueryService(IYnabApi _ynabApi)
    {
        public async Task<IReadOnlyCollection<CategoryGroup>?> GetBudgetCategoriesAsync(Budget.Budget budget)
        {
            var response = await _ynabApi.GetBudgetCategoriesAsync(budget.Id);
            
            return response?.Data?.Groups;
        }
    }
}
