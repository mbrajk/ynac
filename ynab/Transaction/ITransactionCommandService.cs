namespace ynab.Transaction;

/// <summary>
/// Provides write operations for transactions in YNAB.
/// </summary>
public interface ITransactionCommandService
{
    /// <summary>
    /// Creates a single transaction.
    /// </summary>
    /// <param name="budget">The budget to create the transaction in.</param>
    /// <param name="transaction">The transaction to create. AccountId, Date and Amount are required by the API.</param>
    /// <returns>The created transaction, or null if creation failed.</returns>
    Task<Transaction?> CreateTransaction(Budget.Budget budget, SaveTransaction transaction);

    /// <summary>
    /// Updates a single existing transaction. Null properties are left unchanged.
    /// </summary>
    /// <param name="budget">The budget the transaction belongs to.</param>
    /// <param name="transactionId">The id of the transaction to update.</param>
    /// <param name="transaction">The fields to update.</param>
    /// <returns>The updated transaction, or null if the update failed.</returns>
    Task<Transaction?> UpdateTransaction(Budget.Budget budget, string transactionId, SaveTransaction transaction);

    /// <summary>
    /// Updates multiple existing transactions in a single request. Each item must carry its transaction Id;
    /// null properties are left unchanged.
    /// </summary>
    /// <param name="budget">The budget the transactions belong to.</param>
    /// <param name="transactions">The transactions to update.</param>
    /// <returns>The updated transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> UpdateTransactions(Budget.Budget budget, IReadOnlyCollection<SaveTransaction> transactions);

    /// <summary>
    /// Marks the given transactions as approved.
    /// </summary>
    /// <param name="budget">The budget the transactions belong to.</param>
    /// <param name="transactionIds">The ids of the transactions to approve.</param>
    /// <returns>The updated transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> ApproveTransactions(Budget.Budget budget, IReadOnlyCollection<string> transactionIds);

    /// <summary>
    /// Assigns a category to each of the given transactions.
    /// </summary>
    /// <param name="budget">The budget the transactions belong to.</param>
    /// <param name="categoryAssignments">Pairs of transaction id and the category id to assign.</param>
    /// <returns>The updated transactions.</returns>
    Task<IReadOnlyCollection<Transaction>> CategorizeTransactions(Budget.Budget budget, IReadOnlyCollection<(string TransactionId, Guid CategoryId)> categoryAssignments);

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    /// <param name="budget">The budget the transaction belongs to.</param>
    /// <param name="transactionId">The id of the transaction to delete.</param>
    /// <returns>The deleted transaction, or null if deletion failed.</returns>
    Task<Transaction?> DeleteTransaction(Budget.Budget budget, string transactionId);

    /// <summary>
    /// Asks YNAB to import available transactions from all linked accounts on the budget.
    /// </summary>
    /// <param name="budget">The budget to import transactions for.</param>
    /// <returns>The ids of the newly imported transactions.</returns>
    Task<IReadOnlyCollection<string>> ImportLinkedAccountTransactions(Budget.Budget budget);
}
