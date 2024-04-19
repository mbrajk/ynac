using BudgetSync.YnabApi;
using BudgetSync.YnabApi.Budget;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace Ynac;

public static class Setup
{
    public static ServiceProvider BuildServiceProvider()
    {
        var configurationRoot =  new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var services = new ServiceCollection();

        // refit http client
        services
            .AddRefitClient<IYnabApi>()
            .ConfigureHttpClient(
                httpClient =>
                {
                    var endpoint = configurationRoot["YnabApi:Endpoint"];
                    var version = configurationRoot["YnabApi:Version"];
                    var token = configurationRoot["YnabApi:Token"];

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

        // other required dependencies
        services.AddSingleton<IYnabConsole, YnabConsole>();
        services.AddSingleton<IBudgetQueryService, BudgetQueryService>();

        return services.BuildServiceProvider();
    }
}