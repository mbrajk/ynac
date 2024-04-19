using Spectre.Console.Cli;
using Ynac;

var app = new CommandApp<BudgetCommand>();
await app.RunAsync(args);