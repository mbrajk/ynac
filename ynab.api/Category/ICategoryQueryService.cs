namespace YnabApi.Category;

public interface ICategoryQueryService
{
    Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategoriesAsync(Budget.Budget budget);
}