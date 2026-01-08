using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Transaction;

namespace ynab
{
    public interface IBudgetApi
    {
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id);

        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id);

        internal Task<QueryResponse<TransactionResponse>> GetTransactionsAsync(string budgetId);
        
        internal Task<QueryResponse<UpdateTransactionResponse>> UpdateTransactionAsync(string budgetId, string transactionId, UpdateTransactionRequest request);
    }
}
