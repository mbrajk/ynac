using Spectre.Console;
using Spectre.Console.Rendering;
using ynab.Budget;
using ynab.Category;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.Commands;
using ynac.OSFeatures;

namespace ynac;

internal class YnacConsole(
    IBudgetQueryService budgetQueryService,
    IBudgetBrowserOpener budgetBrowserOpener,
    IBudgetSelector budgetSelector,
    IEnumerable<IBudgetAction> budgetActions,
    IValueFormatter valueFormatter,
	IAnsiConsoleService ansiConsoleService
) : IYnacConsole
{
    public async Task RunAsync(BudgetCommandSettings settings)
    {
        ansiConsoleService.WriteHeaderRule("[bold]You Need A Console[/]");

		var pullLastUsedBudget = settings.PullLastUsed;
		var filter = settings.BudgetFilter ?? "";
	
		var selectedBudget = await budgetSelector.SelectBudget(filter, pullLastUsedBudget);
		
		if (selectedBudget.Type == BudgetType.NotFound)
		{
			ansiConsoleService.Markup("[red]Budget(s) not found[/]");
			return;
		}

		if (settings.Open)
		{
			budgetBrowserOpener.OpenBudget(selectedBudget);
			return;
		}
			
		var categoryFilter = settings.CategoryFilter;
		var selectedBudgetFull = await budgetQueryService.GetBudgetMonth(selectedBudget);
		var categoryGroups = await budgetQueryService.GetBudgetCategories(options =>
		{
			options.SelectedBudget = selectedBudget;
			options.CategoryFilter = categoryFilter;
			options.ShowHiddenCategories = settings.ShowHiddenCategories;
		});
		
		void RenderBudget()
		{
			var table = CreateTable(selectedBudget.Name, selectedBudgetFull);
			GenerateCategoryTable(categoryGroups, settings, table);
			ansiConsoleService.Write(table);
		}
		
		RenderBudget();

		while (true)
		{
			var action = ansiConsoleService.Prompt(
				new SelectionPrompt<IBudgetAction>()
					.PageSize(10)
					.MoreChoicesText("more..")
					.AddChoices(budgetActions.OrderBy(b => b.Order))
					.UseConverter(b => b.DisplayName)
					.Title("Select an action:"));
			
			action.Execute();
			RenderBudget();
		}
	}

	internal void GenerateCategoryTable(IReadOnlyCollection<CategoryGroup> categoryGroups, BudgetCommandSettings settings, Table table)
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
			foreach (var category in categoryGroup.Categories.Where(c => settings.ShowHiddenCategories || !c.Hidden).Where(c => !c.Deleted))
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
					new Markup($"[green]{valueFormatter.Format(budgetedDollars)}[/]"),
					new Markup($"[red]{valueFormatter.Format(activityDollars)}[/]"),
					new Markup($"[green]{valueFormatter.Format(availableDollars)}[/]")
				);
			}
			subTable.ShowFooters().AddRow(
				"", 
				$"[green]{valueFormatter.Format(totalBudgeted)}[/]",
				$"[red]{valueFormatter.Format(totalActivity)}[/]",
				$"[green]{valueFormatter.Format(totalAvailable)}[/]"
			).ShowRowSeparators().MinimalBorder();

			table.AddRow(subTable);
		}
	}

	internal Table CreateTable(string budgetName, BudgetMonth selectedBudget)
	{
		var table = new Table()
			.Caption(budgetName)
			.Border(TableBorder.Rounded)
			.BorderColor(Color.Yellow);
		
		// Create header components
		var budgetNameMarkup = new Markup($"[bold white][[[/] [yellow]{budgetName}[/] [bold white]]][/]");
		var ageOfMoneyMarkup = new Markup($"[white]Age of money:[/] [aqua]{valueFormatter.Format(selectedBudget.AgeOfMoney ?? 0)}[/]");
		var toBeBudgetedMarkup = new Markup($"To Be Budgeted: [green]{valueFormatter.Format(selectedBudget.ToBeBudgeted/1000)}[/]");
		
		// Use Grid to layout the header components in a single column with vertical stacking
		var headerGrid = new Grid();
		headerGrid.AddColumn();
		
		// First row: budget name and age of money side-by-side using Columns layout
		var firstRowLayout = new Columns(
			budgetNameMarkup,
			new Padder(ageOfMoneyMarkup, new Padding(2, 0, 0, 0))
		);
		
		headerGrid.AddRow(firstRowLayout);
		headerGrid.AddRow(new Padder(toBeBudgetedMarkup, new Padding(2, 0, 0, 0)));
		
		table.AddColumn(new TableColumn(headerGrid));

		return table;
	}
}