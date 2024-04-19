using Refit;
using ynac.YnabApi.Account;
using ynac.YnabApi.Budget;
using ynac.YnabApi.Category;

namespace ynac.YnabApi
{
    public interface IYnabApi
    {
        [Get("/budgets")]
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        [Get("/budgets/{id}/months/{month}")]
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(Guid id, string month);
        [Get("/budgets/{id}/categories")]
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(Guid id);

        [Get("/budgets/{id}/accounts")]
        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(Guid id);
    }
}
