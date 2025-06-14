namespace ynac;

//currency considerations ( i know a library can do this but I am interested in all of the edgecases and pitfalls):
// currency type symbol $, yen, etc
// currency should be stored as a full number to avoid loss of precision and so formatting is easier
// ynab currency values are stored as 1000 * dollar value
// currency type symbol position (left or right)
// currency uses decimal or comma for positioning
// currency can be hidden but should otherwise respect positioning, decimal/comma, and symbol etc.
// 
public record Currency
{
    private decimal _amount = decimal.Zero;
    
    public Currency(decimal amount)
    {
        _amount = amount;
    }

    public override string ToString()
    {
        var currencyFormatter = new DefaultCurrencyFormatter();
        return currencyFormatter.FormatCurrency(_amount);
    }
    
    public string ToString(ICurrencyFormatter currencyFormatter) => currencyFormatter.FormatCurrency(_amount);
}

public interface ICurrencyFormatter
{
    string FormatCurrency(decimal amount);
}

// 
public enum CurrencyType
{
    Unknown,
    UnitedStatesDollar,
    ChineseRenMinBi,
    JapaneseYen,
    KoreanWon,
    BritishPound,
    EuropeanEuro,
    TaiwaneseDollar,
    HongKongDollar,
}

public static class CurrencySymbols
{
    private static readonly Dictionary<CurrencyType, string> Symbols = new()
    {
        { CurrencyType.Unknown, "$?" },
        { CurrencyType.UnitedStatesDollar, "$" },
        { CurrencyType.ChineseRenMinBi, "¥" },
        { CurrencyType.JapaneseYen, "¥" },
        { CurrencyType.KoreanWon, "₩" },
        { CurrencyType.BritishPound, "£" },
        { CurrencyType.EuropeanEuro, "€" },
        { CurrencyType.TaiwaneseDollar, "NT$" },
        { CurrencyType.HongKongDollar, "HK$" }
    };

    public static string GetSymbol(CurrencyType currencyType) => Symbols.TryGetValue(currencyType, out var symbol) ? symbol : Symbols[CurrencyType.Unknown];
}

public enum SymbolLocation
{
    Unknown,
    Before,
    After
}

// OCP means if currency formats are added we will need to edit this class
// ideally each type of formatting is handed by a separate formatter
// however this will be a bit unweildy for not much benefit. 
// Will need to experiment with a not stupid way of doing this.
public class DefaultCurrencyFormatter : ICurrencyFormatter
{
    private bool _hideCurrency = false;
    private int _decimalPlaces  = 2;
    private char _separator = ',';
    private char _fractionalSeparator = '.';
    private int _digitsBeforeSeparator = 3;
    
    private SymbolLocation _symbolLocation = SymbolLocation.Before;
    private CurrencyType _currencyType = CurrencyType.UnitedStatesDollar;

    public string FormatCurrency(decimal amount)
    {
        var visualAmount = _hideCurrency ? amount.ToString() : "****.**";
        if (_symbolLocation == SymbolLocation.Before)
        {
            return $"{CurrencySymbols.GetSymbol(_currencyType)} {amount}"; ;
        }
        
        if (_symbolLocation == SymbolLocation.After)
        {
            return $"{amount} {CurrencySymbols.GetSymbol(_currencyType)}"; ;
        }
        
        //FIXME: this is not a valid state
        return $"*****"; ;
    } 
}