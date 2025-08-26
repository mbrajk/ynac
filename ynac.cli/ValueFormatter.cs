using System.Globalization;
using ynac.CurrencyFormatting;

namespace ynac;

internal class ValueFormatter : IValueFormatter
{
    private readonly ICurrencyFormatterResolver _resolver;
    private bool _masked;

    public ValueFormatter(
        ICurrencyFormatterResolver currencyFormatterResolver,
        bool initiallyMasked = false)
    {
        _resolver = currencyFormatterResolver;
        _masked = initiallyMasked;
    }

    public void SetMasked(bool masked)
    {
        _masked = masked;
    }

    public bool GetMasked()
    {
        return _masked;
    }

    public string Format(decimal amount)
    {
        return _resolver.Resolve(_masked).Format(amount);
    }

    public string Format(int value)
    {
        if (_masked)
        {
            return "***";
        }

        return value.ToString("N0", CultureInfo.CurrentCulture);
    }
}