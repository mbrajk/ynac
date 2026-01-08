# MCP (Model Context Protocol) Server

The ynac application can run as an MCP server, exposing budget data through the Model Context Protocol to AI assistants and other tools.

## What is MCP?

The Model Context Protocol (MCP) is an open protocol that standardizes how AI applications communicate with external data sources. It uses JSON-RPC 2.0 over stdio for communication.

## Starting the MCP Server

To start the MCP server, use the `--start-mcp` flag:

```bash
ynac [budgetFilter] --start-mcp
```

For example:
```bash
ynac "My Budget" --start-mcp
```

Or to use the last-used budget:
```bash
ynac --last-used --start-mcp
```

## Available Tools

The MCP server currently exposes one tool:

### `get_net_worth`

Returns the current net worth by summing all account balances in the budget.

- **Input**: None
- **Output**: Text containing the current net worth in currency format

## Protocol Flow

The MCP server implements the following JSON-RPC 2.0 methods:

1. **initialize** - Handshake between client and server
2. **initialized** - Notification that initialization is complete
3. **tools/list** - Returns list of available tools
4. **tools/call** - Executes a specific tool

## Example Usage

Here's an example of the protocol flow:

### 1. Initialize the connection

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {"name": "my-client", "version": "1.0.0"}
  }
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "protocolVersion": "2024-11-05",
    "capabilities": {"tools": {}},
    "serverInfo": {"name": "ynac-mcp-server", "version": "1.0.0"}
  }
}
```

### 2. Notify initialization complete

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "initialized",
  "params": {}
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {}
}
```

### 3. List available tools

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/list",
  "params": {}
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "tools": [
      {
        "name": "get_net_worth",
        "description": "Get the current net worth by summing all account balances in the budget",
        "inputSchema": {
          "type": "object",
          "properties": {},
          "required": []
        }
      }
    ]
  }
}
```

### 4. Call the get_net_worth tool

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "tools/call",
  "params": {
    "name": "get_net_worth",
    "arguments": {}
  }
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Current net worth: $12,345.67"
      }
    ]
  }
}
```

## Integrating with AI Tools

To integrate the ynac MCP server with AI tools:

1. Configure your AI tool to connect to a stdio-based MCP server
2. Set the command to run ynac with the `--start-mcp` flag
3. Ensure your YNAB API token is configured (via `config.ini` or `--api-token`)
4. The AI tool can then query your budget data through the MCP protocol

## Future Enhancements

The MCP server is designed to be extensible. Future versions may include additional tools such as:

- Last 30 days of transactions
- Current spending/activity in specific categories
- Category group summaries
- Budget vs. actual analysis
- And more...

## Technical Details

- Protocol: JSON-RPC 2.0 over stdin/stdout
- Implementation: `ynac.cli/Mcp/McpServer.cs`
- Uses source-generated JSON serialization for AOT compatibility
- Supports trimmed and self-contained builds
