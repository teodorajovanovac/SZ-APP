import type { BankStatementSummary, JournalEntrySummary } from './types'

export { formatMoney } from '../../shared/format/money'

export function canPostJournal(journal: JournalEntrySummary) {
  return !journal.isPosted
}

export function canPostStatement(statement: BankStatementSummary) {
  return statement.status === 'Ready'
}
