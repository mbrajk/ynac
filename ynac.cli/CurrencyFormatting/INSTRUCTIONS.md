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

## Selection (DI)
- YnacConsoleProvider registers DefaultCurrencyFormatter, MaskedCurrencyFormatter, and CurrencyFormatterResolver.
- Code that needs a formatter obtains one via `ICurrencyFormatterResolver.Resolve(isMasked)`
- Masking can be set via:
  - CLI flag: `-h|--hide-amounts`.
  - Config file: `[Ynac] HideAmounts = True|False`.

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
