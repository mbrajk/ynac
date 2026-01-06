# CurrencyFormatting: design and usage

This folder defines how monetary amounts are formatted for display in the console.

## Goals
- Centralize currency formatting behind a small interface so renderers do not call ToString("C") directly.
- Allow masking all amounts for privacy (screenshots, demos) via a toggle.
- Be extensible for future localization and currency rules.

## Components
- ICurrencyFormatter: single method `string Format(decimal amount)`.
- DefaultCurrencyFormatter: uses `amount.ToString("C")` (current culture) to format currency.
- MaskedCurrencyFormatter: masks amounts and returns a placeholder string (`***.**`).
- CurrencyFormatterResolver: resolves the active formatter at runtime based on the HideAmounts setting.
- ICurrencyVisibilityState: interface for runtime state that controls whether amounts are hidden or shown.
- CurrencyVisibilityState: implementation of ICurrencyVisibilityState that holds a simple boolean flag.
- ToggleableCurrencyFormatter: alternative formatter implementation that dynamically switches between default and masked formatters based on visibility state (not currently used but demonstrates the pattern).

## Selection (DI)
- YnacConsoleProvider registers DefaultCurrencyFormatter, MaskedCurrencyFormatter, and CurrencyFormatterResolver.
- YnacConsoleProvider also registers ICurrencyVisibilityState as a singleton, initialized from settings.HideAmounts.
- ValueFormatter internally uses ICurrencyVisibilityState to determine which formatter to use at call time.
- Code that needs a formatter obtains one via `ICurrencyFormatterResolver.Resolve(isMasked)`
- Initial masking state can be set via:
  - CLI flag: `-h|--hide-amounts`.
  - Config file: `[Ynac] HideAmounts = True|False`.
- The state can be toggled at runtime via the ToggleHideAmountsBudgetAction (see below).

## Runtime Toggling
- ToggleHideAmountsBudgetAction: a budget action that appears in the action menu.
  - DisplayName dynamically changes: "Hide amounts" when shown, "Show amounts" when hidden.
  - Order = 0 (appears at the top of the action menu).
  - Execute() flips the ICurrencyVisibilityState.Hidden boolean and displays a confirmation message.
- After toggling, YnacConsole re-renders the budget table, immediately reflecting the visibility change.
- The toggle persists only for the current session; CLI/config settings determine the initial state on the next run.

## Usage
- YnacConsole receives `ICurrencyFormatter` via DI and calls `Format(...)` for:
  - Category Budgeted / Activity / Available values.
  - Group totals.
  - "To Be Budgeted" header line.
- Amounts provided to the formatter are already converted from milliunits to currency units by dividing by 1000 upstream.

## Future-proofing
- Localization: introduce a `LocalizedCurrencyFormatter(CultureInfo culture)` or a strategy that reads culture currency settings from YNAB or YNAC config or OS.
- Masking strategy: consider a decorator approach to preserve symbols/patterns while masking digits, e.g.:
  - `new MaskingCurrencyFormatterDecorator(inner: LocalizedCurrencyFormatter)
     => returns masked numerals but keeps negative pattern and currency symbol.`
- Currency-specific rules: extend the interface or pass context if needed (e.g., currency code) once multi-currency support is added.
 - Approvals/write actions and masking: If/when approving transactions (or other write operations) is implemented, re-evaluate `--hide-amounts`. It may be risky to perform irreversible actions while amounts are hidden. Consider one of:
   1) Disallow approval while hidden and prompt to rerun without `--hide-amounts`.
   2) Temporarily reveal only the relevant amount with an explicit confirmation.
   3) Require an additional confirmation step summarizing the change in plain terms.

## Testing
- MSTest tests verify:
  - Default formatting matches en-US examples and is culture-sensitive.
  - Masked formatting returns the placeholder string for any input.

## Notes
- Keep the formatter selection isolated to DI so UI code stays simple.
- If you change the MaskedCurrencyFormatter placeholder string, update tests accordingly.
