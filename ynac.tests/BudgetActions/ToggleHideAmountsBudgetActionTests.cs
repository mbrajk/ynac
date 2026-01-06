using NSubstitute;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.CurrencyFormatting;

namespace ynac.Tests.BudgetActions;

[TestClass]
public class ToggleHideAmountsBudgetActionTests
{
    [TestMethod]
    public void Order_IsZero()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act & Assert
        Assert.AreEqual(0, action.Order);
    }

    [TestMethod]
    public void DisplayName_ShowsHideAmounts_WhenCurrentlyShown()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act & Assert
        Assert.AreEqual("Hide amounts", action.DisplayName);
    }

    [TestMethod]
    public void DisplayName_ShowsShowAmounts_WhenCurrentlyHidden()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = true };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act & Assert
        Assert.AreEqual("Show amounts", action.DisplayName);
    }

    [TestMethod]
    public void Execute_TogglesStateFromShownToHidden()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act
        action.Execute();

        // Assert
        Assert.IsTrue(state.Hidden);
        console.Received(1).Markup(Arg.Is<string>(s => s.Contains("hidden")));
    }

    [TestMethod]
    public void Execute_TogglesStateFromHiddenToShown()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = true };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act
        action.Execute();

        // Assert
        Assert.IsFalse(state.Hidden);
        console.Received(1).Markup(Arg.Is<string>(s => s.Contains("shown")));
    }

    [TestMethod]
    public void Execute_TogglesStateMultipleTimes()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var console = Substitute.For<IAnsiConsoleService>();
        var action = new ToggleHideAmountsBudgetAction(state, console);

        // Act & Assert
        Assert.IsFalse(state.Hidden);
        
        action.Execute();
        Assert.IsTrue(state.Hidden);
        
        action.Execute();
        Assert.IsFalse(state.Hidden);
        
        action.Execute();
        Assert.IsTrue(state.Hidden);
    }
}
