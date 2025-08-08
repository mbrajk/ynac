using System.Globalization;
using ynac.CurrencyFormatting;

namespace ynac.Tests.CurrencyFormatting;

[TestClass]
public class CurrencyFormatterTests
{
    [TestMethod]
    [DataRow(1234.56, "$1,234.56")]
    [DataRow(-0.50, "-$0.50")] // Standard negative pattern for "C" in en-US
    [DataRow(0, "$0.00")]
    [DataRow(2000, "$2,000.00")]
    [DataRow(-789.12, "-$789.12")]
    public void DefaultCurrencyFormatter_FormatsCorrectly(double amountInput, string expectedOutput)
    {
        // Arrange
        var formatter = new DefaultCurrencyFormatter();
        var amount = (decimal)amountInput;

        // Act
        var actualOutput = formatter.Format(amount, CultureInfo.GetCultureInfo("en-US"));

        // Assert
        Assert.AreEqual(expectedOutput, actualOutput);
    }

    [TestMethod]
    public void DefaultCurrencyFormatter_UsesCurrentCulture_If_Not_Specified_Example_fr_FR()
    {
        // This test demonstrates the culture sensitivity of ToString("C").
        var formatter = new DefaultCurrencyFormatter();
        var amount = 1234.56m;
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentCulture = frenchCulture;
            CultureInfo.CurrentUICulture = frenchCulture;
            string expectedFrenchFormat = string.Format(frenchCulture, "{0:C}", amount);

            Assert.AreEqual(expectedFrenchFormat, formatter.Format(amount));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [TestMethod]
    [DataRow(1234.56)]
    [DataRow(-0.50)]
    [DataRow(0)]
    [DataRow(2000)]
    public void HiddenCurrencyFormatter_AlwaysReturnsHiddenString(double amountInput)
    {
        // Arrange
        var formatter = new HiddenCurrencyFormatter();
        var amount = (decimal)amountInput;
        var expectedOutput = "***.**"; // matches current HiddenCurrencyFormatter implementation

        // Act
        var actualOutput = formatter.Format(amount);

        // Assert
        Assert.AreEqual(expectedOutput, actualOutput);
    }
}
