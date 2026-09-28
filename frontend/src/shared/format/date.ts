/** Serbian date formatting: d.m.yyyy (see CLAUDE.md). */
export function formatDate(value: string | Date | null | undefined): string {
  if (!value) return ''
  const date = typeof value === 'string' ? new Date(value) : value
  if (Number.isNaN(date.getTime())) return typeof value === 'string' ? value : ''
  return date.toLocaleDateString('sr-Latn-RS')
}
