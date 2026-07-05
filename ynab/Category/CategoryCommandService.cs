namespace ynab.Category
{
    public class CategoryCommandService(IBudgetApi budgetApi) : ICategoryCommandService
    {
        /// <summary>
        /// The month identifier YNAB uses to address the current month.
        /// </summary>
        public const string CurrentMonth = "current";

        public async Task<Category?> SetMonthCategoryBudgeted(Budget.Budget budget, string month, Guid categoryId, decimal budgetedMilliunits)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(month, nameof(month));

            var request = new SaveMonthCategoryRequest
            {
                Category = new SaveMonthCategory { Budgeted = budgetedMilliunits }
            };

            var response = await budgetApi.UpdateMonthCategoryAsync(budget.BudgetId, month, categoryId.ToString(), request);

            return response.Data?.Category;
        }

        public async Task<Category?> UpdateCategory(Budget.Budget budget, Guid categoryId, string? name = null, string? note = null)
        {
            var request = new SaveCategoryRequest
            {
                Category = new SaveCategory { Name = name, Note = note }
            };

            var response = await budgetApi.UpdateCategoryAsync(budget.BudgetId, categoryId.ToString(), request);

            return response.Data?.Category;
        }
    }
}
