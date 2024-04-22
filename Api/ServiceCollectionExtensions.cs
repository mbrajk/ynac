using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using YnabApi.Budget;

namespace YnabApi;

internal static class YnabOptions
{
    public static string Version => "v1";
    public static string Endpoint => "https://api.youneedabudget.com"; 
}

public static class YnabApiServiceCollectionExtensions
{
    public static IServiceCollection AddYnabApi(this IServiceCollection services, string token)
    {
        if (string.IsNullOrWhiteSpace("token"))
            throw new ArgumentException("Valid YNAB API token is required", nameof(token));
        
        services.AddRefitClient<IYnabApi>()
            .ConfigureHttpClient(
                httpClient =>
                {
                    var endpoint = YnabOptions.Endpoint; 
                    var version = YnabOptions.Version;

                    httpClient.BaseAddress = new Uri($"{endpoint}/{version}");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }
            )
            .AddPolicyHandler(
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(4, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)))
            );

        services.AddSingleton<IBudgetQueryService, BudgetQueryService>();
        return services;
    }
}