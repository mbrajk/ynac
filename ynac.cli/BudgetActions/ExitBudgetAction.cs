namespace ynac.BudgetActions;

public class ExitBudgetAction : IBudgetAction
{
    public string DisplayName  => "Exit"; 
    public int Order  => int.MaxValue; 

    public void Execute()
    {
        Environment.Exit(0);
    }
}