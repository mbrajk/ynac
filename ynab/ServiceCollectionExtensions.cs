using Microsoft.Extensions.DependencyInjection;
using ynab.Account;
using ynab.Budget;
using ynab.Category;

namespace ynab;

internal static class YnabOptions
{
    public static string Version => "v1";
    public static string Endpoint => "https://api.ynab.com"; 
}

public static class YnabApiServiceCollectionExtensions
{
    public static IServiceCollection AddYnabApi(this IServiceCollection services, string token)
    {
        if (string.IsNullOrWhiteSpace("token"))
            throw new ArgumentException("Valid YNAB API token is required", nameof(token));

        services.AddHttpClient(nameof(YnabApi))
            .ConfigureHttpClient(
                httpClient =>
                {
                    var endpoint = YnabOptions.Endpoint;
                    var version = YnabOptions.Version;

                    httpClient.BaseAddress = new Uri($"{endpoint}/{version}/");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }).AddStandardResilienceHandler();

        services.AddSingleton<IYnabApi, YnabApi>();
        services.AddSingleton<IBudgetQueryService, BudgetQueryService>();
        services.AddSingleton<ICategoryQueryService, CategoryQueryService>();
        services.AddSingleton<IAccountQueryService, AccountQueryService>();
        
        return services;
    }
}