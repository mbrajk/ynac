using YnabApi.Category;

namespace YnabApi.Budget
{
    public interface IBudgetQueryService
    {
        Task<IReadOnlyCollection<Budget>> GetBudgets();
        Task<BudgetMonth> GetBudgetMonth(Guid budgetId, DateOnly date);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Budget selectedBudget);
        Task<IReadOnlyCollection<Account.Account>> GetBudgetAccounts(Budget selectedBudget);
    }
}
