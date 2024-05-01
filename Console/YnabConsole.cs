using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using YnabApi.Budget;

namespace ynac;

public interface IYnabConsole
{
    public Task RunAsync(BudgetCommand.Settings settings);
}

class YnabConsole(IBudgetQueryService budgetQueryService) : IYnabConsole
{
	public async Task RunAsync(BudgetCommand.Settings settings) 
	{
		var budgets = await budgetQueryService.GetBudgets();
		Budget? selectedBudget = null;
		var budgetFilter = settings.BudgetFilter;
		var categoryFilter = settings.CategoryFilter;

		WriteHeaderRule("[bold]You Need A Console[/]");

		if (string.IsNullOrWhiteSpace(budgetFilter))
		{
			selectedBudget = PromptBudgetSelection(budgets);
		}
		else
		{
			// todo: print out the budget that was found, or prompt for selection if multiple results
			// todo: allow guid to be entered directly on command line
			selectedBudget = budgets.FirstOrDefault(b => b.Name.Contains(budgetFilter, StringComparison.OrdinalIgnoreCase));
		}

		if (selectedBudget == null)
		{
			AnsiConsole.Markup("[red]Budget(s) not found[/]");
			return;
		}

		// if 'last-used' budget is chosen, we wouldn't be able to open it here without another round trip to ynab to get the id
		if (settings.Open)
		{
			//only works on windows but is possible on Linux and Mac
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = $"{Constants.YnabRootUrl}{selectedBudget.Id}{Constants.BudgetRouteAffix}",
				UseShellExecute = true
			};
			Process.Start (psi);
			return;
		}
			
		var selectedBudgetFull = await budgetQueryService.GetBudgetMonth(selectedBudget.BudgetId, new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1));
		var budgetCategoryGroups = await budgetQueryService.GetBudgetCategories(selectedBudget);
		var table = new Table()
			.Caption("You Need A Table")
			.Border(TableBorder.Rounded)
			.BorderColor(Color.Yellow);

		var columnText = $"[bold white][[[/] [yellow]{selectedBudget.Name}[/] [bold white]]][/]" +
		                 $"                 " +
		                 $"[white]Age of money:[/] [aqua]{selectedBudgetFull.AgeOfMoney ?? 0}[/]\n" +
		                 $"                            " +
		                 $"To Be Budgeted: [green]{(selectedBudgetFull.ToBeBudgeted/1000).ToString("C")}[/]";
		table.AddColumn(columnText);

		var categoryGroups = budgetCategoryGroups
			.Where(c => !c.Hidden)
			.Where(c => !c.Deleted)
			.Skip(1)
			.SkipLast(1);
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
			foreach (var category in categoryGroup.Categories.Where(c => !c.Hidden).Where(c => !c.Deleted))
			{
				var activityDollars = category.Activity / 1000;
				var budgetedDollars = category.Budgeted / 1000;
				var availableDollars = category.Balance / 1000;
					
				totalBudgeted += budgetedDollars;
				totalActivity += activityDollars;
				totalAvailable += availableDollars;


				IRenderable categoryCell;
				if (settings.ShowGoals)
				{
					var breakdownChart = new BreakdownChart();
					breakdownChart.Width(20);
					breakdownChart.HideTags();
					breakdownChart.HideTagValues();
					if (category.GoalPercentageComplete != null)
					{
						breakdownChart.AddItem("Complete", category.GoalPercentageComplete.Value, Color.Green);
						breakdownChart.AddItem("Incomplete", 100 - category.GoalPercentageComplete.Value, Color.Red);
					}
                    					
					var panel = new Panel(breakdownChart);
					panel.Border = BoxBorder.None;
					panel.Header = new PanelHeader(category.Name);
					categoryCell = panel;
				}
				else
				{
					categoryCell = new Markup($"[bold white]{category.Name}[/]");
				}
			
				subTable.AddRow(
					categoryCell,
					new Markup($"[green]{budgetedDollars.ToString("C")}[/]"), 
					new Markup($"[red]{activityDollars.ToString("C")}[/]"), 
					new Markup($"[green]{availableDollars.ToString("C")}[/]")
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

	private Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets)
	{
		// todo: finish implementing last-used as a budget selection
		var lastUsedBudget = new Budget
		{
			Name = "last-used",
			Id = Guid.Empty
		};
		var budget = AnsiConsole.Prompt(
			new SelectionPrompt<Budget>()
				.Title("[italic grey]Select a[/] [underline italic aqua]budget:[/]")
				.PageSize(10)
				.MoreChoicesText("[grey](Move up and down to reveal more budgets)[/]")
				.AddChoices(budgets)
				.UseConverter(budget => budget.ToString() + $" [grey]{budget.BudgetId}[/]")
			);
		
		return budget;
	}

	private static void WriteHeaderRule(string title)
	{
		var rule = new Rule(title);
		rule.Style = Style.Parse("aqua");
		rule.LeftJustified();
		AnsiConsole.Write(rule);
	}
}