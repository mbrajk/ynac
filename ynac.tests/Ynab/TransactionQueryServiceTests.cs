using FluentAssertions;
using NSubstitute;
using ynab;
using ynab.Transaction;

namespace ynac.Tests.Ynab;

[TestClass]
public class TransactionQueryServiceTests
{
    private static readonly ynab.Budget.Budget TestBudget = new() { Id = Guid.NewGuid() };

    [TestMethod]
    public async Task GetTransactions_FiltersDeleted_AndSortsNewestFirst()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetTransactionsAsync(TestBudget.BudgetId, null, null).Returns(new QueryResponse<TransactionsResponse>
        {
            Data = new TransactionsResponse
            {
                Transactions =
                [
                    new Transaction { Id = "old", Date = new DateOnly(2026, 1, 1) },
                    new Transaction { Id = "deleted", Date = new DateOnly(2026, 3, 1), Deleted = true },
                    new Transaction { Id = "new", Date = new DateOnly(2026, 2, 1) },
                ]
            }
        });
        var service = new TransactionQueryService(budgetApi);

        // Act
        var transactions = await service.GetTransactions(TestBudget);

        // Assert
        transactions.Select(transaction => transaction.Id).Should().Equal("new", "old");
    }

    [TestMethod]
    public async Task GetTransactions_PassesSinceDateAndFilterToApi()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(new QueryResponse<TransactionsResponse>());
        var service = new TransactionQueryService(budgetApi);

        // Act
        await service.GetTransactions(TestBudget, new DateOnly(2026, 6, 4), TransactionsFilter.Unapproved);

        // Assert
        await budgetApi.Received(1).GetTransactionsAsync(TestBudget.BudgetId, "2026-06-04", "unapproved");
    }

    [TestMethod]
    public async Task GetTransactions_ReturnsEmpty_WhenApiReturnsNoData()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetTransactionsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(new QueryResponse<TransactionsResponse>());
        var service = new TransactionQueryService(budgetApi);

        // Act
        var transactions = await service.GetTransactions(TestBudget);

        // Assert
        transactions.Should().BeEmpty();
    }

    [TestMethod]
    public async Task GetAccountTransactions_PassesAccountIdToApi()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetAccountTransactionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .Returns(new QueryResponse<TransactionsResponse>());
        var service = new TransactionQueryService(budgetApi);

        // Act
        await service.GetAccountTransactions(TestBudget, accountId);

        // Assert
        await budgetApi.Received(1).GetAccountTransactionsAsync(TestBudget.BudgetId, accountId.ToString(), null);
    }

    [TestMethod]
    [DataRow(TransactionsFilter.None, null)]
    [DataRow(TransactionsFilter.Unapproved, "unapproved")]
    [DataRow(TransactionsFilter.Uncategorized, "uncategorized")]
    public void ToTypeString_MapsFilterValues(TransactionsFilter filter, string? expected)
    {
        TransactionQueryService.ToTypeString(filter).Should().Be(expected);
    }
}
