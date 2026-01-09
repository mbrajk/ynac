# GitHub Copilot Instructions for ynac

## Project Overview

ynac is a cross-platform .NET 9 console application that displays YNAB (You Need A Budget) information using Spectre.Console for rich CLI output. It connects to the YNAB REST API to fetch and display budget data.

### Key Projects
- **ynac.cli**: CLI entry point, UI rendering (Spectre.Console), commands, and actions
- **ynab**: API client, query services, models, and System.Text.Json source generation
- **ynac.tests**: MSTest unit tests

## Architecture

### Entry Point
- `ynac.cli/Program.cs` initializes config and runs `BudgetCommand`
- `BudgetCommand` resolves API token, builds DI container, dispatches to `IYnacConsole.RunAsync`

### API Layer
- `IBudgetApi` implementation (`BudgetApi`) backed by HttpClient
- Base URL: `https://api.ynab.com/v1/`
- Query services: `IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService`
- All HTTP calls use `.AddStandardResilienceHandler()` for retries/circuit breaking

### Configuration
- Config file: `config.ini` created from `ynac._res.config.template.ini`
- API token stored in `[YnabApi]` section with `Token` key
- Token resolution: CLI option `--api-token` → config.ini → console prompt
- **Security**: Token stored in plain text; never log or commit it

## Important Data Contracts

### Currency Values
- **CRITICAL**: All currency values from YNAB API are in **milliunits** (thousandths)
- Always divide by 1000 before displaying to users
- Affected fields: `Category.Budgeted`, `Category.Activity`, `Category.Balance`, `BudgetMonth.ToBeBudgeted`

### Response Envelope
- API responses use `QueryResponse<T> { T? Data }` wrapper
- Always handle null/empty `Data` as failure state

### JSON Serialization
- Uses System.Text.Json with source generation (`YnabJsonSerializerContext`)
- **When adding new models**: Update `YnabJsonSerializerContext` with `[JsonSerializable]` attributes

## CLI Command Structure

### BudgetCommand Arguments
- `[budgetFilter]` - Budget name search (position 0)
- `[categoryFilter]` - Category name filter (position 1)

### Key Options
- `-o|--open` - Open budget in browser (conflicts with `--last-used`)
- `-g|--show-goals` - Show goal progress charts (experimental)
- `-u|--last-used` - Use last-used budget (conflicts with `--open`)
- `--api-token <token>` - Provide API token, persists to config.ini
- `--hide-amounts` - Mask all monetary amounts (privacy mode)
- `--show-hidden-categories` - Include hidden categories in display

## Coding Conventions

### C# Style
- C# 12 / .NET 9 patterns: primary constructors, file-scoped namespaces, records where appropriate
- Use `async/await` with `Task` consistently
- Models use `init;` for immutability
- Prefer defaults over null to avoid NRE in rendering

### Error Handling
- API methods catch exceptions, log to console, return defaults
- Higher layers must validate empty/missing `Data`
- CLI validates mutually exclusive flags

### Testing
- Framework: MSTest in `ynac.tests`
- Tests assume working directory aligns with `AppContext.BaseDirectory`

## UI Rendering with Spectre.Console

### Console Flow
1. Show header
2. Select budget (via `IBudgetSelector`)
3. Optional browser open
4. Load current month budget + categories
5. Render table with category groups
6. Prompt for actions (implementing `IBudgetAction`)

### Currency Formatting
- All amounts pass through `IValueFormatter` → `ICurrencyFormatter`
- Runtime toggling via `ICurrencyVisibilityState` (singleton)
- `--hide-amounts` sets initial state; can be toggled in-session via action
- Default formatter uses `ToString("C")` with current culture

## Extensibility

### Adding a New User Action
1. Implement `IBudgetAction` with `DisplayName`, `Order`, and `Execute()`
2. Register in DI container
3. Appears automatically in action picker
4. After `Execute()`, table re-renders automatically

### Adding API Endpoints
1. Extend `IBudgetApi` + `BudgetApi`
2. Add model classes
3. **Critical**: Update `YnabJsonSerializerContext` with `[JsonSerializable]` attributes

## Known Limitations

- Browser opener for last-used budget: URL builder uses `selectedBudget.Id` (which is `Guid.Empty` for `LastUsedBudget`) instead of `Budget.BudgetId` (which yields `"last-used"` string) for proper URL construction
- Goal display formatting is work-in-progress
- `BudgetSelector` cache doesn't refresh during session
- Token storage is plain text (not OS keychain)
- Error logging is basic `Console.WriteLine` (no structured logging)

## When Making Changes

### Always Update
- Update `AI_INSTRUCTIONS.md` when changing:
  - Commands/flags, API calls, models, configuration
  - Token or config handling
  - DI registrations or default behaviors
- Mark "Last reviewed" date

### Validation Checklist
1. Verify milliunits → currency conversion (÷ 1000)
2. Update `YnabJsonSerializerContext` for new models
3. Handle null/empty `Data` in API responses
4. Test CLI flags don't conflict
5. Verify config/token resolution still works
6. Add MSTest tests for new logic

## Reference Documentation

For comprehensive details, see [AI_INSTRUCTIONS.md](../AI_INSTRUCTIONS.md) in the repository root.

YNAB API documentation: https://api.ynab.com/
Spectre.Console: https://spectreconsole.net/
