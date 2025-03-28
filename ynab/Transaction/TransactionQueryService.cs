namespace ynab.Transaction;

public class TransactionQueryService : ITransactionQueryService
{
    public Task<IReadOnlyCollection<Transaction>> GetBudgetTransactionsAsync(Budget.Budget budget)
    {
        throw new NotImplementedException();
    }
}