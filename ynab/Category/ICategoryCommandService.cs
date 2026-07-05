namespace ynab.Category;

/// <summary>
/// Provides write operations for categories in YNAB.
/// </summary>
public interface ICategoryCommandService
{
    /// <summary>
    /// Sets the budgeted amount for a category in a specific month.
    /// </summary>
    /// <param name="budget">The budget the category belongs to.</param>
    /// <param name="month">The month to update in ISO format (yyyy-MM-01), or "current" for the current month.</param>
    /// <param name="categoryId">The id of the category to update.</param>
    /// <param name="budgetedMilliunits">The budgeted amount in milliunits.</param>
    /// <returns>The updated category, or null if the update failed.</returns>
    Task<Category?> SetMonthCategoryBudgeted(Budget.Budget budget, string month, Guid categoryId, decimal budgetedMilliunits);

    /// <summary>
    /// Updates a category's name and/or note. Null values are left unchanged.
    /// </summary>
    /// <param name="budget">The budget the category belongs to.</param>
    /// <param name="categoryId">The id of the category to update.</param>
    /// <param name="name">The new category name, or null to leave unchanged.</param>
    /// <param name="note">The new category note, or null to leave unchanged.</param>
    /// <returns>The updated category, or null if the update failed.</returns>
    Task<Category?> UpdateCategory(Budget.Budget budget, Guid categoryId, string? name = null, string? note = null);
}
