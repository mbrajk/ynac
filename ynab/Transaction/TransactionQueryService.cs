using ynab.Budget;

namespace ynab.Transaction;

internal class TransactionQueryService(IBudgetApi budgetApi) : ITransactionQueryService
{
    public async Task<IReadOnlyCollection<Transaction>> GetUnapprovedTransactions(Budget.Budget budget)
    {
        var response = await budgetApi.GetTransactionsAsync(budget.BudgetId);
        
        if (response.Data?.Transactions == null)
        {
            return Array.Empty<Transaction>();
        }

        return response.Data.Transactions
            .Where(t => !t.Approved && !t.Deleted)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public async Task<Transaction?> ApproveTransaction(Budget.Budget budget, string transactionId)
    {
        var request = new UpdateTransactionRequest
        {
            Transaction = new UpdateTransactionData
            {
                Id = transactionId,
                Approved = true
            }
        };

        var response = await budgetApi.UpdateTransactionAsync(budget.BudgetId, transactionId, request);
        
        return response.Data?.Transaction;
    }
}
