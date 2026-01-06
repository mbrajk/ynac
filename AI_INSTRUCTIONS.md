## AI instructions: ynac project

This document orients AI tooling and contributors to the ynac codebase. Treat keeping this document accurate as high-priority technical debt. When you change public behavior, add features, or refactor structure, update this guide in the same PR.

Last reviewed: 2026-01-06


## Purpose and scope

- Goal: Console app (Spectre.Console) that displays YNAB budget information via the YNAB REST API.
- Projects:
  - ynac.cli: CLI and UI (Spectre.Console), orchestration, config, OS helpers, commands, actions.
  - ynab: API client + query services + models + System.Text.Json source-gen context.
  - ynab.api: placeholder/auxiliary (currently minimal; not primary entry point).
  - ynac.tests: MSTest tests (currently focused on token/config handling).


## High-level architecture

- CLI entry: `ynac.cli/Program.cs` initializes config file then runs `BudgetCommand`.
- Command: `BudgetCommand` parses settings, resolves API token, builds DI container via `YnacConsoleProvider` (see `ynac.cli`), and dispatches to `IYnacConsole.RunAsync`.
- Console flow: `YnacConsole` shows header, selects budget (via `IBudgetSelector`), optional open-in-browser, loads current-month budget + categories, renders table, then prompts for actions (`IBudgetAction`).
- API layer: `ynab` project provides `IBudgetApi` implementation (`BudgetApi`) backed by `HttpClient` configured by `AddYnabApi(token)` extension.
- Query services: `IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService` encapsulate higher-level use-cases on top of API.
- Models: `Budget`, `BudgetMonth`, `CategoryGroup`, `Category`, `Account` (see `ynab/*`) mirror YNAB API shapes (System.Text.Json).


## Configuration and secrets

- Config file: `config.ini` is created at first run from embedded template `ynac._res.config.template.ini` into `AppContext.BaseDirectory`.
- Key of interest: `[YnabApi]` section with `Token` entry. Accessed via `Constants.YnabApiTokenConfigPath` ("YnabApi:Token").
- Token handling:
  - `BudgetCommand` reads token from CLI option `--api-token` or `config.ini` or, if missing, prompts on console.
  - `TokenHandler.MaybeSaveToken` persists CLI-provided token into `config.ini` (if non-empty and not placeholder `put-your-token-here`).
  - Security: token is stored in plain text; do not log it. Do not commit `config.ini`.


## Data contracts and units

- Currency values from YNAB are in milliunits. Code divides by 1000 to display dollars.
  - Example fields: `Category.Budgeted`, `Category.Activity`, `Category.Balance`, `BudgetMonth.ToBeBudgeted`.
- `BudgetMonth.AgeOfMoney` is nullable `int?`.
- QueryResponse wrapper: `QueryResponse<T> { T? Data }` matches YNAB API response envelope.
- `Budget` special sentinels:
  - `Budget.LastUsedBudget` (Type = LastUsed, `BudgetId` resolves to "last-used").
  - `Budget.NoBudget` (Type = NotFound) used when no budgets are available.


## HTTP/API client

- `BudgetApi` methods:
  - `GetBudgetsAsync()` -> `QueryResponse<BudgetResponse>`
  - `GetBudgetMonthAsync(budgetId, month)` -> `QueryResponse<BudgetMonthResponse>`
  - `GetBudgetCategoriesAsync(budgetId)` -> `QueryResponse<CategoryResponse>`
  - `GetBudgetAccountsAsync(budgetId)` -> `QueryResponse<AccountResponse>`
- `AddYnabApi(token)` configures named `HttpClient` (name: `BudgetApi`) with:
  - BaseAddress: `{YnabOptions.Endpoint}/{YnabOptions.Version}/` (currently `https://api.ynab.com/v1/`).
  - Header: `Authorization: Bearer {token}`.
  - Resilience: `.AddStandardResilienceHandler()` (Microsoft.Extensions.Http.Resilience).
- Error handling: `BudgetApi` catches exceptions and writes `ex.ToString()` to console, returning default/empty response. Callers should handle empty/missing Data.


## Query services (business logic)

