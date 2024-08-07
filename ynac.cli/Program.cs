using System.Diagnostics.CodeAnalysis;
using Spectre.Console.Cli;
using ynac.Commands;


namespace ynac;

internal class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BudgetCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BudgetCommandSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    public static async Task Main(string[] args)
    {
        await InitConfigFile(); 
       
        var app = new CommandApp<BudgetCommand>();
        await app.RunAsync(args);
    }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private static async Task InitConfigFile()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        // create config file if it doesn't exist, but only in release mode. This was done for testing purposes.
        // can likely do this with a flag in the future
#if ! DEBUG
if(!File.Exists(Constants.ConfigFileLocation))
{
    await using var configFileStream = File.Create(Constants.ConfigFileLocation);
    await using var streamWriter = new StreamWriter(configFileStream);
    
    await streamWriter.WriteLineAsync(Constants.YnabSectionKey);
    await streamWriter.WriteLineAsync($"{Constants.TokenString}=\"{Constants.DefaultTokenString}\"");
}
#endif
    }
}