namespace ynac;

internal interface IValueFormatter
{
    public void SetMasked(bool masked);	
    public string Format(decimal amount);
    public string Format(int value);
}