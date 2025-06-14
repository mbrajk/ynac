namespace ynac.CurrencyFormatting;

public class DefaultCurrencyFormatter : ICurrencyFormatter
{
    public string Format(decimal amount)
    {
        return amount.ToString("C");
    }
}
