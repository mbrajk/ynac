using Spectre.Console.Cli;

var app = new CommandApp<BudgetCommand>();
await app.RunAsync(args);