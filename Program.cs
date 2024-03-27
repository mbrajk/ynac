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
		public Task RunAsync()
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
			
			var categoryGroups = _budgetQueryService.GetBudgetCategories(selectedBudget);
			index = 0;
			foreach (var categoryGroup in categoryGroups.Result)
			{
				AnsiConsole.Markup($"[underline yellow]{index++}[/] [bold white]{categoryGroup.Name}[/]\n");
			}
			
			var selectedCategories = _budgetQueryService.GetBudgetCategories(selectedBudget);
			index = 0;
			foreach (var categoryGroup in selectedCategories.Result)
			{
				//AnsiConsole.Markup($"[underline yellow]{index++}[/] [bold white]{categoryGroup.Name}[/]\n");
			}
			//AnsiConsole.Markup("[italic grey]Select a budget:[/] ");
			return Task.CompletedTask;
		}
	}
}