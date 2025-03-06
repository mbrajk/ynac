using System.Text.Json.Serialization;
using ynab.Account;
using ynab.Budget;
using ynab.Category;

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
internal partial class YnabJsonSerializerContext : JsonSerializerContext
{
}