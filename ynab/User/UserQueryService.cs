namespace ynab.User
{
    public class UserQueryService(IBudgetApi budgetApi) : IUserQueryService
    {
        public async Task<Guid> GetAuthenticatedUserId()
        {
            var response = await budgetApi.GetUserAsync();

            return response.Data?.User?.Id ?? Guid.Empty;
        }
    }
}
