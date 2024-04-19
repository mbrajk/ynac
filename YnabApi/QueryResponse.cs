using System.Text.Json.Serialization;

namespace ynac.YnabApi
{
    public class QueryResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }
}
