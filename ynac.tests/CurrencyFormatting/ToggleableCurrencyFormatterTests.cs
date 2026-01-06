using ynac.CurrencyFormatting;

namespace ynac.Tests.CurrencyFormatting;

[TestClass]
public class ToggleableCurrencyFormatterTests
{
    [TestMethod]
    public void Format_ReturnsDefaultFormatted_WhenStateIsNotHidden()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var defaultFormatter = new DefaultCurrencyFormatter();
        var maskedFormatter = new MaskedCurrencyFormatter();
        var formatter = new ToggleableCurrencyFormatter(state, defaultFormatter, maskedFormatter);
        var amount = 1234.56m;

        // Act
        var result = formatter.Format(amount);

        // Assert
        Assert.AreEqual(defaultFormatter.Format(amount), result);
    }

    [TestMethod]
    public void Format_ReturnsMaskedFormatted_WhenStateIsHidden()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = true };
        var defaultFormatter = new DefaultCurrencyFormatter();
        var maskedFormatter = new MaskedCurrencyFormatter();
        var formatter = new ToggleableCurrencyFormatter(state, defaultFormatter, maskedFormatter);
        var amount = 1234.56m;

        // Act
        var result = formatter.Format(amount);

        // Assert
        Assert.AreEqual(maskedFormatter.Format(amount), result);
        Assert.AreEqual("***.**", result);
    }

    [TestMethod]
    public void Format_RespectsStateChanges()
    {
        // Arrange
        var state = new CurrencyVisibilityState { Hidden = false };
        var defaultFormatter = new DefaultCurrencyFormatter();
        var maskedFormatter = new MaskedCurrencyFormatter();
        var formatter = new ToggleableCurrencyFormatter(state, defaultFormatter, maskedFormatter);
        var amount = 100.50m;

        // Act - Initial state (not hidden)
        var resultNotHidden = formatter.Format(amount);

        // Change state to hidden
        state.Hidden = true;
        var resultHidden = formatter.Format(amount);

        // Change state back to not hidden
        state.Hidden = false;
        var resultNotHiddenAgain = formatter.Format(amount);

        // Assert
        Assert.AreEqual(defaultFormatter.Format(amount), resultNotHidden);
        Assert.AreEqual("***.**", resultHidden);
        Assert.AreEqual(defaultFormatter.Format(amount), resultNotHiddenAgain);
    }
}
