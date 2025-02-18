using YnabApi.Account;
using YnabApi.Budget;
using YnabApi.Category;

namespace YnabApi
{
    public interface IYnabApi
    {
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id);

        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id);
    }
}
