using Spectre.Console.Cli;
using ynac;

var app = new CommandApp<BudgetCommand>();
await app.RunAsync(args);