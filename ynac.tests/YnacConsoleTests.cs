using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.Commands;
using ynac.OSFeatures;
using ynab.Budget;
using Spectre.Console;

namespace ynac.Tests;

[TestClass]
public class YnacConsoleTests
{
    private IBudgetQueryService _budgetQueryService = null!;
    private IBudgetBrowserOpener _budgetBrowserOpener = null!;
    private IBudgetSelector _budgetSelector = null!;
    private IEnumerable<IBudgetAction> _budgetActions = null!;
    private IValueFormatter _valueFormatter = null!;
    private IAnsiConsoleService _ansiConsoleService = null!;
    private YnacConsoleSettings _ynacSettings = null!;
    private YnacConsole _sut = null!;

    [TestInitialize]
    public void Initialize()
    {
        _budgetQueryService = Substitute.For<IBudgetQueryService>();
        _budgetBrowserOpener = Substitute.For<IBudgetBrowserOpener>();
        _budgetSelector = Substitute.For<IBudgetSelector>();
        _budgetActions = new List<IBudgetAction>();
        _valueFormatter = Substitute.For<IValueFormatter>();
        _ansiConsoleService = Substitute.For<IAnsiConsoleService>();
        _ynacSettings = new YnacConsoleSettings("token", false, null);

        _sut = new YnacConsole(
            _budgetQueryService,
            _budgetBrowserOpener,
            _budgetSelector,
            _budgetActions,
            _valueFormatter,
            _ansiConsoleService,
            _ynacSettings
        );
        
        var exitAction = Substitute.For<IBudgetAction>();
        exitAction.When(x => x.Execute()).Do(x => { throw new Exception("Exit loop"); });
        _ansiConsoleService.Prompt(Arg.Any<SelectionPrompt<IBudgetAction>>()).Returns(exitAction);
    }

    [TestMethod]
    public async Task RunAsync_UsesDefaultBudget_WhenFilterIsEmptyAndNotPullLastUsed()
    {
        // Arrange
        var defaultBudget = "MyDefaultBudget";
        _ynacSettings = new YnacConsoleSettings("token", false, defaultBudget);
        _sut = new YnacConsole(
            _budgetQueryService,
            _budgetBrowserOpener,
            _budgetSelector,
            _budgetActions,
            _valueFormatter,
            _ansiConsoleService,
            _ynacSettings
        );

        var settings = new BudgetCommandSettings { BudgetFilter = null, PullLastUsed = false };
        _budgetSelector.SelectBudget(Arg.Any<string>(), Arg.Any<bool>()).Returns(new Budget { Name = "FoundBudget" });
        _budgetQueryService.GetCurrentMonthBudget(Arg.Any<Budget>()).Returns(new BudgetMonth());

        // Act
        try
        {
            await _sut.RunAsync(settings);
        }
        catch (Exception ex) when (ex.Message == "Exit loop")
        {
            // Ignore
        }

        // Assert
        await _budgetSelector.Received(1).SelectBudget(defaultBudget, false);
    }

    [TestMethod]
    public async Task RunAsync_IgnoresDefaultBudget_WhenFilterIsProvided()
    {
        // Arrange
        var defaultBudget = "MyDefaultBudget";
        var explicitFilter = "ExplicitBudget";
        _ynacSettings = new YnacConsoleSettings("token", false, defaultBudget);
        _sut = new YnacConsole(
            _budgetQueryService,
            _budgetBrowserOpener,
            _budgetSelector,
            _budgetActions,
            _valueFormatter,
            _ansiConsoleService,
            _ynacSettings
        );

        var settings = new BudgetCommandSettings { BudgetFilter = explicitFilter, PullLastUsed = false };
        _budgetSelector.SelectBudget(Arg.Any<string>(), Arg.Any<bool>()).Returns(new Budget { Name = "FoundBudget" });
        _budgetQueryService.GetCurrentMonthBudget(Arg.Any<Budget>()).Returns(new BudgetMonth());

        // Act
        try
        {
            await _sut.RunAsync(settings);
        }
        catch (Exception ex) when (ex.Message == "Exit loop")
        {
            // Ignore
        }

        // Assert
        await _budgetSelector.Received(1).SelectBudget(explicitFilter, false);
    }

    [TestMethod]
    public async Task RunAsync_IgnoresDefaultBudget_WhenPullLastUsedIsTrue()
    {
        // Arrange
        var defaultBudget = "MyDefaultBudget";
        _ynacSettings = new YnacConsoleSettings("token", false, defaultBudget);
        _sut = new YnacConsole(
            _budgetQueryService,
            _budgetBrowserOpener,
            _budgetSelector,
            _budgetActions,
            _valueFormatter,
            _ansiConsoleService,
            _ynacSettings
        );

        var settings = new BudgetCommandSettings { BudgetFilter = null, PullLastUsed = true };
        _budgetSelector.SelectBudget(Arg.Any<string>(), Arg.Any<bool>()).Returns(new Budget { Name = "FoundBudget" });
        _budgetQueryService.GetCurrentMonthBudget(Arg.Any<Budget>()).Returns(new BudgetMonth());

        // Act
        try
        {
            await _sut.RunAsync(settings);
        }
        catch (Exception ex) when (ex.Message == "Exit loop")
        {
            // Ignore
        }

        // Assert
        await _budgetSelector.Received(1).SelectBudget("", true);
    }
}
