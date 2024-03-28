using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Spectre.Console;
using BudgetSync.YnabApi;
using BudgetSync.YnabApi.Budget;
using Polly;
using Polly.Extensions.Http;
using Spectre.Console.Cli;
using ynac;

var app = new CommandApp<BudgetCommand>();
await app.RunAsync(args);

public static class Setup
{
	public static ServiceProvider BuildServiceProvider(IConfigurationRoot configurationRoot)
	{
		var services = new ServiceCollection();

		// refit http client
		services
			.AddRefitClient<IYnabApi>()
			.ConfigureHttpClient(
				httpClient =>
				{
					var endpoint = configurationRoot["YnabApi:Endpoint"];
					var version = configurationRoot["YnabApi:Version"];
					var token = configurationRoot["YnabApi:Token"];

					httpClient.BaseAddress = new Uri($"{endpoint}/{version}");
					httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
				}
			)
			.AddPolicyHandler(
				HttpPolicyExtensions
					.HandleTransientHttpError()
					.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
					.WaitAndRetryAsync(4, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)))
			);

		// other required dependencies
		services.AddSingleton<IYnabConsole, Ynac>();
		services.AddSingleton<IBudgetQueryService, BudgetQueryService>();

		return services.BuildServiceProvider();
	}
}

public sealed class BudgetCommand : AsyncCommand<BudgetCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[Description("The budget name to filter to. Will find the first budget that contains this string")]
		[CommandArgument(0, "[budgetFilter]")]
		public string? BudgetFilter { get; init; }

		[Description("The category name to filter to. Will find the first category containing this string")]
		[CommandArgument(0, "[budgetFilter]")]
		public string? CategoryFilter { get; init; }
	}

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var config =  new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
		var services = Setup.BuildServiceProvider(config);
		var ynabConsole = services.GetRequiredService<IYnabConsole>();
		await ynabConsole.RunAsync(settings.BudgetFilter, settings.CategoryFilter);
		return 0;
	}
}

namespace ynac
{
	public interface IYnabConsole
	{
		public Task RunAsync(string? budgetFilter, string? categoryFilter);
	}

	class Ynac : IYnabConsole
	{
		private readonly IBudgetQueryService _budgetQueryService;

		public Ynac(IBudgetQueryService budgetQueryService)
		{
			_budgetQueryService = budgetQueryService;
		}
		public async Task RunAsync(string? budgetFilter, string? categoryFilter)
		{
			var budgets = await _budgetQueryService.GetBudgets();
			Budget selectedBudget = null;
			
			if (string.IsNullOrWhiteSpace(budgetFilter))
			{
				var index = 0;
				AnsiConsole.Markup("[bold aqua]            You Need A Console[/]\n");
				foreach (var budget in budgets)
				{
					AnsiConsole.Markup(
						$"[underline yellow]{index++}[/] [bold white]{budget.Name}[/] [italic grey]{budget.Id}[/]\n");
				}

				var selection = AnsiConsole.Ask<string>("[italic grey]   Select a budget:[/] ");
				int.TryParse(selection, out var selectionIndex);
				selectedBudget = budgets.ElementAt(selectionIndex);
			}
			else
			{
				selectedBudget = budgets.FirstOrDefault(b => b.Name.Contains(budgetFilter, StringComparison.OrdinalIgnoreCase));
			}

			if (selectedBudget == null)
			{
				AnsiConsole.Markup("[red]Budget(s) not found[/]");
				return;
			}
			
			var budgetCategoryGroups = await _budgetQueryService.GetBudgetCategories(selectedBudget);
			var table = new Table()
				.Caption("You Need A Table")
				.Border(TableBorder.Rounded)
				.BorderColor(Color.Yellow);	
			table.AddColumn($"[bold white][[[/] [yellow]{selectedBudget.Name}[/] [bold white]]][/]");

			var categoryGroups = budgetCategoryGroups.Where(c => !c.Hidden).Skip(1).SkipLast(1);
			if (!string.IsNullOrWhiteSpace(categoryFilter))
			{
				categoryGroups = categoryGroups.Where(c => c.Name.Contains(categoryFilter, StringComparison.OrdinalIgnoreCase));
			}
			foreach (var categoryGroup in categoryGroups)
			{
				var subTable = new Table()
					.Title(categoryGroup.Name, Style.Parse("italic blue"))
					.Expand();
				
				subTable.AddColumn(new TableColumn("Category").LeftAligned());
				subTable.AddColumn(new TableColumn("Budgeted").RightAligned());
				subTable.AddColumn(new TableColumn("Activity").RightAligned());
				subTable.AddColumn(new TableColumn("Available").RightAligned());	
				
				var totalBudgeted = 0m;
				var totalActivity = 0m;
				var totalAvailable = 0m;
				foreach (var category in categoryGroup.Categories.Where(c => !c.Hidden))
				{
					var activityDollars = category.Activity / 1000;
					var budgetedDollars = category.Budgeted / 1000;
					var availableDollars = category.Balance / 1000;
					
					totalBudgeted += budgetedDollars;
					totalActivity += activityDollars;
					totalAvailable += availableDollars;
					
					subTable.AddRow(
						$"[bold white]{category.Name}[/]", 
						$"[green]{budgetedDollars.ToString("C")}[/]", 
						$"[red]{activityDollars.ToString("C")}[/]", 
						$"[green]{availableDollars.ToString("C")}[/]"
					);
				}
				subTable.ShowFooters().AddRow(
					"", 
					$"[green]{totalBudgeted.ToString("C")}[/]", 
					$"[red]{totalActivity.ToString("C")}[/]", 
					$"[green]{totalAvailable.ToString("C")}[/]"
				).ShowRowSeparators().MinimalBorder();

				table.AddRow(subTable);
			}

			
			AnsiConsole.Write(table);
			
			return;
		}
	}
}