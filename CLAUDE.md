# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the app
dotnet run --project ynac.cli

# Run with arguments
dotnet run --project ynac.cli -- [budgetFilter] [categoryFilter] [options]

# Publish self-contained binary (example: macOS ARM)
dotnet publish -c Release -r osx-arm64 --self-contained true
```

## Project Structure

- **ynac.cli**: CLI entry point, Spectre.Console UI, commands, actions, config handling
- **ynab**: API client, query services, models, JSON serialization context
- **ynac.tests**: MSTest tests with FluentAssertions and NSubstitute

## Architecture

### Flow
`Program.cs` → `BudgetCommand` (parses CLI, resolves token, builds DI) → `YnacConsoleProvider` → `YnacConsole.RunAsync()`

### API Layer (ynab project)
- `BudgetApi` implements `IBudgetApi` with `HttpClient` configured via `AddYnabApi(token)`
- Query services (`IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService`) wrap API calls
- **Currency values from YNAB API are in milliunits** - divide by 1000 for display

### Serialization
Uses System.Text.Json source generation. When adding new API models, update `YnabJsonSerializerContext` with `[JsonSerializable]` attributes.

### Configuration
- Token resolution order: CLI `--api-token` → `config.ini` [YnabApi] Token → env var `YnabApi__Token` → interactive prompt
- Config paths defined in `Constants.cs`

### Adding New Features
- **New CLI option**: Add to `BudgetCommandSettings`, handle in `BudgetCommand.ExecuteAsync()`
- **New budget action**: Implement `IBudgetAction`, register in `YnacConsoleProvider` - auto-appears in action menu
- **New API endpoint**: Extend `IBudgetApi`/`BudgetApi`, add model, update `YnabJsonSerializerContext`

## Key Conventions

- C# 13, .NET 10, nullable enabled, implicit usings
- Models use `init;` for immutability
- Async/await throughout
- Prefer defaults over null to avoid NRE in Spectre rendering
- Keep AI_INSTRUCTIONS.md updated when changing architecture
