using YnabApi.Category;

namespace YnabApi.Budget
{
    public interface IBudgetQueryService
    {
        Task<IReadOnlyCollection<YnabApi.Budget.Budget>> GetBudgets();
        Task<BudgetMonth> GetBudgetMonth(Guid budgetId, DateOnly date);
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(YnabApi.Budget.Budget selectedBudget);
        Task<IReadOnlyCollection<Account.Account>> GetBudgetAccounts(YnabApi.Budget.Budget selectedBudget);
    }
}
