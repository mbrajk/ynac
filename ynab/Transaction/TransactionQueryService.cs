namespace ynab.Transaction
{
    public class TransactionQueryService(IBudgetApi budgetApi) : ITransactionQueryService
    {
        public async Task<IReadOnlyCollection<Transaction>> GetTransactions(Budget.Budget budget, DateOnly? sinceDate = null, TransactionsFilter filter = TransactionsFilter.None)
        {
            var response = await budgetApi.GetTransactionsAsync(budget.BudgetId, ToDateString(sinceDate), ToTypeString(filter));

            return Sort(response.Data?.Transactions);
        }

        public async Task<IReadOnlyCollection<Transaction>> GetAccountTransactions(Budget.Budget budget, Guid accountId, DateOnly? sinceDate = null)
        {
            var response = await budgetApi.GetAccountTransactionsAsync(budget.BudgetId, accountId.ToString(), ToDateString(sinceDate));

            return Sort(response.Data?.Transactions);
        }

        public async Task<IReadOnlyCollection<Transaction>> GetCategoryTransactions(Budget.Budget budget, Guid categoryId, DateOnly? sinceDate = null)
        {
            var response = await budgetApi.GetCategoryTransactionsAsync(budget.BudgetId, categoryId.ToString(), ToDateString(sinceDate));

            return Sort(response.Data?.Transactions);
        }

        public async Task<IReadOnlyCollection<Transaction>> GetPayeeTransactions(Budget.Budget budget, Guid payeeId, DateOnly? sinceDate = null)
        {
            var response = await budgetApi.GetPayeeTransactionsAsync(budget.BudgetId, payeeId.ToString(), ToDateString(sinceDate));

            return Sort(response.Data?.Transactions);
        }

        internal static string? ToDateString(DateOnly? sinceDate) => sinceDate?.ToString("yyyy-MM-dd");

        internal static string? ToTypeString(TransactionsFilter filter) => filter switch
        {
            TransactionsFilter.Unapproved => "unapproved",
            TransactionsFilter.Uncategorized => "uncategorized",
            _ => null,
        };

        private static IReadOnlyCollection<Transaction> Sort(IReadOnlyCollection<Transaction>? transactions)
        {
            if (transactions == null)
            {
                return Array.Empty<Transaction>();
            }

            return transactions
                .Where(transaction => !transaction.Deleted)
                .OrderByDescending(transaction => transaction.Date)
                .ToList();
        }
    }
}
