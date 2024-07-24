namespace ynac.BudgetActions;

public class ExitBudgetAction : IBudgetAction
{
    public string DisplayName  => "Exit"; 
    public int Order  => 1; 

    public void Execute()
    {
        Environment.Exit(0);
    }
}