- `BudgetQueryService`:
  - `GetBudgets()` fetches budgets and falls back to `[Budget.NoBudget]` if empty.
  - `GetBudgetMonth(budget, date)` clamps future year to current year; formats as `yyyy-MM-01`.
  - `GetCurrentMonthBudget(budget)` uses the YNAB API's "current" keyword to fetch the current month's budget data without creating a date object.
  - Category retrieval can be configured via `BudgetCategorySearchOptions`:
    - `SelectedBudget` (required), `CategoryFilter` (contains match), `ShowHiddenCategories`, `ShowDeletedCategories` (currently hidden/deleted are always filtered out in default path).
    - First category group is skipped (YNAB internal master category).
- `CategoryQueryService.GetBudgetCategoriesAsync(budget)` returns full groups collection from API.
- `AccountQueryService.GetBudgetAccounts(budget)` returns accounts or default `[new Account()]`.



## CLI command and settings

- `BudgetCommand` (Spectre.Console.Cli):
  - Arguments: `[budgetFilter]` (0), `[categoryFilter]` (1).
  - Options:
    - `-o|--open`: open budget in browser (cannot be combined with `--last-used`).
    - `-g|--show-goals`: render category progress charts (experimental formatting).
    - `-u|--last-used`: force "last-used" budget (ignores filter; cannot combine with `--open`).
    - `--api-token <token>`: YNAB API token; persisted to `config.ini` if provided.
  - `--hide-amounts`: hide all monetary amounts in output (see CurrencyFormatting).


## UI and rendering

- `YnacConsole` renders header, then either opens browser or prints a main table:
  - Caption shows budget name; metadata shows Age of Money and To Be Budgeted (converted to currency by dividing by 1000).
  - For each `CategoryGroup` (not hidden/deleted): builds a sub-table with columns Category, Budgeted, Activity, Available.
  - With `--show-goals`, category cell becomes a breakdown chart for `GoalPercentageComplete`.
- After render, the app loops, prompting user to pick an `IBudgetAction` (sorted by `Order`). 
- Actions are re-evaluated each loop, allowing dynamic DisplayName values.
- After executing an action, YnacConsole re-renders the budget table to reflect any state changes.
- Sample actions: `ToggleHideAmountsBudgetAction`, `ExitBudgetAction`.

CurrencyFormatting
- All displayed amounts pass through `IValueFormatter` which internally uses `ICurrencyFormatter` (see `ynac.cli/CurrencyFormatting`).
- DI provides `ICurrencyVisibilityState` (singleton) initialized from settings.HideAmounts.
- ValueFormatter queries this state at runtime via ICurrencyFormatterResolver to choose between `DefaultCurrencyFormatter` (uses `ToString("C")` with current culture) and `MaskedCurrencyFormatter` (masks values).
- The `HideAmounts` setting comes from CLI (`-h|--hide-amounts`) or `[Ynac] HideAmounts` in `config.ini` (path: `Constants.YnacHideAmountsConfigPath`).
- Runtime toggling: `ToggleHideAmountsBudgetAction` flips the visibility state during the session. This toggle is session-only and does not persist to config; CLI/config flags only set the initial state.
- See detailed guide: `ynac.cli/CurrencyFormatting/INSTRUCTIONS.md`.
- Future approvals/write actions: When implementing transaction approvals or other write operations, re-evaluate how `--hide-amounts` should behave. It may be unsafe to approve transactions without seeing amounts. Options include: temporarily disallowing approvals while hidden, prompting to disable `--hide-amounts` for that action, or a specialized confirmation flow that reveals only the affected amount with explicit consent.


## OS features

- Browser opening abstractions live under `ynac.cli/OSFeatures`:
  - `IBudgetBrowserOpener` interface; `BudgetBrowserOpener` composes an `IBrowserOpener` (platform-specific elsewhere in the same folder).
  - URL format: `{Constants.YnabRootUrl}{selectedBudget.Id}{Constants.BudgetRouteAffix}` (see Known limitations below).


## Dependency injection and setup

- `AddYnabApi(token)` registers:
  - Named `HttpClient` for API.
  - `IBudgetApi` (singleton), `IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService` (singletons).
- The `ynac.cli` project wires Spectre command and the console components in `YnacConsoleProvider` (see that file for service registrations).


## Testing

- Framework: MSTest (`ynac.tests`).
- Coverage: `TokenHandlerTests` validate config/token persistence behaviors across file states.
- Note: tests assume the working directory aligns with `AppContext.BaseDirectory` for `config.ini` placement.


## Error handling and resilience

