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
			selectedBudget = PromptBudgetSelection(budgets, settings);
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

		// if 'last-used' budget is chosen, we are not able to open it here 
		// as the id is unknown without calling the entire budget export endpoint, which takes a very long time
		if (settings.Open)
		{
			// TODO: implement for other OSes
			OSFeatures.OpenBudgetWindows(selectedBudget);
			return;
		}
			
		var selectedBudgetFull = await budgetQueryService.GetCurrentMonthBudget(selectedBudget);
		var budgetCategoryGroups = await budgetQueryService.GetBudgetCategories(selectedBudget, categoryFilter);
		
		var table = CreateTable(selectedBudget.Name, selectedBudgetFull);

		// skip last category group, this is the "hidden" group that includes categories hidden by the user
		var categoryGroups = budgetCategoryGroups
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

	//TODO: format column text using Spectre tools
	private static Table CreateTable(string budgetName, BudgetMonth selectedBudget)
	{
		var table = new Table()
			.Caption(budgetName)
			.Border(TableBorder.Rounded)
			.BorderColor(Color.Yellow);
		
		var columnText = $"[bold white][[[/] [yellow]{budgetName}[/] [bold white]]][/]" +
		                 $"                 " +
		                 $"[white]Age of money:[/] [aqua]{selectedBudget.AgeOfMoney ?? 0}[/]\n" +
		                 $"                            " +
		                 $"To Be Budgeted: [green]{(selectedBudget.ToBeBudgeted/1000).ToString("C")}[/]";
		
		table.AddColumn(columnText);

		return table;
	}

	private Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets, BudgetCommand.Settings settings)
	{
		var lastUsedBudget = new Budget
		{
			Name = "last-used",
			Id = Guid.Empty
		};
		
		var budgetSelection = settings.Open ? budgets : [lastUsedBudget, ..budgets];
		
		var budget = AnsiConsole.Prompt(
			new SelectionPrompt<Budget>()
				.Title("[italic grey]Select a[/] [underline italic aqua]budget:[/]")
				.PageSize(10)
				.MoreChoicesText("[grey](Move up and down to reveal more budgets)[/]")
				.AddChoices(budgetSelection)
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