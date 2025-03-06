using System.Net.Http.Json;
using ynab.Account;
using ynab.Budget;
using ynab.Category;

namespace ynab;

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
            budgets = await _httpClient.GetFromJsonAsync<QueryResponse<BudgetResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseBudgetResponse) ?? budgets;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return budgets;
    }

    public async Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month)
    {
        var path = $"budgets/{id}/months/{month}";
        var budget = new QueryResponse<BudgetMonthResponse>();
        try
        {
            budget = await _httpClient.GetFromJsonAsync<QueryResponse<BudgetMonthResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseBudgetMonthResponse) ?? budget;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
            
        return budget;
    }

    public async Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id)
    {

        var path = $"budgets/{id}/categories";
        var categories = new QueryResponse<CategoryResponse>();
        try
        {
            categories = await _httpClient.GetFromJsonAsync<QueryResponse<CategoryResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseCategoryResponse) ?? categories;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return categories;
    }

    public async Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id)
    {

        var path = $"budgets/{id}/accounts";
        var accounts = new QueryResponse<AccountResponse>();
        try
        {
            accounts = await _httpClient.GetFromJsonAsync<QueryResponse<AccountResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseAccountResponse) ?? accounts;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return accounts;
    }
}