## AI instructions: ynac project

This document orients AI tooling and contributors to the ynac codebase. Treat keeping this document accurate as high-priority technical debt. When you change public behavior, add features, or refactor structure, update this guide in the same PR.

Last reviewed: 2026-07-04


## Purpose and scope

- Goal: Console app (Spectre.Console) that displays and edits YNAB budget information via the YNAB REST API.
- Projects:
  - ynac.cli: CLI and UI (Spectre.Console), orchestration, config, OS helpers, commands, actions.
  - ynab: API client + query/command services + models + System.Text.Json source-gen context.
  - ynac.tests: MSTest tests (token/config handling, currency formatting, budget actions, ynab services and serialization).


## High-level architecture

- CLI entry: `ynac.cli/Program.cs` initializes config file then runs `BudgetCommand`.
- Command: `BudgetCommand` parses settings, resolves API token, builds DI container via `YnacConsoleProvider` (see `ynac.cli`), and dispatches to `IYnacConsole.RunAsync`.
- Console flow: `YnacConsole` shows header, selects budget (via `IBudgetSelector`), stores it in `IBudgetContext`, optional open-in-browser, loads current-month budget + categories, renders table, then prompts for actions (`IBudgetAction`). When an action sets `IBudgetContext.DataStale`, the console re-fetches month + categories before re-rendering.
- API layer: `ynab` project provides `IBudgetApi` implementation (`BudgetApi`) backed by `HttpClient` configured by `AddYnabApi(token)` extension. Covers user, budgets, months, categories (read + write), accounts, payees (read + write), transactions (full CRUD + bulk update + import) and scheduled transactions (full CRUD).
- Query services (reads): `IBudgetQueryService`, `ICategoryQueryService`, `IAccountQueryService`, `IPayeeQueryService`, `ITransactionQueryService`, `IScheduledTransactionQueryService`, `IUserQueryService`.
- Command services (writes): `ITransactionCommandService`, `ICategoryCommandService`, `IPayeeCommandService`, `IScheduledTransactionCommandService`.
- Models: `Budget`, `BudgetMonth`, `CategoryGroup`, `Category`, `Account`, `Payee`, `Transaction`, `ScheduledTransaction`, `User` plus `Save*` request payloads (see `ynab/*`) mirror YNAB API shapes (System.Text.Json).


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

- `BudgetApi` endpoints (all return `QueryResponse<T>`; members are `internal` — consume through query/command services):
  - Reads: `GetUserAsync`, `GetBudgetsAsync`, `GetBudgetMonthAsync(budgetId, month)`, `GetBudgetMonthsAsync`, `GetBudgetCategoriesAsync`, `GetBudgetAccountsAsync`, `GetBudgetPayeesAsync`, `GetTransactionsAsync(budgetId, sinceDate?, type?)`, `GetAccountTransactionsAsync`, `GetCategoryTransactionsAsync`, `GetPayeeTransactionsAsync`, `GetTransactionAsync`, `GetScheduledTransactionsAsync`.
  - Writes: `CreateTransactionAsync` (POST), `UpdateTransactionAsync` (PUT), `UpdateTransactionsAsync` (PATCH bulk), `DeleteTransactionAsync`, `ImportTransactionsAsync` (POST, pulls from linked accounts), `UpdateMonthCategoryAsync` (PATCH budgeted), `UpdateCategoryAsync` (PATCH name/note), `UpdatePayeeAsync` (PATCH name), `CreateScheduledTransactionAsync`, `UpdateScheduledTransactionAsync`, `DeleteScheduledTransactionAsync`.
  - `sinceDate` is `yyyy-MM-dd`; transaction `type` filter is `unapproved` or `uncategorized` (see `TransactionsFilter`).
- `AddYnabApi(token)` configures named `HttpClient` (name: `BudgetApi`) with:
  - BaseAddress: `{YnabOptions.Endpoint}/{YnabOptions.Version}/` (currently `https://api.ynab.com/v1/`).
  - Header: `Authorization: Bearer {token}`.
  - Resilience: `.AddStandardResilienceHandler()` (Microsoft.Extensions.Http.Resilience).
