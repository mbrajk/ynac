namespace ynab.Payee;

/// <summary>
/// Provides write operations for payees in YNAB.
/// </summary>
public interface IPayeeCommandService
{
    /// <summary>
    /// Renames a payee.
    /// </summary>
    /// <param name="budget">The budget the payee belongs to.</param>
    /// <param name="payeeId">The id of the payee to rename.</param>
    /// <param name="name">The new payee name.</param>
    /// <returns>The updated payee, or null if the update failed.</returns>
    Task<Payee?> RenamePayee(Budget.Budget budget, Guid payeeId, string name);
}
