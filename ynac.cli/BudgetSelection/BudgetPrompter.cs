using YnabApi.Budget;

namespace ynac.BudgetSelection;

public class BudgetPrompter : PrompterBase, IBudgetPrompter
{
    public BudgetPrompter(IConsolePrompt console) 
        : base(console)
    { }
        
    public Budget PromptBudgetSelection(IReadOnlyCollection<Budget> budgets)
    {
        if (budgets.Count == 0)
        {
            return Budget.NoBudget;
        }
        
        // if there is only one budget, select it automatically
        if (budgets.Count == 1)
        {
            return budgets.First();
        }
            		
        return PromptSelection(budgets, "[italic grey]Select a[/] [italic aqua]budget:[/]", budget => $"{budget} [grey]{budget.BudgetId}[/]");
    }
}