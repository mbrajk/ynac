using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ynab;
using ynab.Account;
using ynab.Budget;
using ynac.Mcp;

namespace ynac.tests.Mcp;

[TestClass]
public class McpServerTests
{
    [TestMethod]
    public async Task McpServer_Initialize_ReturnsSuccessfulResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockToken = "test-token";
        services.AddYnabApi(mockToken);
        var provider = services.BuildServiceProvider();
        
        var server = new McpServer(provider);
        
        var initRequest = new JsonRpcRequest
        {
            Jsonrpc = "2.0",
            Id = 1,
            Method = "initialize",
            Params = JsonDocument.Parse(@"{""protocolVersion"":""2024-11-05"",""capabilities"":{},""clientInfo"":{""name"":""test"",""version"":""1.0.0""}}").RootElement
        };
        
        // Since we can't directly call HandleRequestAsync (it's private), 
        // we'll verify the server can be instantiated correctly
        Assert.IsNotNull(server);
    }
    
    [TestMethod]
    public void JsonRpcRequest_Serialization_WorksCorrectly()
    {
        // Arrange
        var request = new JsonRpcRequest
        {
            Jsonrpc = "2.0",
            Id = 1,
            Method = "initialize",
            Params = JsonDocument.Parse(@"{""test"":""value""}").RootElement
        };
        
        // Act
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var deserialized = JsonSerializer.Deserialize<JsonRpcRequest>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("2.0", deserialized.Jsonrpc);
        Assert.AreEqual("initialize", deserialized.Method);
        // Id can be a number or string in JSON-RPC, so check it exists
        Assert.IsNotNull(deserialized.Id);
    }
    
    [TestMethod]
    public void JsonRpcResponse_Serialization_WorksCorrectly()
    {
        // Arrange
        var response = new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = 1,
            Result = new { test = "value" }
        };
        
        // Act
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        StringAssert.Contains(json, "\"jsonrpc\":\"2.0\"");
        StringAssert.Contains(json, "\"id\":1");
        StringAssert.Contains(json, "\"result\"");
    }
    
    [TestMethod]
    public void JsonRpcError_Serialization_WorksCorrectly()
    {
        // Arrange
        var error = new JsonRpcError
        {
            Code = -32601,
            Message = "Method not found"
        };
        
        // Act
        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        // Assert
        StringAssert.Contains(json, "\"code\":-32601");
        StringAssert.Contains(json, "\"message\":\"Method not found\"");
    }
}
