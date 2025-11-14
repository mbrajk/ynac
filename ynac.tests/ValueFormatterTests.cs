using ynac.CurrencyFormatting;

namespace ynac.Tests;

[TestClass]
public class ValueFormatterTests
{
    private sealed class FakeCurrencyFormatter : ICurrencyFormatter
    {
        private readonly string _name;
        public FakeCurrencyFormatter(string name) => _name = name;
        public string Format(decimal amount) => $"{_name}:{amount}";
    }
    
    private sealed class CapturingResolver : ICurrencyFormatterResolver
    {
        public bool? LastMaskFlag { get; private set; }
        private readonly ICurrencyFormatter _unmasked;
        private readonly ICurrencyFormatter _masked;

        public CapturingResolver(ICurrencyFormatter unmasked, ICurrencyFormatter masked)
        {
            _unmasked = unmasked;
            _masked = masked;
        }

        public ICurrencyFormatter Resolve(bool isMasked)
        {
            LastMaskFlag = isMasked;
            return isMasked ? _masked : _unmasked;
        }
    }

    [TestMethod]
    public void Constructor_Defaults_To_Unmasked()
    {
        var resolver = new CapturingResolver(new FakeCurrencyFormatter("U"), new FakeCurrencyFormatter("M"));
        var sut = new ValueFormatter(resolver); 

        var result = sut.Format(10m);

        Assert.AreEqual("U:10", result);
        Assert.IsFalse(resolver.LastMaskFlag);
        Assert.IsFalse(sut.GetMasked());
    }

    [TestMethod]
    public void Constructor_Can_Start_Masked()
    {
        var resolver = new CapturingResolver(new FakeCurrencyFormatter("U"), new FakeCurrencyFormatter("M"));
        var sut = new ValueFormatter(resolver, initiallyMasked: true);

        var result = sut.Format(5m);

        Assert.AreEqual("M:5", result);
        Assert.IsTrue(resolver.LastMaskFlag);
        Assert.IsTrue(sut.GetMasked());
    }

    [TestMethod]
    public void SetMasked_Toggles_State_And_Affects_Decimal_Format()
    {
        var resolver = new CapturingResolver(new FakeCurrencyFormatter("U"), new FakeCurrencyFormatter("M"));
        var sut = new ValueFormatter(resolver);

        sut.SetMasked(true);
        var maskedResult = sut.Format(3.5m);

        sut.SetMasked(false);
        var unmaskedResult = sut.Format(7.2m);

        Assert.AreEqual("M:3.5", maskedResult);
        Assert.AreEqual("U:7.2", unmaskedResult);
    }

    [TestMethod]
    public void Format_Int_Masked_Returns_Asterisks()
    {
        var resolver = new CapturingResolver(new FakeCurrencyFormatter("U"), new FakeCurrencyFormatter("M"));
        var sut = new ValueFormatter(resolver, initiallyMasked: true);

        var result = sut.Format(999);

        Assert.AreEqual("***", result);
    }
}