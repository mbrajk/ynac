using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Spectre.Console;
using BudgetSync.YnabApi;
using BudgetSync.YnabApi.Budget;
using Polly;
using Polly.Extensions.Http;
using ynac;

var config =  new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var services = BuildServiceProvider(config);

ServiceProvider BuildServiceProvider(IConfigurationRoot configurationRoot)
{
	var services = new ServiceCollection();

	// refit http client
	services
		.AddRefitClient<IYnabApi>()
		.ConfigureHttpClient(
			httpClient =>
			{
				var endpoint = config["YnabApi:Endpoint"];
				var version = config["YnabApi:Version"];
				var token = config["YnabApi:Token"];

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

var budgetApplication = services.GetRequiredService<IYnabConsole>();

await budgetApplication.RunAsync();

namespace ynac
{
	public interface IYnabConsole
	{
		public Task RunAsync();
	}

	class Ynac : IYnabConsole
	{
		private readonly IBudgetQueryService _budgetQueryService;

		public Ynac(IBudgetQueryService budgetQueryService)
		{
			_budgetQueryService = budgetQueryService;
		}
		public async Task RunAsync()
		{
			var budgets = _budgetQueryService.GetBudgets();
			var index = 0;
			AnsiConsole.Markup("[bold aqua]            You Need A Console[/]\n");
			foreach (var budget in budgets.Result)	
			{
				AnsiConsole.Markup($"[underline yellow]{index++}[/] [bold white]{budget.Name}[/] [italic grey]{budget.Id}[/]\n");
			}
			var selection = AnsiConsole.Ask<string>("[italic grey]   Select a budget:[/] ");
			int.TryParse(selection, out var selectionIndex);
			var selectedBudget = budgets.Result.ElementAt(selectionIndex);
			
			var budgetCategoryGroups = await _budgetQueryService.GetBudgetCategories(selectedBudget);
			var table = new Table()
				.Caption("You Need A Table")
				.Border(TableBorder.Rounded)
				.BorderColor(Color.Yellow);	
			table.AddColumn($"[bold white][[[/] [yellow]{selectedBudget.Name}[/] [bold white]]][/]");
			
			foreach (var categoryGroup in budgetCategoryGroups.Where(c => !c.Hidden).Skip(1).SkipLast(1))
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
					var budgetedDollars = category.Balance / 1000;
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
					
				subTable.AddRow(
					"", 
					$"[green]{totalBudgeted.ToString("C")}[/]", 
					$"[red]{totalActivity.ToString("C")}[/]", 
					$"[green]{totalAvailable.ToString("C")}[/]"
				);

				table.AddRow(subTable);
			}

			
			AnsiConsole.Write(table);
			
			return;
		}
	}
}