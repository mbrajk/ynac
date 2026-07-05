using System.Text.Json;
using FluentAssertions;
using ynab;
using ynab.Transaction;

namespace ynac.Tests.Ynab;

[TestClass]
public class YnabSerializationTests
{
    [TestMethod]
    public void SaveTransaction_OmitsNullProperties_WhenSerialized()
    {
        // Arrange
        var request = new SaveTransactionsRequest
        {
            Transactions = [new SaveTransaction { Id = "id-1", Approved = true }]
        };

        // Act
        var json = JsonSerializer.Serialize(request, YnabJsonSerializerContext.Default.SaveTransactionsRequest);

        // Assert
        json.Should().Be("""{"transactions":[{"id":"id-1","approved":true}]}""");
    }

    [TestMethod]
    public void SaveTransaction_SerializesDateAndAmount()
    {
        // Arrange
        var request = new SaveTransactionRequest
        {
            Transaction = new SaveTransaction
            {
                AccountId = Guid.Empty,
                Date = new DateOnly(2026, 7, 4),
                Amount = -12340,
                Cleared = TransactionClearedStatus.Cleared,
            }
        };

        // Act
        var json = JsonSerializer.Serialize(request, YnabJsonSerializerContext.Default.SaveTransactionRequest);

        // Assert
        json.Should().Contain("\"date\":\"2026-07-04\"")
            .And.Contain("\"amount\":-12340")
            .And.Contain("\"cleared\":\"cleared\"");
    }

    [TestMethod]
    public void Transaction_DeserializesFromApiShape()
    {
        // Arrange
        const string json = """
        {
            "data": {
                "transactions": [
                    {
                        "id": "t-1",
                        "date": "2026-07-01",
                        "amount": -45670,
                        "memo": "weekly run",
                        "cleared": "cleared",
                        "approved": false,
                        "account_id": "11111111-1111-1111-1111-111111111111",
                        "account_name": "Checking",
                        "payee_name": "Costco",
                        "category_id": null,
                        "category_name": null,
                        "deleted": false,
                        "subtransactions": []
                    }
                ],
                "server_knowledge": 42
            }
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize(json, YnabJsonSerializerContext.Default.QueryResponseTransactionsResponse);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data.ServerKnowledge.Should().Be(42);
        var transaction = response.Data.Transactions.Single();
        transaction.Date.Should().Be(new DateOnly(2026, 7, 1));
        transaction.Amount.Should().Be(-45670);
        transaction.Approved.Should().BeFalse();
        transaction.CategoryId.Should().BeNull();
        transaction.PayeeName.Should().Be("Costco");
    }
}
