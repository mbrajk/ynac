namespace ynac.BudgetActions;

public class ExitBudgetAction : IBudgetAction
{
    public string DisplayName  => "Exit"; 
    public int Order  => int.MaxValue; 

    public Task ExecuteAsync()
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}