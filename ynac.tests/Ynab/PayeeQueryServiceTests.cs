using FluentAssertions;
using NSubstitute;
using ynab;
using ynab.Payee;

namespace ynac.Tests.Ynab;

[TestClass]
public class PayeeQueryServiceTests
{
    private static readonly ynab.Budget.Budget TestBudget = new() { Id = Guid.NewGuid() };

    [TestMethod]
    public async Task GetBudgetPayees_FiltersDeletedPayees()
    {
        // Arrange
        var budgetApi = Substitute.For<IBudgetApi>();
        budgetApi.GetBudgetPayeesAsync(TestBudget.BudgetId).Returns(new QueryResponse<PayeesResponse>
        {
            Data = new PayeesResponse
            {
                Payees =
                [
                    new Payee { Name = "Costco" },
                    new Payee { Name = "Old Store", Deleted = true },
                ]
            }
        });
        var service = new PayeeQueryService(budgetApi);

        // Act
        var payees = await service.GetBudgetPayees(TestBudget);

        // Assert
        payees.Should().ContainSingle(payee => payee.Name == "Costco");
    }
}
