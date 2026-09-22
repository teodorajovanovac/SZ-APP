import type { BankStatementSummary, JournalEntrySummary } from './types'

export function formatMoney(value: number, currency = 'RSD') {
  return new Intl.NumberFormat('sr-Latn-RS', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

export function canPostJournal(journal: JournalEntrySummary) {
  return !journal.isPosted
}

export function canPostStatement(statement: BankStatementSummary) {
  return statement.status === 'Ready'
}
