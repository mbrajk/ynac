using System.Text.Json.Serialization;

namespace YnabApi.Account
{
    public class Account
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("balance")]
        public int Balance { get; init; }
    }
}
