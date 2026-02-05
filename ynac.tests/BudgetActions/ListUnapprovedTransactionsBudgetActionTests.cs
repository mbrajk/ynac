using NSubstitute;
using ynab.Budget;
using ynab.Transaction;
using ynac.BudgetActions;
using ynac.BudgetSelection;

namespace ynac.Tests.BudgetActions;

[TestClass]
public class ListUnapprovedTransactionsBudgetActionTests
{
    private ITransactionQueryService _transactionService = null!;
    private IBudgetContext _budgetContext = null!;
    private IAnsiConsoleService _console = null!;
    private IValueFormatter _formatter = null!;
    private ListUnapprovedTransactionsBudgetAction _action = null!;

    [TestInitialize]
    public void Initialize()
    {
        _transactionService = Substitute.For<ITransactionQueryService>();
        _budgetContext = Substitute.For<IBudgetContext>();
        _console = Substitute.For<IAnsiConsoleService>();
        _formatter = Substitute.For<IValueFormatter>();
        _action = new ListUnapprovedTransactionsBudgetAction(
            _transactionService, _budgetContext, _console, _formatter);
    }

    [TestMethod]
    public void DisplayName_ReturnsCorrectValue()
    {
        // Act & Assert
        Assert.AreEqual("Manage Unapproved Transactions", _action.DisplayName);
    }

    [TestMethod]
    public void Order_Returns1()
    {
        // Act & Assert
        Assert.AreEqual(1, _action.Order);
    }

    [TestMethod]
    public void Execute_ShowsErrorWhenNoBudgetSelected()
    {
        // Arrange
        _budgetContext.CurrentBudget.Returns((Budget?)null);

        // Act
        _action.Execute();

        // Assert
        _console.Received(1).Markup("[red]No budget selected[/]\n");
    }
}
