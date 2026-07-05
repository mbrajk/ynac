# TUI Migration Plan

This document outlines the plan for migrating ynac from a sequential Spectre.Console output app to a fully interactive Terminal User Interface.

## Current State

The app currently works as a linear flow: display header → select budget → fetch data → render table → prompt for action → re-render → repeat. The "interactivity" is a blocking `SelectionPrompt` at the bottom of a wall of printed output. Every action triggers a full clear-and-reprint of the entire budget table. There is no persistent UI, no keyboard navigation, and no way to browse data without re-rendering everything.

## Framework Choice: Terminal.Gui v2

**Terminal.Gui** is the only viable .NET TUI framework. Spectre.Console is a rich output library, not an interactive TUI toolkit — it has no event loop, no focus management, and no persistent widget layout.

- **Package:** `Terminal.Gui` v2 pre-release (develop builds target net10.0)
- **Paradigm:** Widget-based, event-driven (similar to WinForms for the terminal)
- **Widgets:** 55+ built-in views including `TableView`, `ListView`, `TreeView`, `TabView`, `MenuBar`, `StatusBar`, `Dialog`
- **Layout:** Relative `Pos`/`Dim` system (`Pos.Center()`, `Dim.Fill()`, `Dim.Percent()`, etc.)
- **Input:** Command pattern with declarative `KeyBindings`, mouse support
- **Async:** Works with async/await via `Application.Invoke()` for UI thread marshalling

**Coexistence with Spectre.Console:** Keep `Spectre.Console.Cli` for command-line argument parsing (the `BudgetCommand` / `BudgetCommandSettings` layer). Replace all Spectre.Console rendering and prompts with Terminal.Gui views. The CLI parser runs before the TUI starts, so there is no conflict.

**Risk:** Terminal.Gui v2 has no stable release yet (alpha/develop builds only). The API is considered stable, but pin the package version in `Directory.Packages.props` to avoid surprise breakage.

---

## Target Architecture

### Layering

```
CLI Parsing (Spectre.Console.Cli — keep as-is)
    │
    ▼
TUI Application Shell (Terminal.Gui Application + Toplevel)
    │
    ├── Views (Terminal.Gui Views — new)
    │     ├── BudgetSelectionView
    │     ├── BudgetDashboardView
    │     ├── CategoryDetailView
    │     ├── TransactionListView (Phase 6)
    │     ├── AccountListView (Phase 8)
    │     ├── ScheduledTransactionListView (Phase 8)
    │     └── SettingsView (future)
    │
    ├── ViewModels (new layer — plain C# classes, no framework dependency)
    │     ├── BudgetSelectionViewModel
    │     ├── BudgetDashboardViewModel
    │     ├── CategoryDetailViewModel
    │     ├── TransactionListViewModel (Phase 6)
    │     ├── TransactionEditViewModel (Phase 7)
    │     ├── AccountListViewModel (Phase 8)
    │     └── ScheduledTransactionListViewModel (Phase 8)
    │
    ├── Services (existing — keep as-is)
    │     ├── Query services (IBudgetQueryService, ICategoryQueryService, IAccountQueryService,
    │     │   IPayeeQueryService, ITransactionQueryService, IScheduledTransactionQueryService)
    │     ├── Command services (ITransactionCommandService, ICategoryCommandService,
    │     │   IPayeeCommandService, IScheduledTransactionCommandService)
    │     └── IValueFormatter
    │
    └── API Layer (existing — keep as-is)
          ├── IBudgetApi / BudgetApi
          └── Models
```

### Key Design Decisions

1. **Introduce ViewModels.** The current code mixes data fetching, formatting, and Spectre.Console rendering in `YnacConsole.cs`. Split this: ViewModels own the data and state, Views own the Terminal.Gui widgets. This makes the business logic testable without any TUI framework dependency.

2. **Replace `IAnsiConsoleService` with Terminal.Gui views.** The current `AnsiConsoleService` wraps Spectre.Console's `AnsiConsole`. It gets replaced entirely — Terminal.Gui views handle their own rendering.

