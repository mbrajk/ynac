namespace ynab.Transaction;

public interface ITransactionQueryService
{
    Task<IReadOnlyCollection<Transaction>> GetBudgetTransactionsAsync(Budget.Budget budget);
}