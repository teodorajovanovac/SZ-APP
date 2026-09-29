import { apiRequest } from '../../api/generated/client'
import type { PageResponse } from '../ledger-banking/types'

export type CardGrouping = 'none' | 'paymentReference' | 'document' | 'openItems'

export interface LedgerCardRow {
  ledgerEntryId: number | null
  journalEntryId: number | null
  postingDate: string
  account: string
  partnerAccountId: number | null
  subAccountId: string | null
  documentRef: string | null
  description: string | null
  lineType: number | null
  dueDate: string | null
  paymentReference: string | null
  debit: number
  credit: number
  balance: number
  groupKey: string | null
  lineCount: number
}

export interface LedgerCard extends PageResponse<LedgerCardRow> {
  openingBalance: number
  totalDebit: number
  totalCredit: number
  closingBalance: number
}

export interface PartnerBalance {
  partnerAccountId: number
  partnerId: number
  partnerName: string
  accountNumber: number
  account: string
  debit: number
  credit: number
  balance: number
  overdue: number
  lastPaymentDate: string | null
}

export interface SearchResult {
  type: 'partner' | 'unit' | 'payment'
  title: string
  subtitle: string | null
  companyId: number
  partnerAccountId: number | null
  account: string | null
  unitId: number | null
  paymentReference: string | null
  balance: number | null
  partnerId: number | null
}

export interface ChartAccount {
  account: string
  name: string
  isActive: boolean
}

export interface JournalLine {
  id: number
  account: string
  postingDate: string
  debitAmount: number
  creditAmount: number
  documentRef: string | null
  subAccountId: string | null
  partnerAccountId: number | null
  note: string | null
}

export interface JournalDetail {
  header: { id: number; postingDate: string; description: string; isPosted: boolean }
  lines: JournalLine[]
}

/** Drops empty values so the URL only carries set filters. */
function query(params: Record<string, string | number | boolean | null | undefined>) {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value))
  }
  return search.toString()
}

const base = (companyId: number) => `/api/v1/companies/${companyId}`

export const ledgerCardsApi = {
  card: (companyId: number, params: Record<string, string | number | undefined>) =>
    apiRequest<LedgerCard>(`${base(companyId)}/ledger-cards?${query(params)}`),
  balances: (companyId: number, params: Record<string, string | number | boolean | undefined>) =>
    apiRequest<PageResponse<PartnerBalance>>(`${base(companyId)}/partner-balances?${query(params)}`),
  search: (companyId: number, q: string) =>
    apiRequest<SearchResult[]>(`${base(companyId)}/search?${query({ q })}`),
  chartOfAccounts: (companyId: number) => apiRequest<ChartAccount[]>(`${base(companyId)}/chart-of-accounts`),
  journal: (companyId: number, id: number) => apiRequest<JournalDetail>(`${base(companyId)}/journal-entries/${id}`),
}
