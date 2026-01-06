namespace ynac.CurrencyFormatting;

/// <summary>
/// Default implementation of ICurrencyVisibilityState that holds the runtime state
/// for whether currency amounts should be hidden or shown.
/// </summary>
public class CurrencyVisibilityState : ICurrencyVisibilityState
{
    /// <summary>
    /// Gets or sets whether currency amounts should be hidden.
    /// </summary>
    public bool Hidden { get; set; }
}