- Error handling: 401 → `YnabAuthenticationException`; other HTTP failures → `YnabApiException`. Write helpers call `EnsureSuccessStatusCode()` so failed writes surface as `YnabApiException` rather than silent defaults. Callers should still handle empty/missing `Data`.
- Rate limiting: YNAB allows 200 requests/hour per token. Fetch with intent — list endpoints accept `since_date`/`type` filters to keep payloads and request counts small. List responses expose `ServerKnowledge` for future delta-request support.


## Query services (business logic)

- `BudgetQueryService`:
  - `GetBudgets()` fetches budgets and falls back to `[Budget.NoBudget]` if empty.
  - `GetBudgetMonth(budget, date)` clamps future year to current year; formats as `yyyy-MM-01`.
  - `GetCurrentMonthBudget(budget)` uses the YNAB API's "current" keyword to fetch the current month's budget data without creating a date object.
  - Category retrieval can be configured via `BudgetCategorySearchOptions`:
    - `SelectedBudget` (required), `CategoryFilter` (contains match), `ShowHiddenCategories` (defaults to false, respects CLI flag), `ShowDeletedCategories` (deleted categories are always filtered out).
    - First category group is skipped (YNAB internal master category).
- `BudgetQueryService.GetBudgetMonths(budget)` returns non-deleted month summaries, newest first.
- `CategoryQueryService.GetBudgetCategoriesAsync(budget)` returns full groups collection from API.
- `AccountQueryService.GetBudgetAccounts(budget)` returns accounts or default `[new Account()]`.
- `PayeeQueryService.GetBudgetPayees(budget)` returns non-deleted payees.
- `TransactionQueryService` returns non-deleted transactions, newest first; overloads for budget-wide, per-account, per-category and per-payee, each accepting an optional `sinceDate` and (budget-wide) a `TransactionsFilter`.
- `ScheduledTransactionQueryService.GetScheduledTransactions(budget)` returns non-deleted scheduled transactions ordered by `DateNext`.
- `UserQueryService.GetAuthenticatedUserId()` returns the token owner's user id.

Command services (writes)

- `TransactionCommandService`: `CreateTransaction`, `UpdateTransaction`, `UpdateTransactions` (bulk PATCH; empty input short-circuits without an API call), `ApproveTransactions(ids)`, `CategorizeTransactions(id→category pairs)`, `DeleteTransaction`, `ImportLinkedAccountTransactions`.
- `CategoryCommandService`: `SetMonthCategoryBudgeted(budget, month, categoryId, budgetedMilliunits)` (`CategoryCommandService.CurrentMonth` = "current"), `UpdateCategory(name/note)`.
- `PayeeCommandService.RenamePayee`.
- `ScheduledTransactionCommandService`: `CreateScheduledTransaction`, `UpdateScheduledTransaction`, `DeleteScheduledTransaction`.
- Write payloads (`SaveTransaction`, `SaveScheduledTransaction`, `SaveCategory`, ...) use nullable properties; nulls are omitted from JSON (`WhenWritingNull`), which leaves those fields unchanged on update. Amounts in payloads are milliunits.



## CLI command and settings

- `BudgetCommand` (Spectre.Console.Cli):
  - Arguments: `[budgetFilter]` (0), `[categoryFilter]` (1).
  - Options:
    - `-o|--open`: open budget in browser (cannot be combined with `--last-used`).
    - `-g|--show-goals`: render category progress charts (experimental formatting).
    - `-u|--last-used`: force "last-used" budget (ignores filter; cannot combine with `--open`).
    - `--api-token <token>`: YNAB API token; persisted to `config.ini` if provided.
    - `--hide-amounts`: hide all monetary amounts in output (see CurrencyFormatting).
    - `--show-hidden-categories`: show hidden categories in budget view (by default they are filtered out).


