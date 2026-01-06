namespace ynac.CurrencyFormatting;

/// <summary>
/// Represents the runtime state for currency visibility (hidden/shown).
/// </summary>
public interface ICurrencyVisibilityState
{
    /// <summary>
    /// Gets or sets whether currency amounts should be hidden.
    /// </summary>
    bool Hidden { get; set; }
}
