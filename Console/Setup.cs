using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YnabApi;

namespace ynac;

public static class Setup
{
    public static ServiceProvider BuildServiceProvider(string token)
    {
        var services = new ServiceCollection();
        
        services.AddYnabApi(token);

        // other required dependencies
        services.AddSingleton<IYnabConsole, YnabConsole>();

        return services.BuildServiceProvider();
    }
}