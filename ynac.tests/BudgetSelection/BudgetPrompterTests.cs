using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YnabApi.Budget;
using ynac.BudgetSelection;

namespace ynac.Tests.BudgetSelection;

[TestClass]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class BudgetPrompterTests
{
    private BudgetPrompter? _sut;

    [TestMethod]
    public void PromptBudgetSelection_ReturnsNoBudgetIfEmpty()
    {
        // Arrange
        _sut = new BudgetPrompter();
        var budgets = new List<Budget>();
        
        // Act
        var result = _sut.PromptBudgetSelection(budgets);
        
        // Assert
        Assert.AreEqual(Budget.NoBudget, result);
    }
    
    [TestMethod]
    public void PromptBudgetSelection_ReturnsFirstBudgetIfOnlyOne()
    {
        // Arrange
        _sut = new BudgetPrompter();
        var budget = new Budget { Id = Guid.NewGuid(), Name = "Budget 1" };
        var budgets = new List<Budget> { budget };
        
        // Act
        var result = _sut.PromptBudgetSelection(budgets);
        
        // Assert
        Assert.AreEqual(budget.BudgetId, result.BudgetId);
        Assert.AreEqual(budget.Id, result.Id);
    }
    
    
}