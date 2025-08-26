using ynac.CurrencyFormatting;

namespace ynac.Tests.CurrencyFormatting;

[TestClass]
public class CurrencyFormatterResolverTests
{
    private readonly DefaultCurrencyFormatter defaultCurrencyFormatter = new();
    private readonly MaskedCurrencyFormatter maskedCurrencyFormatter = new();
    
    private readonly ICurrencyFormatterResolver _sut;

    public CurrencyFormatterResolverTests()
    {
        _sut = new CurrencyFormatterResolver(defaultCurrencyFormatter, maskedCurrencyFormatter);
    }

    [TestMethod]
    public void CurrencyFormatterResolver_Resolve_ReturnsMaskedCurrencyFormatter_WhenTrue()
    {
        // Arrange
        var isMasked = true;
        
        // Act
        var resolved = _sut.Resolve(isMasked);
        
        // Assert
        Assert.IsNotNull(resolved);
        Assert.IsTrue(resolved is MaskedCurrencyFormatter);
    }
    
    [TestMethod]
    public void CurrencyFormatterResolver_Resolve_ReturnsDefaultFormatter_WhenFalse()
    {
        // Arrange
        var isMasked = false;
        
        var resolved = _sut.Resolve(isMasked);
        
        Assert.IsNotNull(resolved);
        Assert.IsTrue(resolved is DefaultCurrencyFormatter);
    }
}