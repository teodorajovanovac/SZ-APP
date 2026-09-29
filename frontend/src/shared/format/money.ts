/** Serbian money formatting: comma decimal separator, grouped thousands (see CLAUDE.md). */
export function formatMoney(value: number, currency = 'RSD') {
  return new Intl.NumberFormat('sr-Latn-RS', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

/** Serbian plain-number formatting (grouped thousands, comma decimal) for non-money numeric cells. */
export function formatNumber(value: number) {
  return new Intl.NumberFormat('sr-Latn-RS', { maximumFractionDigits: 2 }).format(value)
}

/** Ledger amount without currency symbol, always 2 decimals (card / balance columns). */
export function formatAmount(value: number) {
  return new Intl.NumberFormat('sr-Latn-RS', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value)
}
