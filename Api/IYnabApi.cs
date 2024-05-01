using Refit;
using YnabApi.Account;
using YnabApi.Budget;
using YnabApi.Category;

namespace YnabApi
{
    public interface IYnabApi
    {
        [Get("/budgets")]
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        [Get("/budgets/{id}/months/{month}")]
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        [Get("/budgets/{id}/categories")]
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(Guid id);

        [Get("/budgets/{id}/accounts")]
        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(Guid id);
    }
}
