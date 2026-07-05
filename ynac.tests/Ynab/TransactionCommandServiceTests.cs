using FluentAssertions;
using NSubstitute;
using ynab;
using ynab.Transaction;

namespace ynac.Tests.Ynab;

[TestClass]
public class TransactionCommandServiceTests
{
    private static readonly ynab.Budget.Budget TestBudget = new() { Id = Guid.NewGuid() };

    [TestMethod]
    public async Task ApproveTransactions_SendsIdAndApprovedOnly()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.UpdateTransactionsAsync(Arg.Any<string>(), Arg.Any<SaveTransactionsRequest>())
            .Returns(new QueryResponse<SaveTransactionsResponse>());
        var service = new TransactionCommandService(budgetApi);

        // Act
        await service.ApproveTransactions(TestBudget, ["id-1", "id-2"]);

        // Assert
        await budgetApi.Received(1).UpdateTransactionsAsync(
            TestBudget.BudgetId,
            Arg.Is<SaveTransactionsRequest>(request =>
                request.Transactions.Count == 2 &&
                request.Transactions.All(transaction => transaction.Approved == true) &&
                request.Transactions.All(transaction => transaction.CategoryId == null)));
    }

    [TestMethod]
    public async Task CategorizeTransactions_SendsIdAndCategoryOnly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.UpdateTransactionsAsync(Arg.Any<string>(), Arg.Any<SaveTransactionsRequest>())
            .Returns(new QueryResponse<SaveTransactionsResponse>());
        var service = new TransactionCommandService(budgetApi);

        // Act
        await service.CategorizeTransactions(TestBudget, [("id-1", categoryId)]);

        // Assert
        await budgetApi.Received(1).UpdateTransactionsAsync(
            TestBudget.BudgetId,
            Arg.Is<SaveTransactionsRequest>(request =>
                request.Transactions.Count == 1 &&
                request.Transactions.Single().Id == "id-1" &&
                request.Transactions.Single().CategoryId == categoryId &&
                request.Transactions.Single().Approved == null));
    }

    [TestMethod]
    public async Task UpdateTransactions_SkipsApiCall_WhenListIsEmpty()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        var service = new TransactionCommandService(budgetApi);

        // Act
        var updated = await service.UpdateTransactions(TestBudget, []);

        // Assert
        updated.Should().BeEmpty();
        await budgetApi.DidNotReceive().UpdateTransactionsAsync(Arg.Any<string>(), Arg.Any<SaveTransactionsRequest>());
    }

    [TestMethod]
    public async Task CreateTransaction_ReturnsCreatedTransaction()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.CreateTransactionAsync(Arg.Any<string>(), Arg.Any<SaveTransactionRequest>())
            .Returns(new QueryResponse<SaveTransactionsResponse>
            {
                Data = new SaveTransactionsResponse { Transaction = new Transaction { Id = "created" } }
            });
        var service = new TransactionCommandService(budgetApi);

        // Act
        var created = await service.CreateTransaction(TestBudget, new SaveTransaction { Amount = -12340 });

        // Assert
        created.Should().NotBeNull();
        created.Id.Should().Be("created");
    }
}
