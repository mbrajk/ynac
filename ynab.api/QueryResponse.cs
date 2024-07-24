using System.Text.Json.Serialization;

namespace YnabApi
{
    public class QueryResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }
}
