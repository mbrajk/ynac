// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace ynac;

public static class Constants
{
    private const string YnabApiString = "YnabApi";
    private const string YnacString = "Ynac";

    internal static readonly string YnabApiSectionHeader = $"[{YnabApiString}]";
    internal static readonly string TokenString = "Token";
    internal static readonly string YnabApiTokenConfigPath = $"{YnabApiString}:{TokenString}";

    internal static readonly string YnacSectionHeader = $"[{YnacString}]";
    internal static readonly string HideAmountsString = "HideAmounts";
    internal static readonly string YnacHideAmountsConfigPath = $"{YnacString}:{HideAmountsString}";

    internal static readonly string YnabRootUrl = "https://app.ynab.com/";
    internal static readonly string BudgetRouteAffix = "/budget";

    // config.ini constants
    internal static readonly string ConfigFileName = "config.ini";
    internal static readonly string ConfigFileTemplate = "ynac._res.config.template.ini";
    internal static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);

    // The default string to check for in config.ini, do not edit here 
    internal static readonly string DefaultTokenString = "put-your-token-here";
}
