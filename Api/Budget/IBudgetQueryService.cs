using YnabApi.Category;

namespace YnabApi.Budget
{
    public interface IBudgetQueryService
    {
        Task<IReadOnlyCollection<Budget>> GetBudgets();
        Task<BudgetMonth> GetBudgetMonth(Budget budget, DateOnly date);
        Task<BudgetMonth> GetCurrentMonthBudget(Budget selectedBudget);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Budget selectedBudget, string? categoryFilter = null);
    }
}
