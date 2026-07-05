using System.Text.Json.Serialization;

namespace ynab.User
{
    public class User
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }
    }

    public class UserResponse
    {
        [JsonPropertyName("user")]
        public User? User { get; init; }
    }
}
