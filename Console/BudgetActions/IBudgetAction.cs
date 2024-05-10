namespace ynac.BudgetActions;

public interface IBudgetAction
{
   public string DisplayName { get; }
   public int Order { get; }
   public void Execute(); 
}

public class ListTransactionsBudgetAction : IBudgetAction
{
   public string DisplayName => "List Transactions";
   public int Order => 0;

   public void Execute()
   {
      Console.WriteLine("Listing transactions...");
   }
}

public class ExitBudgetAction : IBudgetAction
{
   public string DisplayName  => "Exit"; 
   public int Order  => 1; 

   public void Execute()
   {
      Environment.Exit(0);
   }
}