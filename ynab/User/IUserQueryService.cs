namespace ynab.User;

/// <summary>
/// Provides query operations for the authenticated YNAB user.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    /// Retrieves the id of the user the current API token belongs to.
    /// </summary>
    /// <returns>The authenticated user's id, or <see cref="Guid.Empty"/> if unavailable.</returns>
    Task<Guid> GetAuthenticatedUserId();
}
