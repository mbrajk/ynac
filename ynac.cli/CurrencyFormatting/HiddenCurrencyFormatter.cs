namespace ynac.CurrencyFormatting;

public class HiddenCurrencyFormatter : ICurrencyFormatter
{
    public string Format(decimal amount)
    {
        return "***.**";
    }
}
