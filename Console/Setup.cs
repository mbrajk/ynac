using Microsoft.Extensions.DependencyInjection;
using YnabApi;
using ynac.BudgetActions;
using ynac.OSFeatures;

namespace ynac;

public static class Setup
{
    public static ServiceProvider BuildServiceProvider(string token)
    {
        var services = new ServiceCollection();
        
        services.AddYnabApi(token);

        // other required dependencies
        services.AddSingleton<IYnabConsole, YnabConsole>();
        
        services.AddSingleton<IBudgetOpener, BudgetOpener>();
        services.AddSingleton<IBrowserOpener, WindowsBrowserOpener>();
        
        services.AddSingleton<IBudgetAction, ExitBudgetAction>();
        services.AddSingleton<IBudgetAction, ListTransactionsBudgetAction>(); 
        
        return services.BuildServiceProvider();
    }
}