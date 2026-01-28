using System.Text.Json.Serialization;

namespace ynac.JsonOutput;

[JsonSerializable(typeof(BudgetOutput))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class YnacJsonSerializerContext : JsonSerializerContext
{
}
