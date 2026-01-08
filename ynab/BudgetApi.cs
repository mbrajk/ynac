using System.Net.Http.Json;
using ynab.Account;
using ynab.Budget;
using ynab.Category;

namespace ynab;

internal class BudgetApi : IBudgetApi
{
    private readonly HttpClient _httpClient;
    public BudgetApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(BudgetApi));
    }
    
    private async Task<TResponse> ExecuteApiRequestAsync<TResponse>(Func<Task<TResponse?>> apiCall, TResponse defaultResponse) where TResponse : new()
    {
        try
        {
            return await apiCall() ?? defaultResponse;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new YnabAuthenticationException("Authentication failed. The provided API token is invalid or has expired.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new YnabApiException($"Failed to communicate with the YNAB API: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new YnabApiException("An unexpected error occurred while communicating with the YNAB API. Please try again later.", ex);
        }
    }
        
    public async Task<QueryResponse<BudgetResponse>> GetBudgetsAsync()
    {
        var path = "budgets";
        return await ExecuteApiRequestAsync(
            () => _httpClient.GetFromJsonAsync<QueryResponse<BudgetResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseBudgetResponse),
            new QueryResponse<BudgetResponse>()
        );
    }

    public async Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month)
    {
        var path = $"budgets/{id}/months/{month}";
        return await ExecuteApiRequestAsync(
            () => _httpClient.GetFromJsonAsync<QueryResponse<BudgetMonthResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseBudgetMonthResponse),
            new QueryResponse<BudgetMonthResponse>()
        );
    }

    public async Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id)
    {
        var path = $"budgets/{id}/categories";
        return await ExecuteApiRequestAsync(
            () => _httpClient.GetFromJsonAsync<QueryResponse<CategoryResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseCategoryResponse),
            new QueryResponse<CategoryResponse>()
        );
    }

    public async Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id)
    {
        var path = $"budgets/{id}/accounts";
        return await ExecuteApiRequestAsync(
            () => _httpClient.GetFromJsonAsync<QueryResponse<AccountResponse>>(path, YnabJsonSerializerContext.Default.QueryResponseAccountResponse),
            new QueryResponse<AccountResponse>()
        );
    }
}