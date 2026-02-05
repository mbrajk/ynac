using ynab.Budget;

namespace ynab.Transaction;

public interface ITransactionQueryService
{
    Task<IReadOnlyCollection<Transaction>> GetUnapprovedTransactions(Budget.Budget budget);
    Task<Transaction?> ApproveTransaction(Budget.Budget budget, string transactionId);
}
