namespace ynac.BudgetActions;

public class ListTransactionsBudgetAction : IBudgetAction
{
    public string DisplayName => "List Transactions";
    public int Order => 0;

    public void Execute()
    {
        Console.WriteLine("Listing transactions...");
    }
}