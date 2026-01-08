using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using ynab.Account;
using ynab.Budget;
using ynab.Category;
using ynab.Transaction;

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
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Valid YNAB API token is required", nameof(token));

        services.AddHttpClient(nameof(BudgetApi))
            .ConfigureHttpClient(
                httpClient =>
                {
                    var endpoint = YnabOptions.Endpoint;
                    var version = YnabOptions.Version;

                    httpClient.BaseAddress = new Uri($"{endpoint}/{version}/");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                {
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    }
                }).AddStandardResilienceHandler();

        services.AddSingleton<IBudgetApi, BudgetApi>();
        services.AddSingleton<IBudgetQueryService, BudgetQueryService>();
        services.AddSingleton<ICategoryQueryService, CategoryQueryService>();
        services.AddSingleton<IAccountQueryService, AccountQueryService>();
        services.AddSingleton<ITransactionQueryService, TransactionQueryService>();
        
        return services;
    }
}