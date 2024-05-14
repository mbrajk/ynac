using Spectre.Console;
using YnabApi.Budget;

namespace ynac;

public interface IBudgetSelector
{
    public Task<Budget> SelectBudget(string budgetFilter, bool selectLastBudget = true);
}

public class BudgetSelector : IBudgetSelector
{
	private	readonly IBudgetQueryService _budgetQueryService;
    private readonly IBudgetPrompter _budgetPrompter;
    
	public BudgetSelector(
		IBudgetQueryService budgetQueryService,
		IBudgetPrompter budgetPrompter
	)
	{
		_budgetQueryService = budgetQueryService;
		_budgetPrompter = budgetPrompter;
	}
	
    private Budget? _selectedBudget;
    private IReadOnlyCollection<Budget> _budgets;

    public async Task<Budget> SelectBudget(string budgetFilter = "", bool selectLastBudget = true)
    {
	    if (selectLastBudget)
	    {
		    _selectedBudget = Budget.LastUsedBudget;
		    return _selectedBudget;
	    }
	    
	    // this line prevents ever re-calling the API to get budgets again, should eventually
	    // allow refreshing via option or a time based cache etc. Furthermore, caching could 
	    // move into the query service itself so that this consumer doesn't have to know about it
	    _budgets = _budgets.Any() ? _budgets : await _budgetQueryService.GetBudgets();
	    IReadOnlyCollection<Budget> filteredBudgets = [ Budget.LastUsedBudget, .._budgets];

	    if (!string.IsNullOrWhiteSpace(budgetFilter))
	    {
		   filteredBudgets = filteredBudgets
			   .Where(budget => budget.Name.Contains(budgetFilter, StringComparison.InvariantCultureIgnoreCase))
               .Where(budget => budget.Type != BudgetType.LastUsed)
               .ToList();
	    }
	    
	    var selectedBudget = _budgetPrompter.PromptBudgetSelection(filteredBudgets);
        
	    return selectedBudget;
    }
}