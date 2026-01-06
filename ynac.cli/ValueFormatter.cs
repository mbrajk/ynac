using System.Globalization;
using ynac.CurrencyFormatting;

namespace ynac;

internal class ValueFormatter : IValueFormatter
{
    private readonly ICurrencyFormatterResolver _resolver;
    private readonly ICurrencyVisibilityState _visibilityState;

    public ValueFormatter(
        ICurrencyFormatterResolver currencyFormatterResolver,
        ICurrencyVisibilityState visibilityState)
    {
        _resolver = currencyFormatterResolver;
        _visibilityState = visibilityState;
    }

    public void SetMasked(bool masked)
    {
        _visibilityState.Hidden = masked;
    }

    public bool GetMasked()
    {
        return _visibilityState.Hidden;
    }

    public string Format(decimal amount)
    {
        return _resolver.Resolve(_visibilityState.Hidden).Format(amount);
    }

    public string Format(int value)
    {
        if (_visibilityState.Hidden)
        {
            return "***";
        }

        return value.ToString("N0", CultureInfo.CurrentCulture);
    }
}
