namespace ynac;

public static class Constants
{
    private const string YnabApiString = "YnabApi";
   
    internal const string TokenString = "Token";
    internal const string YnabApiSectionTokenKey = $"{YnabApiString}:{TokenString}";
    internal const string YnabSectionKey = $"[{YnabApiString}]";
    internal const string YnabRootUrl = "https://app.ynab.com/";
    internal const string BudgetRouteAffix = "/budget";
    internal const string ConfigFileLocation = "./config.ini";
}