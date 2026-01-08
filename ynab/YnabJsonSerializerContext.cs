using System.Text.Json.Serialization;
using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Transaction;

namespace ynab;

[JsonSerializable(typeof(QueryResponse<BudgetResponse>))]
[JsonSerializable(typeof(BudgetResponse))]
[JsonSerializable(typeof(Budget.Budget))]

[JsonSerializable(typeof(QueryResponse<CategoryResponse>))]
[JsonSerializable(typeof(CategoryResponse))]
[JsonSerializable(typeof(Category.Category))]
[JsonSerializable(typeof(CategoryGroup))]

[JsonSerializable(typeof(QueryResponse<AccountResponse>))]
[JsonSerializable(typeof(AccountResponse))]
[JsonSerializable(typeof(Account.Account))]

[JsonSerializable(typeof(QueryResponse<BudgetMonthResponse>))]
[JsonSerializable(typeof(BudgetMonthResponse))]
[JsonSerializable(typeof(BudgetMonth))]

[JsonSerializable(typeof(QueryResponse<TransactionResponse>))]
[JsonSerializable(typeof(TransactionResponse))]
[JsonSerializable(typeof(Transaction.Transaction))]

[JsonSerializable(typeof(QueryResponse<UpdateTransactionResponse>))]
[JsonSerializable(typeof(UpdateTransactionResponse))]
[JsonSerializable(typeof(UpdateTransactionRequest))]
[JsonSerializable(typeof(UpdateTransactionData))]
internal partial class YnabJsonSerializerContext : JsonSerializerContext
{
}