using System.Diagnostics;
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
            var budgets = new QueryResponse<BudgetResponse>();
            try
            {
                budgets = await _httpClient.GetFromJsonAsync<QueryResponse<BudgetResponse>>(path) ?? budgets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return budgets;
        }

        public async Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month)
        {
            var path = $"budgets/{id}/months/{month}";
            var budget = new QueryResponse<BudgetMonthResponse>();
            try
            {
                budget = await _httpClient.GetFromJsonAsync<QueryResponse<BudgetMonthResponse>>(path) ?? budget;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            
            return budget;
        }

        public async Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id)
        {

            var path = $"budgets/{id}/categories";
            var categories = new QueryResponse<CategoryResponse>();
            try
            {
                categories = await _httpClient.GetFromJsonAsync<QueryResponse<CategoryResponse>>(path) ?? categories;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return categories;
        }

        public async Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id)
        {

            var path = $"budgets/{id}/accounts";
            var accounts = new QueryResponse<AccountResponse>();
            try
            {
                accounts = await _httpClient.GetFromJsonAsync<QueryResponse<AccountResponse>>(path) ?? accounts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return accounts;
        }
    }
}
