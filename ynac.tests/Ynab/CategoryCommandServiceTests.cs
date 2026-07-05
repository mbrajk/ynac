using FluentAssertions;
using NSubstitute;
using ynab;
using ynab.Category;

namespace ynac.Tests.Ynab;

[TestClass]
public class CategoryCommandServiceTests
{
    private static readonly ynab.Budget.Budget TestBudget = new() { Id = Guid.NewGuid() };

    [TestMethod]
    public async Task SetMonthCategoryBudgeted_SendsMilliunitsToApi()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.UpdateMonthCategoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SaveMonthCategoryRequest>())
            .Returns(new QueryResponse<SingleCategoryResponse>
            {
                Data = new SingleCategoryResponse { Category = new Category { Id = categoryId, Budgeted = 250000 } }
            });
        var service = new CategoryCommandService(budgetApi);

        // Act
        var updated = await service.SetMonthCategoryBudgeted(TestBudget, CategoryCommandService.CurrentMonth, categoryId, 250000);

        // Assert
        updated.Should().NotBeNull();
        updated.Budgeted.Should().Be(250000);
        await budgetApi.Received(1).UpdateMonthCategoryAsync(
            TestBudget.BudgetId,
            "current",
            categoryId.ToString(),
            Arg.Is<SaveMonthCategoryRequest>(request => request.Category.Budgeted == 250000));
    }

    [TestMethod]
    public async Task SetMonthCategoryBudgeted_Throws_WhenMonthIsEmpty()
    {
        // Arrange
        var service = new CategoryCommandService(Substitute.For<IBudgetApi>());

        // Act
        var act = () => service.SetMonthCategoryBudgeted(TestBudget, "", Guid.NewGuid(), 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [TestMethod]
    public async Task UpdateCategory_SendsNameAndNote()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.UpdateCategoryAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<SaveCategoryRequest>())
            .Returns(new QueryResponse<SingleCategoryResponse>());
        var service = new CategoryCommandService(budgetApi);

        // Act
        await service.UpdateCategory(TestBudget, categoryId, "Groceries", "food only");

        // Assert
        await budgetApi.Received(1).UpdateCategoryAsync(
            TestBudget.BudgetId,
            categoryId.ToString(),
            Arg.Is<SaveCategoryRequest>(request => request.Category.Name == "Groceries" && request.Category.Note == "food only"));
    }
}
