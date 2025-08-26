using System.Globalization;
using ynac.CurrencyFormatting;

namespace ynac;

internal class ValueFormatter : IValueFormatter
{
    private readonly ICurrencyFormatter _defaultCurrencyFormatter;
    private readonly ICurrencyFormatter _hiddenCurrencyFormatter;
    private bool _masked;

    public ValueFormatter(
        ICurrencyFormatter defaultCurrencyFormatter, 
        ICurrencyFormatter hiddenCurrencyFormatter,
        bool initiallyMasked = false)
    {
        _defaultCurrencyFormatter = defaultCurrencyFormatter;
        _hiddenCurrencyFormatter = hiddenCurrencyFormatter;
        _masked = initiallyMasked;
    }

    public void SetMasked(bool masked)
    {
        _masked = masked;
    }

    public string Format(decimal amount)
    {
        if (_masked)
        {
            return _hiddenCurrencyFormatter.Format(amount);
        }
		
        return _defaultCurrencyFormatter.Format(amount);
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