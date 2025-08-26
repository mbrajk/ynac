namespace ynac.BudgetActions;

internal class MaskValuesAction //: IBudgetAction
{
    IValueFormatter _formatter;
    
    public MaskValuesAction(IValueFormatter formatter)
    {
        _formatter = formatter;
    }
    public string DisplayName => _formatter.GetMasked() ? "Show Values" : "Mask Values";
    public int Order => 1;
    public void Execute()
    {
        _formatter.SetMasked(!_formatter.GetMasked()); 
    }
}