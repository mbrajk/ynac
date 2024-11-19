using System.Net.Http.Json;
using Refit;
using YnabApi.Account;
using YnabApi.Budget;
using YnabApi.Category;

namespace YnabApi
{
    public interface IYnabApi
    {
        [Get("/budgets")]
        internal Task<QueryResponse<BudgetResponse>> GetBudgetsAsync();
        [Get("/budgets/{id}/months/{month}")]
        internal Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month);
        [Get("/budgets/{id}/categories")]
        internal Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id);

        [Get("/budgets/{id}/accounts")]
        internal Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id);
    }

    public class YnabApi : IYnabApi
    {
        private readonly HttpClient _httpClient;
        public YnabApi(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(YnabApi));
        }
        
        public async Task<QueryResponse<BudgetResponse>> GetBudgetsAsync()
        {
            var path = "budgets";
            var budgets = await _httpClient.GetFromJsonAsync<QueryResponse<BudgetResponse>>(path);
            return budgets;
        }

        public Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month)
        {
            throw new NotImplementedException();
        }

        public Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
