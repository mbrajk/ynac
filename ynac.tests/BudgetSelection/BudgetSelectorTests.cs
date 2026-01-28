using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ynab.Budget;
using ynac.BudgetSelection;
using FluentAssertions;

namespace ynac.Tests.BudgetSelection;

[TestClass]
public class BudgetSelectorTests
{
    private IBudgetQueryService _budgetQueryService = null!;
    private IBudgetPrompter _budgetPrompter = null!;
    private BudgetSelector _sut = null!;

    [TestInitialize]
    public void Initialize()
    {
        _budgetQueryService = Substitute.For<IBudgetQueryService>();
        _budgetPrompter = Substitute.For<IBudgetPrompter>();
        _sut = new BudgetSelector(_budgetQueryService, _budgetPrompter);
    }

    [TestMethod]
    public async Task SelectBudget_WithLastBudgetFlag_ReturnsLastUsedBudget()
    {
        // Act
        var result = await _sut.SelectBudget("", selectLastBudget: true);

        // Assert
        result.Should().Be(Budget.LastUsedBudget);
        await _budgetQueryService.DidNotReceive().GetBudgets();
    }

    [TestMethod]
    public async Task SelectBudget_WithNoBudgetsFromApi_ReturnsNoBudget()
    {
        // Arrange
        _budgetQueryService.GetBudgets().Returns(Array.Empty<Budget>());

        // Act
        var result = await _sut.SelectBudget("", selectLastBudget: false);

        // Assert
        result.Should().Be(Budget.NoBudget);
    }

    [TestMethod]
    public async Task SelectBudget_WithGuidFilter_ReturnsMatchingBudget()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = new Budget { Id = budgetId, Name = "Test Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget });

        // Act
        var result = await _sut.SelectBudget(budgetId.ToString(), selectLastBudget: false);

        // Assert
        result.Id.Should().Be(budgetId);
        result.Name.Should().Be("Test Budget");
    }

    [TestMethod]
    public async Task SelectBudget_WithGuidFilterNoMatch_ReturnsNoBudget()
    {
        // Arrange
        var budget = new Budget { Id = Guid.NewGuid(), Name = "Test Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget });

        // Act
        var result = await _sut.SelectBudget(Guid.NewGuid().ToString(), selectLastBudget: false);

        // Assert
        result.Should().Be(Budget.NoBudget);
    }

    [TestMethod]
    public async Task SelectBudget_NonInteractiveWithEmptyFilter_ReturnsLastUsedBudget()
    {
        // Arrange
        var budget1 = new Budget { Id = Guid.NewGuid(), Name = "Budget 1" };
        var budget2 = new Budget { Id = Guid.NewGuid(), Name = "Budget 2" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget1, budget2 });

        // Act
        var result = await _sut.SelectBudget("", selectLastBudget: false, nonInteractive: true);

        // Assert
        result.Should().Be(Budget.LastUsedBudget); // First in the list is LastUsedBudget when no filter
        _budgetPrompter.DidNotReceive().PromptBudgetSelection(Arg.Any<IReadOnlyCollection<Budget>>());
    }

    [TestMethod]
    public async Task SelectBudget_NonInteractiveWithMatchingFilter_ReturnsFirstMatch()
    {
        // Arrange
        var budget1 = new Budget { Id = Guid.NewGuid(), Name = "Personal Budget" };
        var budget2 = new Budget { Id = Guid.NewGuid(), Name = "Business Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget1, budget2 });

        // Act
        var result = await _sut.SelectBudget("Personal", selectLastBudget: false, nonInteractive: true);

        // Assert
        result.Id.Should().Be(budget1.Id);
        result.Name.Should().Be("Personal Budget");
        _budgetPrompter.DidNotReceive().PromptBudgetSelection(Arg.Any<IReadOnlyCollection<Budget>>());
    }

    [TestMethod]
    public async Task SelectBudget_NonInteractiveWithNoMatchingFilter_ReturnsNoBudget()
    {
        // Arrange
        var budget1 = new Budget { Id = Guid.NewGuid(), Name = "Personal Budget" };
        var budget2 = new Budget { Id = Guid.NewGuid(), Name = "Business Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget1, budget2 });

        // Act
        var result = await _sut.SelectBudget("NonExistent", selectLastBudget: false, nonInteractive: true);

        // Assert
        result.Should().Be(Budget.NoBudget);
        _budgetPrompter.DidNotReceive().PromptBudgetSelection(Arg.Any<IReadOnlyCollection<Budget>>());
    }

    [TestMethod]
    public async Task SelectBudget_InteractiveModeWithFilter_CallsPrompter()
    {
        // Arrange
        var budget1 = new Budget { Id = Guid.NewGuid(), Name = "Personal Budget" };
        var budget2 = new Budget { Id = Guid.NewGuid(), Name = "Business Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget1, budget2 });
        _budgetPrompter.PromptBudgetSelection(Arg.Any<IReadOnlyCollection<Budget>>()).Returns(budget1);

        // Act
        var result = await _sut.SelectBudget("Personal", selectLastBudget: false, nonInteractive: false);

        // Assert
        result.Should().Be(budget1);
        _budgetPrompter.Received(1).PromptBudgetSelection(Arg.Any<IReadOnlyCollection<Budget>>());
    }

    [TestMethod]
    public async Task SelectBudget_CachesBudgets_OnMultipleCalls()
    {
        // Arrange
        var budget = new Budget { Id = Guid.NewGuid(), Name = "Test Budget" };
        _budgetQueryService.GetBudgets().Returns(new[] { budget });

        // Act
        await _sut.SelectBudget("", selectLastBudget: false, nonInteractive: true);
        await _sut.SelectBudget("", selectLastBudget: false, nonInteractive: true);

        // Assert
        await _budgetQueryService.Received(1).GetBudgets(); // Should only call once
    }
}
