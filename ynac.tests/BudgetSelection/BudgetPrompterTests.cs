using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Spectre.Console;
using ynab.Budget;
using ynac.BudgetSelection;

namespace ynac.Tests.BudgetSelection;

[TestClass]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class BudgetPrompterTests
{
    private readonly IAnsiConsoleService _console = Substitute.For<IAnsiConsoleService>();
    private BudgetPrompter _sut = null!;

    [TestInitialize]
    public void Initialize()
    {
        _sut = new BudgetPrompter(_console);
    }
    
    [TestMethod]
    public void PromptBudgetSelection_ReturnsNoBudgetIfEmpty()
    {
        // Arrange
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
        var budget = new Budget { Id = Guid.NewGuid(), Name = "Budget 1" };
        var budgets = new List<Budget> { budget };
        
        // Act
        var result = _sut.PromptBudgetSelection(budgets);
        
        // Assert
        Assert.AreEqual(budget.BudgetId, result.BudgetId);
        Assert.AreEqual(budget.Id, result.Id);
    }
    
    [TestMethod]
    public void PromptBudgetSelection_PromptsForBudgetIfMultiple()
    {
        // Arrange
        var budgets = new List<Budget>
        {
            new Budget { Id = Guid.NewGuid(), Name = "Budget 1" },
            new Budget { Id = Guid.NewGuid(), Name = "Budget 2" },
            new Budget { Id = Guid.NewGuid(), Name = "Budget 3" },
        };
        _console.Prompt(Arg.Any<SelectionPrompt<Budget>>()).Returns(budgets[1]);
                
        // Act
        var result = _sut.PromptBudgetSelection(budgets);
                
        // Assert
        Assert.AreEqual(budgets[1].BudgetId, result.BudgetId);
        Assert.AreEqual(budgets[1].Id, result.Id);
        _console.Received(1).Prompt(Arg.Any<SelectionPrompt<Budget>>());
    }
}