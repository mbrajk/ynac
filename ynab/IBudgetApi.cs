using ynab.Account;
using ynab.Budget;
using ynab.Category;

namespace ynab
{
    public interface IBudgetApi
    {
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id);

        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id);
    }
}
