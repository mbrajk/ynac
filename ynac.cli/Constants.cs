// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace ynac;

public static class Constants
{
    private const string YnabApiString = "YnabApi";
   
    internal static string TokenString => "Token";
    internal static string YnabApiSectionTokenKey => $"{YnabApiString}:{TokenString}";
    internal static string YnabSectionKey => $"[{YnabApiString}]";
    internal static string YnabRootUrl => "https://app.ynab.com/";
    internal static string BudgetRouteAffix => "/budget";
    internal static string ConfigFileLocation => "./config.ini";
    
    // The default string to check for in config.ini, do not edit here 
    internal static string DefaultTokenString => "put-your-token-here";
}