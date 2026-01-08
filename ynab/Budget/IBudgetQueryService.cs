using ynab.Category;

namespace ynab.Budget
{
    /// <summary>
    /// Provides query operations for retrieving budget and category information from YNAB.
    /// </summary>
    public interface IBudgetQueryService
    {
        /// <summary>
        /// Retrieves all budgets available for the authenticated user.
        /// </summary>
        /// <returns>A read-only collection of budgets.</returns>
        Task<IReadOnlyCollection<Budget>> GetBudgets();
        
        /// <summary>
        /// Retrieves budget data for a specific month.
        /// </summary>
        /// <param name="budget">The budget to retrieve month data for.</param>
        /// <param name="date">The date within the month to retrieve. If null, retrieves the current month.</param>
        /// <returns>Budget month data including categories and amounts.</returns>
        Task<BudgetMonth> GetBudgetMonth(Budget budget, DateOnly? date = null);
        
        /// <summary>
        /// Retrieves budget categories with search options configured via a callback.
        /// </summary>
        /// <param name="configureOptions">An action to configure the search options for filtering categories.</param>
        /// <returns>A read-only collection of category groups matching the search criteria.</returns>
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Action<BudgetCategorySearchOptions> configureOptions);
        
        /// <summary>
        /// Retrieves budget categories with the specified search options.
        /// </summary>
        /// <param name="budgetCategorySearchSettings">The search options for filtering categories.</param>
        /// <returns>A read-only collection of category groups matching the search criteria.</returns>
        Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(BudgetCategorySearchOptions budgetCategorySearchSettings);
    }
}
