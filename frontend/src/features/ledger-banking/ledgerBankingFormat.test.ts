import { canPostJournal, canPostStatement, formatMoney } from './ledgerBankingFormat'
import type { BankStatementSummary, JournalEntrySummary } from './types'

describe('ledger banking presentation rules', () => {
  it('formats money with two decimals', () => {
    expect(formatMoney(1234.5)).toMatch(/1[.\s]234,50/)
  })

  it('enables only valid posting transitions', () => {
    expect(canPostJournal({ isPosted: false } as JournalEntrySummary)).toBe(true)
    expect(canPostJournal({ isPosted: true } as JournalEntrySummary)).toBe(false)
    expect(canPostStatement({ status: 'Ready' } as BankStatementSummary)).toBe(true)
    expect(canPostStatement({ status: 'Imported' } as BankStatementSummary)).toBe(false)
  })
})
