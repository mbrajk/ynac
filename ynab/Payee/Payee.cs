using System.Text.Json.Serialization;

namespace ynab.Payee
{
    public class Payee
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("transfer_account_id")]
        public Guid? TransferAccountId { get; init; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; init; }
    }

    public class PayeesResponse
    {
        [JsonPropertyName("payees")]
        public IReadOnlyCollection<Payee> Payees { get; init; } = Array.Empty<Payee>();
    }

    public class PayeeResponse
    {
        [JsonPropertyName("payee")]
        public Payee? Payee { get; init; }
    }

    public class SavePayee
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }

    public class SavePayeeRequest
    {
        [JsonPropertyName("payee")]
        public required SavePayee Payee { get; init; }
    }
}
