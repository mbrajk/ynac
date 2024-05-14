using Spectre.Console;
using Spectre.Console.Rendering;
using YnabApi.Budget;
using YnabApi.Category;
using ynac.BudgetActions;
using ynac.OSFeatures;

namespace ynac;

public interface IYnabConsole
{
    public Task RunAsync(BudgetCommand.Settings settings);
}

class YnabConsole(
	IBudgetQueryService budgetQueryService, 
	IBudgetOpener budgetOpener,
	IBudgetSelector budgetSelector,
	IEnumerable<IBudgetAction> budgetActions
) : IYnabConsole
{
	public async Task RunAsync(BudgetCommand.Settings settings) 
	{
		WriteHeaderRule("[bold]You Need A Console[/]");

		var pullLastUsedBudget = settings.PullLastUsed;
		var filter = settings.BudgetFilter ?? "";
	
		var selectedBudget = await budgetSelector.SelectBudget(filter, pullLastUsedBudget);
		
		if (selectedBudget.Type == BudgetType.NotFound)
		{
			AnsiConsole.Markup("[red]Budget(s) not found[/]");
			return;
		}

		if (settings.Open)
		{
			budgetOpener.OpenBudget(selectedBudget);
			return;
		}
			
		var categoryFilter = settings.CategoryFilter;
		var selectedBudgetFull = await budgetQueryService.GetCurrentMonthBudget(selectedBudget);
		var categoryGroups = await budgetQueryService.GetBudgetCategories(options =>
		{
			options.SelectedBudget = selectedBudget;
			options.CategoryFilter = categoryFilter;
		});
		
		var table = CreateTable(selectedBudget.Name, selectedBudgetFull);
		
		GenerateCategoryTable(categoryGroups, settings, table);

		AnsiConsole.Write(table);

		while (true)
		{
			var action = AnsiConsole.Prompt(
				new SelectionPrompt<IBudgetAction>()
					.PageSize(10)
					.MoreChoicesText("more..")
					.AddChoices(budgetActions.OrderBy(b => b.Order))
					.UseConverter(b => b.DisplayName)
					.Title("Select an action:"));
			
			action.Execute();
		}
	}

	private static void GenerateCategoryTable(IReadOnlyCollection<CategoryGroup> categoryGroups, BudgetCommand.Settings settings, Table table)
	{
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

	

	private static void WriteHeaderRule(string title)
	{
		var rule = new Rule(title);
		rule.Style = Style.Parse("aqua");
		rule.LeftJustified();
		AnsiConsole.Write(rule);
	}
}