namespace ynab.Payee
{
    public class PayeeCommandService(IBudgetApi budgetApi) : IPayeeCommandService
    {
        public async Task<Payee?> RenamePayee(Budget.Budget budget, Guid payeeId, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            var request = new SavePayeeRequest
            {
                Payee = new SavePayee { Name = name }
            };

            var response = await budgetApi.UpdatePayeeAsync(budget.BudgetId, payeeId.ToString(), request);

            return response.Data?.Payee;
        }
    }
}
