/** Serbian money formatting: comma decimal separator, grouped thousands (see CLAUDE.md). */
export function formatMoney(value: number, currency = 'RSD') {
  return new Intl.NumberFormat('sr-Latn-RS', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}