3. **Replace `IBudgetAction` with keybinding commands.** The current action system is a menu prompt at the bottom of output. In the TUI, actions become keyboard shortcuts and menu bar items that operate on the current view. The `IBudgetAction` interface can be preserved internally but actions are triggered by key bindings rather than a selection prompt.

4. **Keep the ynab project completely untouched.** The API layer, models, query services, and serialization context require zero changes. The TUI migration is entirely within `ynac.cli`.

5. **Keep `Spectre.Console.Cli` for argument parsing.** `BudgetCommand` and `BudgetCommandSettings` continue to parse CLI arguments. After parsing, instead of calling `YnacConsole.RunAsync()`, launch the Terminal.Gui application.

---

## Screen Designs

### Screen 1: Budget Selection

Shown when the app starts without `--last-used` and the budget filter matches multiple budgets.

```
┌─ You Need A Console ──────────────────────────────────┐
│                                                        │
│   Select a budget:                                     │
│                                                        │
│   ┌──────────────────────────────────────────────┐     │
│   │ > My Budget                                  │     │
│   │   Joint Budget                               │     │
│   │   Savings Tracker                            │     │
│   └──────────────────────────────────────────────┘     │
│                                                        │
│   Filter: [________________]                           │
│                                                        │
└────────────────────────────────────────────────────────┘
 Enter: Select  |  /: Filter  |  q: Quit
```

**Terminal.Gui components:** `ListView` for budget list, `TextField` for filter, `StatusBar` for key hints.

### Screen 2: Budget Dashboard (main view)

The primary screen. Shows the budget table with category groups, a summary header, and a status bar.

```
┌─ [ My Budget ] ───────────────────────────────────────────────────┐
│  Age of Money: 45 days          To Be Budgeted: $1,234.56        │
├───────────────────────────────────────────────────────────────────┤
│ ▼ Immediate Obligations                                          │
│   Category              Budgeted      Activity     Available     │
│   ─────────────────────────────────────────────────────────────  │
│ > Rent                  $1,500.00    -$1,500.00       $0.00      │
│   Electric               $150.00      -$120.30      $29.70      │
│   Internet                $70.00       -$70.00       $0.00      │
│   Groceries              $600.00      -$423.12     $176.88      │
│   ─────────────────────────────────────────────────────────────  │
│   Total                $2,320.00    -$2,113.42     $206.58      │
│                                                                   │
│ ▶ Subscriptions (collapsed)                                      │
│                                                                   │
│ ▼ Quality of Life                                                │
│   Category              Budgeted      Activity     Available     │
│   ─────────────────────────────────────────────────────────────  │
│   Dining Out             $200.00      -$145.00      $55.00      │
│   Entertainment          $100.00       -$30.00      $70.00      │
│   ─────────────────────────────────────────────────────────────  │
│   Total                  $300.00      -$175.00     $125.00      │
└───────────────────────────────────────────────────────────────────┘
 ↑↓: Navigate  |  Enter: Expand/Collapse  |  h: Hide amounts  |  /: Filter  |  b: Budgets  |  q: Quit
```

**Terminal.Gui components:** `TreeView` for collapsible category groups (or `TableView` with group row handling), `FrameView` for the outer border, `Label` for header stats, `StatusBar` for keybindings.

**Key behaviors:**
- Arrow keys navigate between categories
- Enter expands/collapses a category group
- `h` toggles amount visibility (replaces `ToggleHideAmountsBudgetAction`)
- `/` opens a filter input to search categories by name
- `b` returns to budget selection
- `g` toggles goal progress display
- `q` quits
- The view updates in-place — no full redraws

### Screen 3: Category Detail (drill-down)

Shown when the user presses Enter on a specific category. This is a new capability not possible in the current app.

```
┌─ Groceries ───────────────────────────────────────────┐
│                                                        │
│   Budgeted:    $600.00                                 │
│   Activity:   -$423.12                                 │
│   Available:   $176.88                                 │
│                                                        │
│   Goal: Monthly target $600.00                         │
│   Progress: ████████████████████░░░░  80%              │
│                                                        │
│   Note: Weekly grocery budget for family of 4          │
│                                                        │
└────────────────────────────────────────────────────────┘
 Esc: Back  |  h: Hide amounts
```

