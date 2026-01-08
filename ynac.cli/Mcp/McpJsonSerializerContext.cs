using System.Text.Json.Serialization;

namespace ynac.Mcp;

[JsonSerializable(typeof(JsonRpcRequest))]
[JsonSerializable(typeof(JsonRpcResponse))]
[JsonSerializable(typeof(JsonRpcError))]
internal partial class McpJsonSerializerContext : JsonSerializerContext
{
}
