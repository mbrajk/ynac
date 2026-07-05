namespace ynab.Payee
{
    public class PayeeQueryService(IBudgetApi budgetApi) : IPayeeQueryService
    {
        public async Task<IReadOnlyCollection<Payee>> GetBudgetPayees(Budget.Budget budget)
        {
            var response = await budgetApi.GetBudgetPayeesAsync(budget.BudgetId);

            var payees = response.Data?.Payees ?? Array.Empty<Payee>();

            return payees.Where(payee => !payee.Deleted).ToList();
        }
    }
}
