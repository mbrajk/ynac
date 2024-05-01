using YnabApi.Category;

namespace YnabApi.Budget
{
    public interface IBudgetQueryService
    {
        Task<IReadOnlyCollection<Budget>> GetBudgets();
        Task<BudgetMonth> GetBudgetMonth(string budgetId, DateOnly date);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Budget selectedBudget);
    }
}
