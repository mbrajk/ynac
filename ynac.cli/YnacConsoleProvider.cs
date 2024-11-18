using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using YnabApi;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.OSFeatures;

namespace ynac;

public static class YnacConsoleProvider
{
    public static ServiceProvider BuildYnacServices(string token)
    {
        var services = new ServiceCollection();
        
        services.AddYnabApi(token);

        // other required dependencies
        services.AddSingleton<IYnacConsole, YnacConsole>();
        
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
        services.AddSingleton<IConsolePrompt, ConsolePrompt>();
        services.AddSingleton<IBudgetSelector, BudgetSelector>();
        
        services.AddSingleton<IBudgetAction, ExitBudgetAction>();
        // add back when implemented
        //services.AddSingleton<IBudgetAction, ListTransactionsBudgetAction>(); 
        
        return services.BuildServiceProvider();
    }
}