**Terminal.Gui components:** `Dialog` or `FrameView` overlay, `ProgressBar` for goal, `Label` for fields.

---

## Implementation Phases

### Phase 0: Preparation (no UI changes)

**Goal:** Set up the foundation without breaking anything.

1. **Add Terminal.Gui package.**
   - Add `Terminal.Gui` v2 develop build to `Directory.Packages.props` with a pinned version.
   - Verify it builds alongside existing Spectre.Console packages.

2. **Create the ViewModel layer.**
   - `BudgetSelectionViewModel`: holds budget list, selected budget, filter text. Exposes `LoadBudgetsAsync()`, `FilteredBudgets` property, `SelectBudget(Budget)`.
   - `BudgetDashboardViewModel`: holds `BudgetMonth`, `IReadOnlyList<CategoryGroup>`, visibility state, selected category. Exposes `LoadAsync(Budget, BudgetCommandSettings)`, `ToggleAmountVisibility()`, `FilterCategories(string)`, computed properties for formatted header values.
   - Extract the data-fetching and formatting logic from `YnacConsole.cs` into these ViewModels.
   - These classes depend only on the existing service interfaces (`IBudgetQueryService`, `IValueFormatter`, etc.) — no Terminal.Gui dependency.

3. **Write tests for ViewModels.**
   - Test data loading, filtering, visibility toggling, edge cases (empty budgets, no categories, null data).
   - This directly addresses the test coverage gap identified in the codebase — the logic currently buried in `YnacConsole` becomes testable.

**Files changed:** New files only. No existing code modified.

### Phase 1: Minimal TUI Shell

**Goal:** Replace the Spectre.Console output with a Terminal.Gui application that displays the same data.

4. **Create `TuiApp` class** (replaces `YnacConsole` as the UI entry point).
   - Initialize `Application.Init()`
   - Create `Toplevel` with `MenuBar` and `StatusBar`
   - `MenuBar`: File → Quit, View → Toggle Amounts / Toggle Goals / Show Hidden Categories
   - `StatusBar`: Show key shortcuts for current context

5. **Create `BudgetDashboardView`.**
   - Use a `TreeView` (or `TableView`) to render category groups and categories.
   - Category groups are top-level nodes (collapsible).
   - Categories are child nodes with columns: Name, Budgeted, Activity, Available.
   - Header area: budget name, age of money, to-be-budgeted amount.
   - Wire up `BudgetDashboardViewModel` for data.

6. **Wire into existing entry point.**
   - In `BudgetCommand.ExecuteAsync()`, after token resolution and DI setup, call `TuiApp.Run()` instead of `YnacConsole.RunAsync()`.
   - Register `TuiApp` and views in DI via `YnacConsoleProvider`.

7. **Handle `--open` flag.** If `--open` is passed, skip the TUI entirely and open the browser (same behavior as today).

**Result:** The app starts as a persistent TUI showing the budget dashboard. No more printed-and-scrolled output.

### Phase 2: Budget Selection View

**Goal:** Replace the Spectre.Console `SelectionPrompt` for budget picking.

8. **Create `BudgetSelectionView`.**
   - `ListView` bound to `BudgetSelectionViewModel.FilteredBudgets`.
   - `TextField` for live filtering (type to narrow the list).
   - Enter to select, Escape to quit.

9. **Navigation flow.**
   - If `--last-used` is set, skip selection and go straight to dashboard.
   - If budget filter from CLI matches exactly one budget, skip selection.
   - Otherwise, show `BudgetSelectionView` first, then transition to `BudgetDashboardView`.
   - `b` key from dashboard returns to budget selection.

### Phase 3: Keyboard Navigation and Interactions

**Goal:** Make the budget dashboard fully interactive.

10. **Category group expand/collapse.**
    - TreeView nodes for category groups toggle on Enter.
    - Visual indicator: `▼` expanded, `▶` collapsed.
    - Remember expand/collapse state during the session.

