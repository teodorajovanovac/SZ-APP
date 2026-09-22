export interface PageResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface JournalEntrySummary {
  id: number
  postingDate: string
  description: string
  currency: string
  balance: number
  isPosted: boolean
  postedAt?: string | null
  reversalOfId?: number | null
  rowVersion: string
}

export interface BankStatementSummary {
  id: number
  bankAccountId: number
  statementNumber: number
  statementSuffix?: string | null
  date: string
  previousBalance: number
  newBalance: number
  debit: number
  credit: number
  lineCount: number
  status: 'Imported' | 'PartiallyMatched' | 'Ready' | 'Posted'
  journalEntryId?: number | null
  rowVersion: string
}

export interface BankStatementLineImport {
  lineNumber: number
  payerRecipientName: string
  bankAccountNumber?: string | null
  debit: number
  credit: number
  info?: string | null
  code?: number | null
  paymentReference?: string | null
  paymentReferenceOut?: string | null
  bankRef?: string | null
}

export interface BankStatementImport {
  bankAccountId: number
  ledgerAccount: string
  statementNumber: number
  statementSuffix?: string | null
  date: string
  previousBalance: number
  newBalance: number
  debit: number
  credit: number
  lines: BankStatementLineImport[]
}

export interface PostingResult {
  journalEntryId: number
  alreadyPosted: boolean
  rowVersion: string
}
