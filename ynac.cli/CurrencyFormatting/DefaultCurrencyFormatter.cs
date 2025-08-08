using System.Globalization;

namespace ynac.CurrencyFormatting;

public class DefaultCurrencyFormatter : ICurrencyFormatter
{
    public string Format(decimal amount, CultureInfo? culture)
    {
        culture ??= CultureInfo.CurrentCulture;
        return amount.ToString("C", culture);
    }

    public string Format(decimal amount)
    {
        return Format(amount, CultureInfo.CurrentCulture);
    }
}