## UI and rendering

- `YnacConsole` renders header, then either opens browser or prints a main table:
  - Caption shows budget name; metadata shows Age of Money and To Be Budgeted (converted to currency by dividing by 1000).
  - For each `CategoryGroup` (not hidden/deleted): builds a sub-table with columns Category, Budgeted, Activity, Available.
  - With `--show-goals`, category cell becomes a breakdown chart for `GoalPercentageComplete`.
- After render, the app loops, prompting user to pick an `IBudgetAction` (sorted by `Order`). 
- Actions are re-evaluated each loop, allowing dynamic DisplayName values.
- After executing an action, YnacConsole re-renders the budget table. If the action set `IBudgetContext.DataStale`, the month and categories are re-fetched first.
- Registered actions: `ToggleHideAmountsBudgetAction` (0), `ListTransactionsBudgetAction` (1), `ApproveTransactionsBudgetAction` (2), `CategorizeTransactionsBudgetAction` (3), `AddTransactionBudgetAction` (4), `EditCategoryBudgetedBudgetAction` (5), `ViewAccountsBudgetAction` (6), `ViewScheduledTransactionsBudgetAction` (7), `ExitBudgetAction` (last).
- View-style actions render their own tables and block on "press enter to continue" (`BudgetActionHelpers.WaitForEnter`) so output can be read before the budget re-renders.
- Write-style actions always confirm before calling the API (`ConfirmationPrompt`).

CurrencyFormatting
- All displayed amounts pass through `IValueFormatter` which internally uses `ICurrencyFormatter` (see `ynac.cli/CurrencyFormatting`).
- DI provides `ICurrencyVisibilityState` (singleton) initialized from settings.HideAmounts.
- ValueFormatter queries this state at runtime via ICurrencyFormatterResolver to choose between `DefaultCurrencyFormatter` (uses `ToString("C")` with current culture) and `MaskedCurrencyFormatter` (masks values).
- The `HideAmounts` setting comes from CLI (`-h|--hide-amounts`) or `[Ynac] HideAmounts` in `config.ini` (path: `Constants.YnacHideAmountsConfigPath`).
- Runtime toggling: `ToggleHideAmountsBudgetAction` flips the visibility state during the session. This toggle is session-only and does not persist to config; CLI/config flags only set the initial state.
- See detailed guide: `ynac.cli/CurrencyFormatting/INSTRUCTIONS.md`.
- Write actions and hidden amounts: acting on amounts the user cannot see is risky, so write actions run through `BudgetActionHelpers.RunWithAmountsRevealOffer`, which offers to reveal amounts for the duration of the action and restores the hidden state afterwards.


## OS features

- Browser opening abstractions live under `ynac.cli/OSFeatures`:
  - `IBudgetBrowserOpener` interface; `BudgetBrowserOpener` composes an `IBrowserOpener` (platform-specific elsewhere in the same folder).
  - URL format: `{Constants.YnabRootUrl}{selectedBudget.Id}{Constants.BudgetRouteAffix}` (see Known limitations below).


## Dependency injection and setup

- `AddYnabApi(token)` registers:
  - Named `HttpClient` for API.
  - `IBudgetApi` plus all query and command services listed above (all singletons).
- The `ynac.cli` project wires Spectre command and the console components in `YnacConsoleProvider` (see that file for service registrations), including `IBudgetContext` (shared session state) and every `IBudgetAction`.


## Testing

- Framework: MSTest (`ynac.tests`) with FluentAssertions and NSubstitute.
- Coverage: `TokenHandlerTests` (config/token persistence), currency formatting, budget actions, and `Ynab/*` tests for query/command services and serializer behavior (null-omission on writes, API-shape deserialization).
- The `ynab` project exposes internals to `ynac.tests` and to `DynamicProxyGenAssembly2` so `IBudgetApi` (internal members) can be substituted in service tests.
- Note: tests assume the working directory aligns with `AppContext.BaseDirectory` for `config.ini` placement.


