namespace ynac.CurrencyFormatting;

public class MaskedCurrencyFormatter : ICurrencyFormatter
{
    public string Format(decimal amount)
    {
        return "***.**";
    }
}
