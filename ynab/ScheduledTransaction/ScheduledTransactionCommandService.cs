namespace ynab.ScheduledTransaction
{
    public class ScheduledTransactionCommandService(IBudgetApi budgetApi) : IScheduledTransactionCommandService
    {
        public async Task<ScheduledTransaction?> CreateScheduledTransaction(Budget.Budget budget, SaveScheduledTransaction scheduledTransaction)
        {
            ArgumentNullException.ThrowIfNull(scheduledTransaction, nameof(scheduledTransaction));

            var request = new SaveScheduledTransactionRequest { ScheduledTransaction = scheduledTransaction };
            var response = await budgetApi.CreateScheduledTransactionAsync(budget.BudgetId, request);

            return response.Data?.ScheduledTransaction;
        }

        public async Task<ScheduledTransaction?> UpdateScheduledTransaction(Budget.Budget budget, Guid scheduledTransactionId, SaveScheduledTransaction scheduledTransaction)
        {
            ArgumentNullException.ThrowIfNull(scheduledTransaction, nameof(scheduledTransaction));

            var request = new SaveScheduledTransactionRequest { ScheduledTransaction = scheduledTransaction };
            var response = await budgetApi.UpdateScheduledTransactionAsync(budget.BudgetId, scheduledTransactionId.ToString(), request);

            return response.Data?.ScheduledTransaction;
        }

        public async Task<ScheduledTransaction?> DeleteScheduledTransaction(Budget.Budget budget, Guid scheduledTransactionId)
        {
            var response = await budgetApi.DeleteScheduledTransactionAsync(budget.BudgetId, scheduledTransactionId.ToString());

            return response.Data?.ScheduledTransaction;
        }
    }
}