11. **Keybinding system.**
    - `h` — toggle amount visibility (calls `ViewModel.ToggleAmountVisibility()`, view refreshes).
    - `g` — toggle goal display.
    - `/` — focus the filter field (filter categories by name, live-updating the tree).
    - `Escape` — clear filter / back to previous view.
    - `q` — quit.
    - `b` — return to budget selection.
    - `r` — refresh data from API.

12. **Amount visibility toggle.**
    - Reuse existing `ICurrencyVisibilityState` and `IValueFormatter` — the ViewModel calls the same formatting pipeline.
    - View re-reads formatted values from ViewModel and updates cells.

### Phase 4: Category Detail View

**Goal:** Add drill-down capability (new feature, not possible in current app).

13. **Create `CategoryDetailView`.**
    - `Dialog` or overlay `FrameView` shown when Enter is pressed on a category row.
    - Shows: budgeted, activity, available, goal info, goal progress bar, note.
    - `ProgressBar` widget for goal completion percentage.
    - Escape to dismiss and return to dashboard.

14. **Wire `CategoryDetailViewModel`.**
    - Receives the selected `Category` from `BudgetDashboardViewModel`.
    - Exposes formatted values and goal percentage.

### Phase 5: Polish and Extended Features

**Goal:** Quality-of-life improvements that make the TUI feel complete.

15. **Color theming.**
    - Define a color scheme: green for available/budgeted, red for activity/negative, yellow for budget name, aqua for headers (matching current Spectre output).
    - Apply via Terminal.Gui's `ColorScheme` system so it's consistent across all views.

16. **Loading indicators.**
    - Show a `SpinnerView` or status message while API calls are in flight.
    - Use `Application.Invoke()` to update UI from async data loading.

17. **Error handling in the TUI.**
    - API failures show a `Dialog` with the error message instead of crashing.
    - Auth failures show a dialog with token troubleshooting steps (same content as current `BudgetCommand` catch blocks, but rendered in-TUI).

18. **Responsive layout.**
    - Use `Dim.Fill()` and `Dim.Percent()` so the layout adapts to terminal size.
    - Handle terminal resize events (Terminal.Gui does this automatically).

19. **Persistent settings.**
    - Save collapse state, last-selected budget, and amount visibility preference to `config.ini` so they persist across sessions.

### Phase 6: Transaction Views (read)

**Goal:** Surface the transaction features that exist today as interactive actions (`ListTransactionsBudgetAction`) as a first-class register view.

20. **Create `TransactionListView` + `TransactionListViewModel`.**
    - `TableView` with columns: Date, Account, Payee, Category, Memo, Amount, Cleared, Approved (same columns as `BudgetActionHelpers.BuildTransactionTable` today).
    - Data via `ITransactionQueryService.GetTransactions(budget, sinceDate, filter)`; default window last 30 days, `[`/`]` widen/narrow the window (7/30/90/all).
    - Entry points:
      - `t` from the dashboard — all recent transactions.
      - Enter on a category row → per-category register (`GetCategoryTransactions`).
      - Enter on an account row (Phase 8) → per-account register (`GetAccountTransactions`).
    - Filter row toggles: `u` unapproved only, `c` uncategorized only (maps to `TransactionsFilter`).
    - Respect the YNAB rate limit: fetch on view entry and explicit `r` refresh only, never per-keystroke.

### Phase 7: Write Operations

**Goal:** Move the write actions (approve, categorize, add, edit budgeted) from prompt sequences into in-view editing. All writes already exist in the command services — this phase is purely UI.

