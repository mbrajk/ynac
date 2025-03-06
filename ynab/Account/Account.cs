using System.Text.Json.Serialization;

namespace ynab.Account
{
    public class Account
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("balance")]
        public int Balance { get; init; }
    }
}
