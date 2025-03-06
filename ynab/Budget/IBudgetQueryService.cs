using ynab.Category;

namespace ynab.Budget
{
    public interface IBudgetQueryService
    {
        Task<IReadOnlyCollection<Budget>> GetBudgets();
        Task<BudgetMonth> GetBudgetMonth(Budget budget, DateOnly date);
        Task<BudgetMonth> GetCurrentMonthBudget(Budget selectedBudget);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Action<BudgetCategorySearchOptions> configureOptions);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(BudgetCategorySearchOptions budgetCategorySearchSettings);
    }
}
