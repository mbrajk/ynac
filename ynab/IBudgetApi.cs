using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Payee;
using ynab.ScheduledTransaction;
using ynab.Transaction;
using ynab.User;

namespace ynab
{
    public interface IBudgetApi
    {
        // user
        internal Task<QueryResponse<UserResponse>> GetUserAsync();

        // budgets and months
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        internal Task<QueryResponse<BudgetMonthsResponse>> GetBudgetMonthsAsync(string id);

        // categories
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id);
        internal Task<QueryResponse<SingleCategoryResponse>> UpdateMonthCategoryAsync(string id, string month, string categoryId, SaveMonthCategoryRequest request);
        internal Task<QueryResponse<SingleCategoryResponse>> UpdateCategoryAsync(string id, string categoryId, SaveCategoryRequest request);

        // accounts
        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id);

        // payees
        internal Task<QueryResponse<PayeesResponse>> GetBudgetPayeesAsync(string id);
        internal Task<QueryResponse<PayeeResponse>> UpdatePayeeAsync(string id, string payeeId, SavePayeeRequest request);

        // transactions
        internal Task<QueryResponse<TransactionsResponse>> GetTransactionsAsync(string id, string? sinceDate = null, string? type = null);
        internal Task<QueryResponse<TransactionsResponse>> GetAccountTransactionsAsync(string id, string accountId, string? sinceDate = null);
        internal Task<QueryResponse<TransactionsResponse>> GetCategoryTransactionsAsync(string id, string categoryId, string? sinceDate = null);
        internal Task<QueryResponse<TransactionsResponse>> GetPayeeTransactionsAsync(string id, string payeeId, string? sinceDate = null);
        internal Task<QueryResponse<TransactionResponse>> GetTransactionAsync(string id, string transactionId);
        internal Task<QueryResponse<SaveTransactionsResponse>> CreateTransactionAsync(string id, SaveTransactionRequest request);
        internal Task<QueryResponse<TransactionResponse>> UpdateTransactionAsync(string id, string transactionId, SaveTransactionRequest request);
        internal Task<QueryResponse<SaveTransactionsResponse>> UpdateTransactionsAsync(string id, SaveTransactionsRequest request);
        internal Task<QueryResponse<TransactionResponse>> DeleteTransactionAsync(string id, string transactionId);
        internal Task<QueryResponse<TransactionImportResponse>> ImportTransactionsAsync(string id);

        // scheduled transactions
        internal Task<QueryResponse<ScheduledTransactionsResponse>> GetScheduledTransactionsAsync(string id);
        internal Task<QueryResponse<ScheduledTransactionResponse>> CreateScheduledTransactionAsync(string id, SaveScheduledTransactionRequest request);
        internal Task<QueryResponse<ScheduledTransactionResponse>> UpdateScheduledTransactionAsync(string id, string scheduledTransactionId, SaveScheduledTransactionRequest request);
        internal Task<QueryResponse<ScheduledTransactionResponse>> DeleteScheduledTransactionAsync(string id, string scheduledTransactionId);
    }
}
