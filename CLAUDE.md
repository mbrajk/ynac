# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run the app
dotnet run --project ynac.cli

# Run with arguments
dotnet run --project ynac.cli -- [budgetFilter] [categoryFilter] [options]

# Skip config.ini creation during development
dotnet run --project ynac.cli -- --debug-skip-config

# Publish self-contained binary (example: macOS ARM)
dotnet publish -c Release -r osx-arm64 --self-contained true
```

## Project Structure

- **ynac.cli**: CLI entry point, Spectre.Console UI, commands, actions, config handling
- **ynab**: API client, query services, models, JSON serialization context
- **ynac.tests**: MSTest tests with FluentAssertions and NSubstitute

## Architecture

### Flow
`Program.cs` → `BudgetCommand` (parses CLI, resolves token, builds DI) → `YnacConsoleProvider.BuildYnacServices()` → `YnacConsole.RunAsync()` → render budget table → action loop (prompt action → await execute → re-fetch if `IBudgetContext.DataStale` → re-render)

### API Layer (ynab project)
- `BudgetApi` implements `IBudgetApi` with named `HttpClient("BudgetApi")` configured via `AddYnabApi(token)` extension; base URL `https://api.ynab.com/v1/`
- Covers user, budgets, months, categories, accounts, payees, transactions and scheduled transactions, including writes (transaction CRUD/bulk PATCH/import, month-category budgeted, category/payee rename, scheduled transaction CRUD)
- Query services wrap reads (`IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService`, `IPayeeQueryService`, `ITransactionQueryService`, `IScheduledTransactionQueryService`, `IUserQueryService`); command services wrap writes (`ITransactionCommandService`, `ICategoryCommandService`, `IPayeeCommandService`, `IScheduledTransactionCommandService`)
- API responses use a `QueryResponse<T> { Data }` envelope; `Save*` write payloads omit null properties (unchanged on update)
- **Currency values from YNAB API are in milliunits** - divide by 1000 for display, multiply dollars by 1000 for write payloads
- YNAB rate limit is 200 requests/hour — use `since_date`/`type` filters, fetch with intent
- Error handling: 401 → `YnabAuthenticationException`, other HTTP errors → `YnabApiException`. API methods return defaults on failure; callers must handle null/empty `Data`
- HTTP resilience via `AddStandardResilienceHandler()` (retries/circuit breaker)

### Serialization
Uses System.Text.Json source generation. When adding new API models, update `YnabJsonSerializerContext` with `[JsonSerializable]` attributes.

### Configuration
- Token resolution order: CLI `--api-token` → `config.ini` [YnabApi] Token → env var `YnabApi__Token` → interactive prompt
- Config file created at first run from embedded template `ynac._res.config.template.ini`
- Config paths defined in `Constants.cs`

### DI and Action System
- All services registered as singletons in `YnacConsoleProvider.BuildYnacServices()`
- `IBudgetAction` implementations are explicitly registered and discovered via `IEnumerable<IBudgetAction>` injection
- Actions sorted by `Order` property in the selection prompt; `DisplayName` is re-evaluated each loop (supports dynamic text)
- Actions are async (`ExecuteAsync()`); `IBudgetContext` (singleton) carries the selected budget and a `DataStale` flag — write actions set it so the console re-fetches before re-rendering
- Write actions confirm before calling the API and go through `BudgetActionHelpers.RunWithAmountsRevealOffer` when amounts are hidden

### Currency Formatting
- Strategy pattern: `IValueFormatter` → `ICurrencyFormatterResolver` → `DefaultCurrencyFormatter` or `MaskedCurrencyFormatter`
- `ICurrencyVisibilityState` singleton toggleable at runtime via `ToggleHideAmountsBudgetAction`
- See `ynac.cli/CurrencyFormatting/INSTRUCTIONS.md` for detailed guide

### Adding New Features
- **New CLI option**: Add to `BudgetCommandSettings`, handle in `BudgetCommand.ExecuteAsync()`. Some flags are mutually exclusive (`--open` vs `--last-used`) — validation lives in the command
- **New budget action**: Implement `IBudgetAction` (with `DisplayName`, `Order`, `ExecuteAsync()`), register as singleton in `YnacConsoleProvider` — auto-appears in action menu; inject `IBudgetContext` for the selected budget and set `DataStale` after writes
- **New API endpoint**: Extend `IBudgetApi`/`BudgetApi`, add model, update `YnabJsonSerializerContext`

## Key Conventions

- C# 13, .NET 10, nullable enabled, implicit usings
- NuGet versions centrally managed in `Directory.Packages.props`
- Models use `init;` for immutability
- Async/await throughout
- Prefer defaults over null to avoid NRE in Spectre rendering
- `ynac.cli` and `ynab` have `[InternalsVisibleTo("ynac.tests")]` for test access (`ynab` also exposes internals to `DynamicProxyGenAssembly2` so NSubstitute can mock `IBudgetApi`)
- AOT/trimming supported — `Program.cs` uses `[DynamicDependency]` attributes for compatibility

## Testing Conventions

- MSTest with `[TestClass]`/`[TestMethod]`, FluentAssertions for assertions, NSubstitute for mocking
- Parametric tests via `[DataRow]` attributes
- Pattern: create real objects for simple types, substitute interfaces via `Substitute.For<T>()`
- Tests assume working directory aligns with `AppContext.BaseDirectory` for config.ini tests

## Reference

See [AI_INSTRUCTIONS.md](./AI_INSTRUCTIONS.md) for comprehensive architecture details — keep it updated when changing commands/flags, API calls, models, DI registrations, or configuration behavior.
