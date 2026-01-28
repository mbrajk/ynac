using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using ynab;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.Commands;
using ynac.CurrencyFormatting;
using ynac.ErrorHandling;
using ynac.JsonOutput;
using ynac.OSFeatures;

namespace ynac;

public static class YnacConsoleProvider
{
    public static ServiceProvider BuildYnacServices(YnacConsoleSettings settings)
    {
        var services = new ServiceCollection();
        
        services.AddYnabApi(settings.Token);

        // Register currency visibility state initialized from settings
        services.AddSingleton<ICurrencyVisibilityState>(new CurrencyVisibilityState { Hidden = settings.HideAmounts });

        // Register error writer for consistent error output
        services.AddSingleton(settings.ErrorWriter);
        
        services.AddSingleton<ICurrencyFormatterResolver, CurrencyFormatterResolver>();
        services.AddSingleton<MaskedCurrencyFormatter>();
        services.AddSingleton<DefaultCurrencyFormatter>();
        services.AddSingleton<IValueFormatter, ValueFormatter>();

        // other required dependencies
        services.AddSingleton<IJsonOutputWriter, JsonOutputWriter>();
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
        services.AddSingleton<IAnsiConsoleService, AnsiConsoleService>();
        services.AddSingleton<IBudgetSelector, BudgetSelector>();
        
        services.AddSingleton<IBudgetAction, ToggleHideAmountsBudgetAction>();
        services.AddSingleton<IBudgetAction, ExitBudgetAction>();
        //services.AddSingleton<IBudgetAction, MaskValuesAction>();
        // add back when implemented
        //services.AddSingleton<IBudgetAction, ListTransactionsBudgetAction>(); 
        
        return services.BuildServiceProvider();
    }
}