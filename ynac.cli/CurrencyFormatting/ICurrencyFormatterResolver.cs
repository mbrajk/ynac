namespace ynac.CurrencyFormatting;

internal interface ICurrencyFormatterResolver
{
    ICurrencyFormatter Resolve(bool isMasked);
}