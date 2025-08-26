namespace ynac.CurrencyFormatting;

public interface ICurrencyFormatter
{
    /// <summary>
    /// Formats a given currency value based on the concrete implementation rules
    /// </summary>
    /// <param name="amount">a decimal representing the value to be formatted</param>
    string Format(decimal amount);
}