- HTTP: Standard resilience handler provides retries/circuit breaker; API methods swallow exceptions and return defaults. Higher layers must treat missing/empty `Data` as a failure state.
- CLI validation: `BudgetCommand` prevents conflicting flags (`--open` with `--last-used`).


## Performance and caching

- `BudgetSelector` caches budgets on first fetch and does not refresh unless process restarts. This is intentional but called out to be improved (e.g., time-based cache or explicit refresh).


## Extensibility points

- Add a new user action:
  - Implement `IBudgetAction` with `DisplayName`, `Order`, and `Execute()`; register in DI; it will auto-appear in the action picker.
  - DisplayName can be dynamic (re-evaluated each menu loop) to reflect current state (e.g., ToggleHideAmountsBudgetAction).
  - After Execute() completes, YnacConsole re-renders the budget table, allowing actions to trigger immediate visual updates.
- Add a new command:
  - Create a Spectre `Command` + `CommandSettings` and register in the `CommandApp`.
- Add new API endpoints:
  - Extend `IBudgetApi` + `BudgetApi`, add models, and extend `YnabJsonSerializerContext` with `[JsonSerializable]` attributes for new types.
- Modify rendering:
  - Update `YnacConsole` table generation; prefer Spectre components (Table, Panel, BreakdownChart) and keep amounts in currency units (divide by 1000) consistently.


## Coding conventions

- C# 12/NET 9 patterns: primary constructors, file-scoped namespaces, records where sensible, `async/await` with `Task`.
- Models are immutable via `init;` where possible.
- Null-handling: prefer defaults to avoid NRE in rendering; validate required options.
- Serialization: System.Text.Json with source generation (`YnabJsonSerializerContext`). Keep it updated when adding new models.


## Known limitations and TODOs (as of last review)

- Browser opener URL for last-used budget: builds `…/{selectedBudget.Id}/budget`. For `LastUsedBudget` the Id is `Guid.Empty`. Likely should use `Budget.BudgetId` (which yields `"last-used"`) to support opening the last used budget.
- `YnacConsole` goal display is WIP and formatting may not match non-goal view.
- `BudgetSelector` cache prevents refresh; add explicit refresh or time-based cache in future.
- Error logging is basic (Console.WriteLine). Consider structured logging.
 - Currency: current default formatter is culture-sensitive; future multi-currency/localization can add a localized formatter or a masking decorator while preserving symbols/patterns.


## How to safely change behavior (AI checklist)

When implementing changes, follow this checklist:

1) Identify contracts
   - Inputs/outputs and units (milliunits vs currency) for any changed path.
   - JSON shapes and serializer context updates.
   - CLI flags interactions and defaults.

2) Update this document
   - Reflect new commands/options, models, services, known limitations.
   - Mark last reviewed date.

3) Validate end-to-end
   - Ensure config/token resolution still works (CLI option, ini file, prompt).
   - Render paths don’t throw on null/empty `Data`.
   - For new HTTP calls: add to `YnabJsonSerializerContext` and handle error/empty responses.

4) Tests and smoke checks
   - Add/extend MSTest tests for new logic (happy path + a failure/empty-data case).
   - Run a local smoke of `BudgetCommand` with and without `--last-used` and `--open`.

5) Backward compatibility
   - Preserve public behaviors of existing flags and output where possible; document changes here.


## Questions to resolve before larger changes

- Do we need write operations (approving/categorizing transactions)? If so, design auth/error strategy and add tests.
- Should token storage be more secure (keychain/OS store) instead of plain-text `ini`?
- What’s the desired refresh semantics for budgets/categories (cache TTL, manual refresh command)?
- Standardize currency/formatting helpers to remove repeated `/1000` conversions and formatting.
- Should we utilize the last knowledge id from the YNAB Api


## Glossary

- YNAB: You Need A Budget (the budgeting app). API docs: https://api.ynab.com/
- Spectre.Console: C# console UI library.
- Milliunits: monetary amounts in thousandths of currency unit used by YNAB API.


## Maintenance note

Always consider updating this AI_INSTRUCTIONS.md in any PR that:
- Changes commands/flags, rendering, API calls, models, or configuration behavior.
- Alters token or config handling.
- Adds/removes DI registrations or default behaviors.

Treat drifts in this document as a high-priority issue. If unsure, add a short “Notes” subsection here during the PR and refine later.
