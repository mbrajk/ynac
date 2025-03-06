using System.Text.Json.Serialization;

namespace ynab
{
    public class QueryResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }
}
