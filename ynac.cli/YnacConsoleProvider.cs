using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using ynab;
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
            services.AddSingleton<IBrowserOpener, WindowsBrowserOpener>();
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            services.AddSingleton<IBrowserOpener, OSXBrowserOpener>();
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            services.AddSingleton<IBrowserOpener, UnsupportedOsBrowserOpener>();
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