21. **Approve in the transaction list.** `Space` marks/unmarks rows, `a` approves marked rows (or the current row) after a confirm `Dialog`; call `ITransactionCommandService.ApproveTransactions`. Approved rows update in place — no full reload.
22. **Categorize in the transaction list.** `C` on a row (or marked rows) opens a searchable category picker `Dialog` (flattened "Group: Category" list, same shape as `CategoryChoice`); call `CategorizeTransactions` with the accumulated assignments in ONE bulk PATCH.
23. **Add transaction.** `n` opens a form `Dialog` (account dropdown, payee text with autocomplete from `IPayeeQueryService`, category picker, amount, date, memo, cleared checkbox); calls `CreateTransaction`. Reuse the milliunit conversion rules from `AddTransactionBudgetAction` (dollars × 1000, outflow negative).
24. **Edit budgeted inline on the dashboard.** `e` on a category row turns the Budgeted cell into an edit field; on commit call `ICategoryCommandService.SetMonthCategoryBudgeted(budget, "current", ...)` and update the row + To-Be-Budgeted header from the response (the PATCH returns the updated category — no extra GET needed).
25. **Data staleness.** Replace the `IBudgetContext.DataStale` flag with targeted ViewModel updates: writes return the updated entities, so views patch their own state. A full `r` refresh remains available.
26. **Hidden-amounts safety.** Preserve the `RunWithAmountsRevealOffer` behavior: if amounts are masked, any write flow first shows a confirm dialog offering to reveal amounts for that dialog only.

### Phase 8: Accounts, Scheduled, and Remaining Wrapper Features

**Goal:** Complete feature parity with the API wrapper.

