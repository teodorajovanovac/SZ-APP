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

export interface PostingPeriodLock {
  id: number
  periodYYMM: number
  lockedAt: string
  lockedByStaffId: number
  unlockedAt: string | null
  unlockedByStaffId: number | null
}

export interface BankStatementAllocation {
  id: number
  account: string
  partnerAccountId?: number | null
  partnerName?: string | null
  amount: number
  kind: 'Reference' | 'Fifo' | 'Advance' | 'Manual'
  subAccountId?: string | null
  parameters?: string | null
  documentRef?: string | null
  invoiceId?: number | null
  supplierInvoiceId?: number | null
  collectionPriority?: number | null
  closesDocumentType?: number | null
}

export interface BankStatementAllocationInput {
  account: string
  partnerAccountId?: number | null
  amount: number
  subAccountId?: string | null
  parameters?: string | null
  documentRef?: string | null
  invoiceId?: number | null
  supplierInvoiceId?: number | null
  collectionPriority?: number | null
  closesDocumentType?: number | null
}

export interface BankStatementLine {
  id: number
  lineNumber: number
  payerRecipientName: string
  bankAccountNumber?: string | null
  info?: string | null
  code?: number | null
  debit: number
  credit: number
  paymentReference?: string | null
  status: 'Pending' | 'Matched' | 'Ignored' | 'Posted'
  matchSource?: string | null
  isConfidentMatch: boolean
  matchNote?: string | null
  bankRef?: string | null
  rowVersion: string
  allocations: BankStatementAllocation[]
}

export interface BankStatementDetail {
  header: BankStatementSummary
  lines: BankStatementLine[]
}

export interface BankStatementFormat {
  bankCode: number
  name: string
  isSupported: boolean
}

export interface BankStatementImportResult {
  statement: BankStatementDetail
  alreadyImported: boolean
  bankCode: number
  formatName: string
  warnings: string[]
}

export interface MatchPartnerOption {
  partnerAccountId: number
  account: string
  accountNumber: number
  partnerName: string
}

export interface BankTemplateCondition {
  field: string
  function: string
  value: string
}

export interface BankTemplate {
  id: number
  name: string
  partnerAccountId?: number | null
  subAccountId?: string | null
  isActive: boolean
  conditions: BankTemplateCondition[]
}
