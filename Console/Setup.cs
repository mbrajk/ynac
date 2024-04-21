using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YnabApi;

namespace ynac;

public static class Setup
{
    public static ServiceProvider BuildServiceProvider()
    {
        var configurationRoot =  new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection();

        var token = configurationRoot["YnabApi:Token"]; 
        services.AddYnabApi(token);

        // other required dependencies
        services.AddSingleton<IYnabConsole, YnabConsole>();

        return services.BuildServiceProvider();
    }
}