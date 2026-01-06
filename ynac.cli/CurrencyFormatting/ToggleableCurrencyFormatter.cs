namespace ynac.CurrencyFormatting;

/// <summary>
/// A currency formatter that dynamically switches between a default formatter and a masked formatter
/// based on the current visibility state.
/// </summary>
public class ToggleableCurrencyFormatter : ICurrencyFormatter
{
    private readonly ICurrencyVisibilityState _state;
    private readonly DefaultCurrencyFormatter _defaultFormatter;
    private readonly MaskedCurrencyFormatter _maskedFormatter;

    public ToggleableCurrencyFormatter(
        ICurrencyVisibilityState state,
        DefaultCurrencyFormatter defaultFormatter,
        MaskedCurrencyFormatter maskedFormatter)
    {
        _state = state;
        _defaultFormatter = defaultFormatter;
        _maskedFormatter = maskedFormatter;
    }

    public string Format(decimal amount)
    {
        return _state.Hidden
            ? _maskedFormatter.Format(amount)
            : _defaultFormatter.Format(amount);
    }
}
