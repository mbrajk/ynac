using Spectre.Console.Cli;
using ynac;

#if ! DEBUG
if(!File.Exists(Constants.ConfigFileLocation))
{
    await using var configFileStream = File.Create(Constants.ConfigFileLocation);
    await using var streamWriter = new StreamWriter(configFileStream);
    
    await streamWriter.WriteLineAsync(Constants.YnabSectionKey);
    await streamWriter.WriteLineAsync($"{Constants.TokenString}=\"{Constants.DefaultTokenString}\"");
}
#endif

var app = new CommandApp<BudgetCommand>();
await app.RunAsync(args);