27. **`AccountListView`** (`A` from dashboard): on-budget and tracking sections with balance/cleared/uncleared/last-reconciled columns (today's `ViewAccountsBudgetAction`). Enter on a row → per-account transaction register.
28. **`ScheduledTransactionListView`** (`S` from dashboard): upcoming scheduled transactions ordered by `DateNext` (today's `ViewScheduledTransactionsBudgetAction`). Add create (`n`), edit (`e`), delete (`d`) via form dialogs — the command service (`IScheduledTransactionCommandService`) already supports full CRUD.
29. **Payee management.** In the transaction list, `P` on a row opens payee rename (`IPayeeCommandService.RenamePayee`) — useful for cleaning up imported payee noise.
30. **Import trigger.** `i` from the transaction list calls `ITransactionCommandService.ImportLinkedAccountTransactions` and reports how many transactions were pulled in, then refreshes the unapproved filter view (import → approve is the natural workflow).
31. **Month navigation.** `←`/`→` on the dashboard move between months using `IBudgetQueryService.GetBudgetMonth(budget, date)` / `GetBudgetMonths`; month-category edits target the shown month instead of "current".

---

## What to Keep, Replace, and Add

| Component | Action | Notes |
|---|---|---|
| `Spectre.Console.Cli` (BudgetCommand, BudgetCommandSettings) | **Keep** | CLI parsing stays, runs before TUI |
| `YnacConsole` | **Replace** | Replaced by `TuiApp` + views |
| `IAnsiConsoleService` / `AnsiConsoleService` | **Replace** | No longer needed — Terminal.Gui views handle rendering |
| `IBudgetPrompter` / `BudgetPrompter` | **Replace** | Replaced by `BudgetSelectionView` |
| `PrompterBase` | **Remove** | Spectre.Console-specific abstraction |
| `IBudgetSelector` / `BudgetSelector` | **Keep** | Selection logic (caching, GUID matching) is reusable — just remove the prompt call |
| `IBudgetAction` / implementations | **Rework** | Keep the interface concept but wire to keybindings instead of a selection prompt |
| `ExitBudgetAction` | **Remove** | Quit is a keybinding (`q`) handled by TUI shell |
| `ToggleHideAmountsBudgetAction` | **Rework** | Becomes a keybinding handler calling `ViewModel.ToggleAmountVisibility()` |
| `ListTransactionsBudgetAction` | **Rework** | Becomes `TransactionListView` (Phase 6) |
| `ApproveTransactionsBudgetAction` / `CategorizeTransactionsBudgetAction` | **Rework** | Become in-view operations on `TransactionListView` (Phase 7) |
| `AddTransactionBudgetAction` / `EditCategoryBudgetedBudgetAction` | **Rework** | Become form dialog / inline edit (Phase 7) |
| `ViewAccountsBudgetAction` / `ViewScheduledTransactionsBudgetAction` | **Rework** | Become `AccountListView` / `ScheduledTransactionListView` (Phase 8) |
| `BudgetActionHelpers.RunWithAmountsRevealOffer` | **Rework** | Same policy, rendered as a confirm dialog before write flows |
| `IBudgetContext` | **Replace** | ViewModels own selected-budget state; `DataStale` becomes targeted view updates |
| `IValueFormatter` / currency formatting stack | **Keep** | Unchanged — ViewModels call `IValueFormatter.Format()` |
| `ICurrencyVisibilityState` | **Keep** | Unchanged — toggled by keybinding handler |
| `TokenHandler` | **Keep** | Token resolution unchanged |
| `Constants` | **Keep** | Config paths unchanged |
| `IBudgetBrowserOpener` / OS features | **Keep** | `--open` flag still works |
| Entire `ynab` project | **Keep** | Zero changes to API layer |

## New Files to Create

```
ynac.cli/
  Tui/
    TuiApp.cs                         — Application shell, Toplevel, MenuBar, StatusBar
    Views/
      BudgetSelectionView.cs          — ListView + filter for budget picking
      BudgetDashboardView.cs          — TreeView/TableView for category groups
      CategoryDetailView.cs           — Overlay dialog for category drill-down
    ViewModels/
      BudgetSelectionViewModel.cs     — Budget list state, filtering
      BudgetDashboardViewModel.cs     — Budget data, category state, formatting
      CategoryDetailViewModel.cs      — Single category detail state
```

## Migration Risk Mitigation

1. **Terminal.Gui v2 instability.** Pin the exact package version. If a build introduces a regression, the pinned version protects you. Monitor the Terminal.Gui GitHub for the v2 beta/stable release and upgrade when available.

2. **System.Text.Json version conflict.** Terminal.Gui v2 constrains `System.Text.Json >= 8.0.4 && < 9.0.0`. If .NET 10 packages pull in a newer version, this will cause a dependency conflict. Mitigation: check for this during Phase 0 and file an issue upstream if it occurs. The develop builds may have already resolved this for net10.0 targets.

3. **Preserve the non-TUI path.** Keep `--open` working as a non-interactive path. Consider keeping a `--no-tui` flag that falls back to the current Spectre.Console output for CI/scripting use cases where a persistent TUI is inappropriate.

4. **Incremental delivery.** Each phase produces a working application. Phase 1 alone is a usable (if minimal) TUI. Phases can be shipped independently.

## Testing Strategy

- **ViewModels are fully unit-testable** — they depend only on service interfaces, no TUI framework types. This is the primary testing surface.
- **Views are tested manually** — Terminal.Gui views are difficult to unit test. Focus manual testing on: keyboard navigation, resize behavior, color rendering across platforms (Windows Terminal, iTerm2, Linux terminals).
- **Keep existing tests passing** — the ynab project and formatting tests are unaffected. `BudgetSelector` tests may need updates if the prompt interface changes.
- **Add ViewModel tests for every phase** — each new ViewModel should ship with tests covering data loading, filtering, state transitions, and edge cases (empty data, API failures).

## Definition of Done

The TUI migration is complete when:

- The app starts as a persistent, interactive terminal application
- Users can navigate budgets, category groups, and categories with keyboard
- All current CLI flags continue to work (`--open`, `--last-used`, `--hide-amounts`, `--show-goals`, `--show-hidden-categories`, `--api-token`)
- Amount visibility can be toggled with a keypress
- Category groups can be expanded/collapsed
- Categories can be filtered by typing
- The app handles API errors gracefully in-TUI (no raw stack traces)
- The app adapts to terminal resize
- All existing tests pass, new ViewModel tests added
- Everything the interactive action menu can do today works in the TUI: list/approve/categorize/add transactions, edit budgeted amounts, view accounts and scheduled transactions
- Wrapper features without a UI today are surfaced: payee rename, scheduled transaction create/edit/delete, linked-account import, per-account/category transaction registers, month navigation
- Write operations confirm before calling the API and respect the hidden-amounts reveal policy
