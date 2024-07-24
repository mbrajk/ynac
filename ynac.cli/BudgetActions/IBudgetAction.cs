namespace ynac.BudgetActions;

public interface IBudgetAction
{
   public string DisplayName { get; }
   public int Order { get; }
   public void Execute(); 
}