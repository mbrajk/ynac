namespace ynac.CurrencyFormatting;

internal class CurrencyFormatterResolver : ICurrencyFormatterResolver
{
    private readonly DefaultCurrencyFormatter _defaultFormatter;
    private readonly MaskedCurrencyFormatter _maskedFormatter;

    public CurrencyFormatterResolver(
        DefaultCurrencyFormatter defaultFormatter, 
        MaskedCurrencyFormatter maskedFormatter)
    {
        _defaultFormatter = defaultFormatter;
        _maskedFormatter = maskedFormatter;
    }
    
    public ICurrencyFormatter Resolve(bool isMasked) => isMasked ? _maskedFormatter : _defaultFormatter;
}