## Error handling and resilience

- HTTP: Standard resilience handler provides retries/circuit breaker; API methods swallow exceptions and return defaults. Higher layers must treat missing/empty `Data` as a failure state.
- CLI validation: `BudgetCommand` prevents conflicting flags (`--open` with `--last-used`).


## Performance and caching

- `BudgetSelector` caches budgets on first fetch and does not refresh unless process restarts. This is intentional but called out to be improved (e.g., time-based cache or explicit refresh).


## Extensibility points

- Add a new user action:
  - Implement `IBudgetAction` with `DisplayName`, `Order`, and `ExecuteAsync()`; register in DI; it will auto-appear in the action picker.
  - Inject `IBudgetContext` to access the selected budget; set `DataStale = true` if the action changed data on the server so the console re-fetches before re-rendering.
  - DisplayName can be dynamic (re-evaluated each menu loop) to reflect current state (e.g., ToggleHideAmountsBudgetAction).
  - After ExecuteAsync() completes, YnacConsole re-renders the budget table, allowing actions to trigger immediate visual updates.
  - Wrap write flows in `BudgetActionHelpers.RunWithAmountsRevealOffer` and confirm before calling the API.
- Add a new command:
  - Create a Spectre `Command` + `CommandSettings` and register in the `CommandApp`.
- Add new API endpoints:
  - Extend `IBudgetApi` + `BudgetApi`, add models, and extend `YnabJsonSerializerContext` with `[JsonSerializable]` attributes for new types.
- Modify rendering:
  - Update `YnacConsole` table generation; prefer Spectre components (Table, Panel, BreakdownChart) and keep amounts in currency units (divide by 1000) consistently.


## Coding conventions

- C# 13/.NET 10 patterns: primary constructors, file-scoped namespaces, records where sensible, `async/await` with `Task`.
- Models are immutable via `init;` where possible.
- Null-handling: prefer defaults to avoid NRE in rendering; validate required options.
- Serialization: System.Text.Json with source generation (`YnabJsonSerializerContext`). Keep it updated when adding new models.


## Known limitations and TODOs (as of last review)

- Browser opener URL for last-used budget: builds `…/{selectedBudget.Id}/budget`. For `LastUsedBudget` the Id is `Guid.Empty`. Likely should use `Budget.BudgetId` (which yields `"last-used"`) to support opening the last used budget.
- `YnacConsole` goal display is WIP and formatting may not match non-goal view.
- `BudgetSelector` cache prevents refresh; add explicit refresh or time-based cache in future.
- Error logging is basic (Console.WriteLine). Consider structured logging.
- Currency: current default formatter is culture-sensitive; future multi-currency/localization can add a localized formatter or a masking decorator while preserving symbols/patterns.
- Wrapper features not yet surfaced in the UI: payee rename, scheduled transaction create/update/delete, transaction import, single-transaction get/update/delete, per-account/category/payee transaction views, month list.
- Split (sub)transactions render as their parent only; creating splits is not supported.
- Trimming: `Spectre.Console.Cli` produces IL2104 (third-party trim warnings); required Spectre.Cli internals are preserved via `[DynamicDependency]` in `Program.cs`. Full Native AOT (`PublishAot`) is not supported upstream by Spectre.Console.Cli; the supported shape is trimmed self-contained publish, and the `ynab` project is fully AOT-safe (source-generated JSON only).


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

- Should token storage be more secure (keychain/OS store) instead of plain-text `ini`?
- What’s the desired refresh semantics for budgets/categories (cache TTL, manual refresh command)?
- Standardize currency/formatting helpers to remove repeated `/1000` conversions and formatting.
- Should we utilize `server_knowledge` for delta requests? The wrapper already surfaces it on list responses; the query services do not yet accept a `last_knowledge_of_server` parameter.
- Which wrapper features should be surfaced in the UI next: payee rename, scheduled transaction create/edit/delete, transaction import, per-account/category/payee transaction views?


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
