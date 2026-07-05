namespace ynab.Transaction;

/// <summary>
/// Server-side filters supported by the YNAB transactions endpoints.
/// </summary>
public enum TransactionsFilter
{
    None,
    Unapproved,
    Uncategorized,
}

/// <summary>
/// Provides query operations for retrieving transactions from YNAB.
/// </summary>
public interface ITransactionQueryService
{
    /// <summary>
    /// Retrieves transactions for the given budget, newest first. Deleted transactions are filtered out.
    /// </summary>
    /// <param name="budget">The budget to retrieve transactions for.</param>
    /// <param name="sinceDate">If provided, only transactions on or after this date are returned.</param>
    /// <param name="filter">An optional server-side filter (unapproved or uncategorized).</param>
    /// <returns>A read-only collection of transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> GetTransactions(Budget.Budget budget, DateOnly? sinceDate = null, TransactionsFilter filter = TransactionsFilter.None);

    /// <summary>
    /// Retrieves transactions for a single account, newest first. Deleted transactions are filtered out.
    /// </summary>
    /// <param name="budget">The budget the account belongs to.</param>
    /// <param name="accountId">The account to retrieve transactions for.</param>
    /// <param name="sinceDate">If provided, only transactions on or after this date are returned.</param>
    /// <returns>A read-only collection of transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> GetAccountTransactions(Budget.Budget budget, Guid accountId, DateOnly? sinceDate = null);

    /// <summary>
    /// Retrieves transactions for a single category, newest first. Deleted transactions are filtered out.
    /// </summary>
    /// <param name="budget">The budget the category belongs to.</param>
    /// <param name="categoryId">The category to retrieve transactions for.</param>
    /// <param name="sinceDate">If provided, only transactions on or after this date are returned.</param>
    /// <returns>A read-only collection of transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> GetCategoryTransactions(Budget.Budget budget, Guid categoryId, DateOnly? sinceDate = null);

    /// <summary>
    /// Retrieves transactions for a single payee, newest first. Deleted transactions are filtered out.
    /// </summary>
    /// <param name="budget">The budget the payee belongs to.</param>
    /// <param name="payeeId">The payee to retrieve transactions for.</param>
    /// <param name="sinceDate">If provided, only transactions on or after this date are returned.</param>
    /// <returns>A read-only collection of transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> GetPayeeTransactions(Budget.Budget budget, Guid payeeId, DateOnly? sinceDate = null);
}
