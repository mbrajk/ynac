using System.Text.Json.Serialization;

namespace ynab.Account
{
    public class AccountResponse
    {
        [JsonPropertyName("accounts")]
        public IReadOnlyCollection<Account> Accounts { get; init; } = Array.Empty<Account>();
    }
}
