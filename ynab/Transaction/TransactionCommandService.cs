namespace ynab.Transaction
{
    public class TransactionCommandService(IBudgetApi budgetApi) : ITransactionCommandService
    {
        public async Task<Transaction?> CreateTransaction(Budget.Budget budget, SaveTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

            var request = new SaveTransactionRequest { Transaction = transaction };
            var response = await budgetApi.CreateTransactionAsync(budget.BudgetId, request);

            return response.Data?.Transaction ?? response.Data?.Transactions?.FirstOrDefault();
        }

        public async Task<Transaction?> UpdateTransaction(Budget.Budget budget, string transactionId, SaveTransaction transaction)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionId, nameof(transactionId));
            ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

            var request = new SaveTransactionRequest { Transaction = transaction };
            var response = await budgetApi.UpdateTransactionAsync(budget.BudgetId, transactionId, request);

            return response.Data?.Transaction;
        }

        public async Task<IReadOnlyCollection<Transaction>> UpdateTransactions(Budget.Budget budget, IReadOnlyCollection<SaveTransaction> transactions)
        {
            ArgumentNullException.ThrowIfNull(transactions, nameof(transactions));

            if (transactions.Count == 0)
            {
                return Array.Empty<Transaction>();
            }

            var request = new SaveTransactionsRequest { Transactions = transactions };
            var response = await budgetApi.UpdateTransactionsAsync(budget.BudgetId, request);

            return response.Data?.Transactions ?? Array.Empty<Transaction>();
        }

        public Task<IReadOnlyCollection<Transaction>> ApproveTransactions(Budget.Budget budget, IReadOnlyCollection<string> transactionIds)
        {
            ArgumentNullException.ThrowIfNull(transactionIds, nameof(transactionIds));

            var updates = transactionIds
                .Select(id => new SaveTransaction { Id = id, Approved = true })
                .ToList();

            return UpdateTransactions(budget, updates);
        }

        public Task<IReadOnlyCollection<Transaction>> CategorizeTransactions(Budget.Budget budget, IReadOnlyCollection<(string TransactionId, Guid CategoryId)> categoryAssignments)
        {
            ArgumentNullException.ThrowIfNull(categoryAssignments, nameof(categoryAssignments));

            var updates = categoryAssignments
                .Select(assignment => new SaveTransaction { Id = assignment.TransactionId, CategoryId = assignment.CategoryId })
                .ToList();

            return UpdateTransactions(budget, updates);
        }

        public async Task<Transaction?> DeleteTransaction(Budget.Budget budget, string transactionId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transactionId, nameof(transactionId));

            var response = await budgetApi.DeleteTransactionAsync(budget.BudgetId, transactionId);

            return response.Data?.Transaction;
        }

        public async Task<IReadOnlyCollection<string>> ImportLinkedAccountTransactions(Budget.Budget budget)
        {
            var response = await budgetApi.ImportTransactionsAsync(budget.BudgetId);

            return response.Data?.TransactionIds ?? Array.Empty<string>();
        }
    }
}
