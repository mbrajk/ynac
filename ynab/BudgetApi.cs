using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Payee;
using ynab.ScheduledTransaction;
using ynab.Transaction;
using ynab.User;

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

    private Task<QueryResponse<TResponse>> GetAsync<TResponse>(string path, JsonTypeInfo<QueryResponse<TResponse>> responseTypeInfo) where TResponse : class
    {
        return ExecuteApiRequestAsync(
            () => _httpClient.GetFromJsonAsync(path, responseTypeInfo),
            new QueryResponse<TResponse>()
        );
    }

    private Task<QueryResponse<TResponse>> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<QueryResponse<TResponse>> responseTypeInfo) where TResponse : class
    {
        return ExecuteApiRequestAsync(
            async () =>
            {
                using var httpRequest = new HttpRequestMessage(method, path);
                httpRequest.Content = JsonContent.Create(request, requestTypeInfo);

                using var httpResponse = await _httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();

                return await httpResponse.Content.ReadFromJsonAsync(responseTypeInfo);
            },
            new QueryResponse<TResponse>()
        );
    }

    private Task<QueryResponse<TResponse>> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        JsonTypeInfo<QueryResponse<TResponse>> responseTypeInfo) where TResponse : class
    {
        return ExecuteApiRequestAsync(
            async () =>
            {
                using var httpRequest = new HttpRequestMessage(method, path);

                using var httpResponse = await _httpClient.SendAsync(httpRequest);
                httpResponse.EnsureSuccessStatusCode();

                return await httpResponse.Content.ReadFromJsonAsync(responseTypeInfo);
            },
            new QueryResponse<TResponse>()
        );
    }

    private static string AppendQuery(string path, params (string Name, string? Value)[] parameters)
    {
        var query = string.Join("&", parameters
            .Where(parameter => !string.IsNullOrEmpty(parameter.Value))
            .Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value!)}"));

        return string.IsNullOrEmpty(query) ? path : $"{path}?{query}";
    }

    public Task<QueryResponse<UserResponse>> GetUserAsync()
    {
        return GetAsync("user", YnabJsonSerializerContext.Default.QueryResponseUserResponse);
    }

    public Task<QueryResponse<BudgetResponse>> GetBudgetsAsync()
    {
        return GetAsync("budgets", YnabJsonSerializerContext.Default.QueryResponseBudgetResponse);
    }

    public Task<QueryResponse<BudgetMonthResponse>> GetBudgetMonthAsync(string id, string month)
    {
        return GetAsync($"budgets/{id}/months/{month}", YnabJsonSerializerContext.Default.QueryResponseBudgetMonthResponse);
    }

    public Task<QueryResponse<BudgetMonthsResponse>> GetBudgetMonthsAsync(string id)
    {
        return GetAsync($"budgets/{id}/months", YnabJsonSerializerContext.Default.QueryResponseBudgetMonthsResponse);
    }

    public Task<QueryResponse<CategoryResponse>> GetBudgetCategoriesAsync(string id)
    {
        return GetAsync($"budgets/{id}/categories", YnabJsonSerializerContext.Default.QueryResponseCategoryResponse);
    }

    public Task<QueryResponse<SingleCategoryResponse>> UpdateMonthCategoryAsync(string id, string month, string categoryId, SaveMonthCategoryRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            $"budgets/{id}/months/{month}/categories/{categoryId}",
            request,
            YnabJsonSerializerContext.Default.SaveMonthCategoryRequest,
            YnabJsonSerializerContext.Default.QueryResponseSingleCategoryResponse);
    }

    public Task<QueryResponse<SingleCategoryResponse>> UpdateCategoryAsync(string id, string categoryId, SaveCategoryRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            $"budgets/{id}/categories/{categoryId}",
            request,
            YnabJsonSerializerContext.Default.SaveCategoryRequest,
            YnabJsonSerializerContext.Default.QueryResponseSingleCategoryResponse);
    }

    public Task<QueryResponse<AccountResponse>> GetBudgetAccountsAsync(string id)
    {
        return GetAsync($"budgets/{id}/accounts", YnabJsonSerializerContext.Default.QueryResponseAccountResponse);
    }

    public Task<QueryResponse<PayeesResponse>> GetBudgetPayeesAsync(string id)
    {
        return GetAsync($"budgets/{id}/payees", YnabJsonSerializerContext.Default.QueryResponsePayeesResponse);
    }

    public Task<QueryResponse<PayeeResponse>> UpdatePayeeAsync(string id, string payeeId, SavePayeeRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            $"budgets/{id}/payees/{payeeId}",
            request,
            YnabJsonSerializerContext.Default.SavePayeeRequest,
            YnabJsonSerializerContext.Default.QueryResponsePayeeResponse);
    }

    public Task<QueryResponse<TransactionsResponse>> GetTransactionsAsync(string id, string? sinceDate = null, string? type = null)
    {
        var path = AppendQuery($"budgets/{id}/transactions", ("since_date", sinceDate), ("type", type));
        return GetAsync(path, YnabJsonSerializerContext.Default.QueryResponseTransactionsResponse);
    }

    public Task<QueryResponse<TransactionsResponse>> GetAccountTransactionsAsync(string id, string accountId, string? sinceDate = null)
    {
        var path = AppendQuery($"budgets/{id}/accounts/{accountId}/transactions", ("since_date", sinceDate));
        return GetAsync(path, YnabJsonSerializerContext.Default.QueryResponseTransactionsResponse);
    }

    public Task<QueryResponse<TransactionsResponse>> GetCategoryTransactionsAsync(string id, string categoryId, string? sinceDate = null)
    {
        var path = AppendQuery($"budgets/{id}/categories/{categoryId}/transactions", ("since_date", sinceDate));
        return GetAsync(path, YnabJsonSerializerContext.Default.QueryResponseTransactionsResponse);
    }

    public Task<QueryResponse<TransactionsResponse>> GetPayeeTransactionsAsync(string id, string payeeId, string? sinceDate = null)
    {
        var path = AppendQuery($"budgets/{id}/payees/{payeeId}/transactions", ("since_date", sinceDate));
        return GetAsync(path, YnabJsonSerializerContext.Default.QueryResponseTransactionsResponse);
    }

    public Task<QueryResponse<TransactionResponse>> GetTransactionAsync(string id, string transactionId)
    {
        return GetAsync($"budgets/{id}/transactions/{transactionId}", YnabJsonSerializerContext.Default.QueryResponseTransactionResponse);
    }

    public Task<QueryResponse<SaveTransactionsResponse>> CreateTransactionAsync(string id, SaveTransactionRequest request)
    {
        return SendAsync(
            HttpMethod.Post,
            $"budgets/{id}/transactions",
            request,
            YnabJsonSerializerContext.Default.SaveTransactionRequest,
            YnabJsonSerializerContext.Default.QueryResponseSaveTransactionsResponse);
    }

    public Task<QueryResponse<TransactionResponse>> UpdateTransactionAsync(string id, string transactionId, SaveTransactionRequest request)
    {
        return SendAsync(
            HttpMethod.Put,
            $"budgets/{id}/transactions/{transactionId}",
            request,
            YnabJsonSerializerContext.Default.SaveTransactionRequest,
            YnabJsonSerializerContext.Default.QueryResponseTransactionResponse);
    }

    public Task<QueryResponse<SaveTransactionsResponse>> UpdateTransactionsAsync(string id, SaveTransactionsRequest request)
    {
        return SendAsync(
            HttpMethod.Patch,
            $"budgets/{id}/transactions",
            request,
            YnabJsonSerializerContext.Default.SaveTransactionsRequest,
            YnabJsonSerializerContext.Default.QueryResponseSaveTransactionsResponse);
    }

    public Task<QueryResponse<TransactionResponse>> DeleteTransactionAsync(string id, string transactionId)
    {
        return SendAsync(
            HttpMethod.Delete,
            $"budgets/{id}/transactions/{transactionId}",
            YnabJsonSerializerContext.Default.QueryResponseTransactionResponse);
    }

    public Task<QueryResponse<TransactionImportResponse>> ImportTransactionsAsync(string id)
    {
        return SendAsync(
            HttpMethod.Post,
            $"budgets/{id}/transactions/import",
            YnabJsonSerializerContext.Default.QueryResponseTransactionImportResponse);
    }

    public Task<QueryResponse<ScheduledTransactionsResponse>> GetScheduledTransactionsAsync(string id)
    {
        return GetAsync($"budgets/{id}/scheduled_transactions", YnabJsonSerializerContext.Default.QueryResponseScheduledTransactionsResponse);
    }

    public Task<QueryResponse<ScheduledTransactionResponse>> CreateScheduledTransactionAsync(string id, SaveScheduledTransactionRequest request)
    {
        return SendAsync(
            HttpMethod.Post,
            $"budgets/{id}/scheduled_transactions",
            request,
            YnabJsonSerializerContext.Default.SaveScheduledTransactionRequest,
            YnabJsonSerializerContext.Default.QueryResponseScheduledTransactionResponse);
    }

    public Task<QueryResponse<ScheduledTransactionResponse>> UpdateScheduledTransactionAsync(string id, string scheduledTransactionId, SaveScheduledTransactionRequest request)
    {
        return SendAsync(
            HttpMethod.Put,
            $"budgets/{id}/scheduled_transactions/{scheduledTransactionId}",
            request,
            YnabJsonSerializerContext.Default.SaveScheduledTransactionRequest,
            YnabJsonSerializerContext.Default.QueryResponseScheduledTransactionResponse);
    }

    public Task<QueryResponse<ScheduledTransactionResponse>> DeleteScheduledTransactionAsync(string id, string scheduledTransactionId)
    {
        return SendAsync(
            HttpMethod.Delete,
            $"budgets/{id}/scheduled_transactions/{scheduledTransactionId}",
            YnabJsonSerializerContext.Default.QueryResponseScheduledTransactionResponse);
    }
}
