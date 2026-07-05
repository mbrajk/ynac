namespace ynab.ScheduledTransaction
{
    public class ScheduledTransactionQueryService(IBudgetApi budgetApi) : IScheduledTransactionQueryService
    {
        public async Task<IReadOnlyCollection<ScheduledTransaction>> GetScheduledTransactions(Budget.Budget budget)
        {
            var response = await budgetApi.GetScheduledTransactionsAsync(budget.BudgetId);

            var scheduledTransactions = response.Data?.ScheduledTransactions ?? Array.Empty<ScheduledTransaction>();

            return scheduledTransactions
                .Where(scheduledTransaction => !scheduledTransaction.Deleted)
                .OrderBy(scheduledTransaction => scheduledTransaction.DateNext)
                .ToList();
        }
    }
}
