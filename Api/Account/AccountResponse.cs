using System.Text.Json.Serialization;

namespace YnabApi.Account
{
    public class AccountResponse
    {
        [JsonPropertyName("accounts")]
        public IReadOnlyCollection<Account> Accounts { get; set; }
    }
}
