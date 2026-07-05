using System.Text.Json.Serialization;
using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Payee;
using ynab.ScheduledTransaction;
using ynab.Transaction;
using ynab.User;

namespace ynab;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]

[JsonSerializable(typeof(QueryResponse<UserResponse>))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(User.User))]

[JsonSerializable(typeof(QueryResponse<BudgetResponse>))]
[JsonSerializable(typeof(BudgetResponse))]
[JsonSerializable(typeof(Budget.Budget))]

[JsonSerializable(typeof(QueryResponse<CategoryResponse>))]
[JsonSerializable(typeof(CategoryResponse))]
[JsonSerializable(typeof(Category.Category))]
[JsonSerializable(typeof(CategoryGroup))]

[JsonSerializable(typeof(QueryResponse<SingleCategoryResponse>))]
[JsonSerializable(typeof(SingleCategoryResponse))]
[JsonSerializable(typeof(SaveMonthCategoryRequest))]
[JsonSerializable(typeof(SaveMonthCategory))]
[JsonSerializable(typeof(SaveCategoryRequest))]
[JsonSerializable(typeof(SaveCategory))]

[JsonSerializable(typeof(QueryResponse<AccountResponse>))]
[JsonSerializable(typeof(AccountResponse))]
[JsonSerializable(typeof(Account.Account))]

[JsonSerializable(typeof(QueryResponse<BudgetMonthResponse>))]
[JsonSerializable(typeof(BudgetMonthResponse))]
[JsonSerializable(typeof(QueryResponse<BudgetMonthsResponse>))]
[JsonSerializable(typeof(BudgetMonthsResponse))]
[JsonSerializable(typeof(BudgetMonth))]

[JsonSerializable(typeof(QueryResponse<PayeesResponse>))]
[JsonSerializable(typeof(PayeesResponse))]
[JsonSerializable(typeof(QueryResponse<PayeeResponse>))]
[JsonSerializable(typeof(PayeeResponse))]
[JsonSerializable(typeof(Payee.Payee))]
[JsonSerializable(typeof(SavePayeeRequest))]
[JsonSerializable(typeof(SavePayee))]

[JsonSerializable(typeof(QueryResponse<TransactionsResponse>))]
[JsonSerializable(typeof(TransactionsResponse))]
[JsonSerializable(typeof(QueryResponse<TransactionResponse>))]
[JsonSerializable(typeof(TransactionResponse))]
[JsonSerializable(typeof(QueryResponse<SaveTransactionsResponse>))]
[JsonSerializable(typeof(SaveTransactionsResponse))]
[JsonSerializable(typeof(QueryResponse<TransactionImportResponse>))]
[JsonSerializable(typeof(TransactionImportResponse))]
[JsonSerializable(typeof(Transaction.Transaction))]
[JsonSerializable(typeof(SubTransaction))]
[JsonSerializable(typeof(SaveTransactionRequest))]
[JsonSerializable(typeof(SaveTransactionsRequest))]
[JsonSerializable(typeof(SaveTransaction))]

[JsonSerializable(typeof(QueryResponse<ScheduledTransactionsResponse>))]
[JsonSerializable(typeof(ScheduledTransactionsResponse))]
[JsonSerializable(typeof(QueryResponse<ScheduledTransactionResponse>))]
[JsonSerializable(typeof(ScheduledTransactionResponse))]
[JsonSerializable(typeof(ScheduledTransaction.ScheduledTransaction))]
[JsonSerializable(typeof(SaveScheduledTransactionRequest))]
[JsonSerializable(typeof(SaveScheduledTransaction))]
internal partial class YnabJsonSerializerContext : JsonSerializerContext
{
}
