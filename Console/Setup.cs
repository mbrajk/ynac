using System.Runtime.InteropServices;
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
        
        services.AddSingleton<IBudgetBrowserOpener, BudgetBrowserOpener>();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // this could be determined from within OSFeatures
            services.AddSingleton<IBrowserOpener, WindowsBrowserOpener>();
        }
        else
        {
            services.AddSingleton<IBrowserOpener, UnsupportedOsBrowserOpener>();
        }
        
        services.AddSingleton<IBudgetPrompter, BudgetPrompter>();
        services.AddSingleton<IBudgetSelector, BudgetSelector>();
        
        services.AddSingleton<IBudgetAction, ExitBudgetAction>();
        services.AddSingleton<IBudgetAction, ListTransactionsBudgetAction>(); 
        
        return services.BuildServiceProvider();
    }
}