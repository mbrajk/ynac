using FluentAssertions;
using NSubstitute;
using ynab;
using ynab.ScheduledTransaction;

namespace ynac.Tests.Ynab;

[TestClass]
public class ScheduledTransactionQueryServiceTests
{
    private static readonly ynab.Budget.Budget TestBudget = new() { Id = Guid.NewGuid() };

    [TestMethod]
    public async Task GetScheduledTransactions_FiltersDeleted_AndOrdersByNextDate()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetScheduledTransactionsAsync(TestBudget.BudgetId).Returns(new QueryResponse<ScheduledTransactionsResponse>
        {
            Data = new ScheduledTransactionsResponse
            {
                ScheduledTransactions =
                [
                    new ScheduledTransaction { Memo = "later", DateNext = new DateOnly(2026, 8, 1) },
                    new ScheduledTransaction { Memo = "deleted", DateNext = new DateOnly(2026, 7, 1), Deleted = true },
                    new ScheduledTransaction { Memo = "sooner", DateNext = new DateOnly(2026, 7, 10) },
                ]
            }
        });
        var service = new ScheduledTransactionQueryService(budgetApi);

        // Act
        var scheduledTransactions = await service.GetScheduledTransactions(TestBudget);

        // Assert
        scheduledTransactions.Select(scheduledTransaction => scheduledTransaction.Memo).Should().Equal("sooner", "later");
    }

    [TestMethod]
    public async Task GetScheduledTransactions_ReturnsEmpty_WhenApiReturnsNoData()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetScheduledTransactionsAsync(Arg.Any<string>()).Returns(new QueryResponse<ScheduledTransactionsResponse>());
        var service = new ScheduledTransactionQueryService(budgetApi);

        // Act
        var scheduledTransactions = await service.GetScheduledTransactions(TestBudget);

        // Assert
        scheduledTransactions.Should().BeEmpty();
    }
}
