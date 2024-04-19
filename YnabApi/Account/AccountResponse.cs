using System.Text.Json.Serialization;

namespace ynac.YnabApi.Account
{
    public class AccountResponse
    {
        [JsonPropertyName("accounts")]
        public IReadOnlyCollection<Account> Accounts { get; set; }
    }
}
