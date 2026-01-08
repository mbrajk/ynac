using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ynab.Account;
using ynab.Budget;

namespace ynac.Mcp;

public class McpServer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly McpJsonSerializerContext _jsonContext;
    private bool _initialized;
    private string? _budgetId;

    public McpServer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _jsonContext = new McpJsonSerializerContext(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public async Task RunAsync(string budgetId, CancellationToken cancellationToken = default)
    {
        _budgetId = budgetId;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await Console.In.ReadLineAsync(cancellationToken);
            if (line == null) break;

            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize(line, _jsonContext.JsonRpcRequest);
                if (request == null) continue;

                var response = await HandleRequestAsync(request, cancellationToken);
                var responseJson = JsonSerializer.Serialize(response, _jsonContext.JsonRpcResponse);
                await Console.Out.WriteLineAsync(responseJson);
                await Console.Out.FlushAsync();
            }
            catch (Exception ex)
            {
                var errorResponse = new JsonRpcResponse
                {
                    Jsonrpc = "2.0",
                    Id = null,
                    Error = new JsonRpcError
                    {
                        Code = -32603,
                        Message = $"Internal error: {ex.Message}"
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse, _jsonContext.JsonRpcResponse);
                await Console.Out.WriteLineAsync(errorJson);
                await Console.Out.FlushAsync();
            }
        }
    }

    private async Task<JsonRpcResponse> HandleRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "initialized" => HandleInitialized(request),
            "tools/list" => HandleToolsList(request),
            "tools/call" => await HandleToolsCallAsync(request, cancellationToken),
            _ => new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32601,
                    Message = $"Method not found: {request.Method}"
                }
            }
        };
    }

    private JsonRpcResponse HandleInitialize(JsonRpcRequest request)
    {
        _initialized = false;
        
        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = request.Id,
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "ynac-mcp-server",
                    version = "1.0.0"
                }
            }
        };
    }

    private JsonRpcResponse HandleInitialized(JsonRpcRequest request)
    {
        _initialized = true;
        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = request.Id,
            Result = new { }
        };
    }

    private JsonRpcResponse HandleToolsList(JsonRpcRequest request)
    {
        if (!_initialized)
        {
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32002,
                    Message = "Server not initialized"
                }
            };
        }

        return new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = request.Id,
            Result = new
            {
                tools = new[]
                {
                    new
                    {
                        name = "get_net_worth",
                        description = "Get the current net worth by summing all account balances in the budget",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new { },
                            required = Array.Empty<string>()
                        }
                    }
                }
            }
        };
    }

    private async Task<JsonRpcResponse> HandleToolsCallAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32002,
                    Message = "Server not initialized"
                }
            };
        }

        if (request.Params?.TryGetProperty("name", out var nameElement) != true)
        {
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32602,
                    Message = "Invalid params: missing 'name' field"
                }
            };
        }

        var toolName = nameElement.GetString();

        if (toolName != "get_net_worth")
        {
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32602,
                    Message = $"Unknown tool: {toolName}"
                }
            };
        }

        try
        {
            var netWorth = await GetNetWorthAsync(cancellationToken);
            
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Current net worth: {netWorth:C}"
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new JsonRpcResponse
            {
                Jsonrpc = "2.0",
                Id = request.Id,
                Error = new JsonRpcError
                {
                    Code = -32603,
                    Message = $"Error calculating net worth: {ex.Message}"
                }
            };
        }
    }

    private async Task<decimal> GetNetWorthAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_budgetId))
        {
            throw new InvalidOperationException("Budget ID not set");
        }

        var accountQueryService = _serviceProvider.GetRequiredService<IAccountQueryService>();
        var budgetQueryService = _serviceProvider.GetRequiredService<IBudgetQueryService>();
        
        // Get the budget
        var budgets = await budgetQueryService.GetBudgets();
        var budget = budgets.FirstOrDefault(b => b.BudgetId == _budgetId);
        
        if (budget == null)
        {
            throw new InvalidOperationException($"Budget not found: {_budgetId}");
        }

        // Get all accounts
        var accounts = await accountQueryService.GetBudgetAccounts(budget);
        
        // Sum all account balances and convert from milliunits to currency
        var totalBalance = accounts.Sum(a => a.Balance);
        return totalBalance / 1000m;
    }
}

public class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
    
    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

public class JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }
}

public class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
