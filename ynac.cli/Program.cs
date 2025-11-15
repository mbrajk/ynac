using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        var skipConfigCreation = args.Any(arg => arg.Equals("--debug-skip-config", StringComparison.OrdinalIgnoreCase));
        
        if (!skipConfigCreation)
        {
            await InitConfigFile();
        }

        var app = new CommandApp<BudgetCommand>();
        await app.RunAsync(args);
    }


    private static async Task InitConfigFile()
    {
        if (!File.Exists(Constants.ConfigFilePath))
        {
            await using var configFileStream = File.Create(Constants.ConfigFilePath);
            await using var streamWriter = new StreamWriter(configFileStream);

            var assembly = Assembly.GetExecutingAssembly();
            await using var configTemplateStream = assembly.GetManifestResourceStream(Constants.ConfigFileTemplate);
            if (configTemplateStream == null)
            {
                throw new FileNotFoundException("Embedded resource not found", Constants.ConfigFileTemplate);
            }
            await configTemplateStream.CopyToAsync(configFileStream);
            Console.WriteLine($"[YNAC] created {Constants.ConfigFilePath}");
        }
    }
}
