﻿using ynab.Category;

namespace ynab.Budget
{
    public record BudgetCategorySearchOptions
    {
        public Budget? SelectedBudget { get; set; }
        public string? CategoryFilter { get; set; }
        public bool ShowHiddenCategories { get; set; }
        public bool ShowDeletedCategories { get; set; }
    }

    public class BudgetQueryService(IBudgetApi budgetApi) : IBudgetQueryService
    {
        private BudgetCategorySearchOptions _options = new();
        private void Configure(Action<BudgetCategorySearchOptions> budgetAction)
        {
            budgetAction(_options);
        }
        
        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(Action<BudgetCategorySearchOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(configureOptions, nameof(configureOptions));
            Configure(configureOptions);
            
            return await GetBudgetCategories();
        }

        public async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories(BudgetCategorySearchOptions budgetCategorySearchSettings)
        {
            ArgumentNullException.ThrowIfNull(budgetCategorySearchSettings, nameof(budgetCategorySearchSettings));
            _options = budgetCategorySearchSettings;

            return await GetBudgetCategories();
        }

        private async Task<IReadOnlyCollection<CategoryGroup>> GetBudgetCategories()
        {
            ArgumentNullException.ThrowIfNull(_options.SelectedBudget, nameof(_options.SelectedBudget));
            
            var response = await budgetApi.GetBudgetCategoriesAsync(_options.SelectedBudget.BudgetId);

            // first group is the "Internal Master Category" used by YNAB, so we skip it
            var filteredGroups = response.Data?.Groups?
                .Where(group => !group.Hidden)
                .Where(group => !group.Deleted)
                .Where(group => group.Categories.Any())
                .Skip(1);

            if (!string.IsNullOrWhiteSpace(_options.CategoryFilter))
            {
                filteredGroups = filteredGroups?
                    .Where(group => group.Name.Contains(_options.CategoryFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            return filteredGroups?.ToList() ?? [new CategoryGroup()];
        }
        
        public async Task<IReadOnlyCollection<Budget>> GetBudgets()
        {
            var response = await budgetApi.GetBudgetsAsync();

            var budgets = response.Data?.Budgets;
            
            if (budgets == null || budgets.Count is 0)
            {
                return [ Budget.NoBudget ];
            }

            return budgets; 
        }
        
        public async Task<BudgetMonth> GetBudgetMonth(Budget budget, DateOnly date)
        {
            var dateModified = date;

            if (date.Year > DateTime.UtcNow.Year)
            {
                dateModified = new DateOnly(DateTime.UtcNow.Year, date.Month, 1);
            }

            var dateString = dateModified.ToString("yyyy-MM-01");
            var response = await budgetApi.GetBudgetMonthAsync(budget.BudgetId, dateString);
        
            return response.Data?.Budget ?? new BudgetMonth();
        } 
        
        public Task<BudgetMonth> GetCurrentMonthBudget(Budget budget)
        {
            return GetBudgetMonth(budget, DateOnly.FromDateTime(DateTime.UtcNow));
        }
    }
}
