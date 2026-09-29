export interface BillingPage<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface InvoiceBatch {
  id: number
  periodYYMM: number
  caption: string
  month: number
  year: number
  issueDate: string
  dueDate: string
  exchangeRateNbs: number
  status: 'Draft' | 'Generated' | 'Posted'
  journalEntryId: number | null
  rowVersion: string
}

export interface InvoiceSummary {
  id: number
  partnerId: number
  invoiceBatchId: number | null
  sequenceNumber: string
  issueDate: string
  dueDate: string
  partnerName: string
  address: string
  postalCode: string | null
  city: string
  currency: string
  amount: number
  vatAmount: number
  total: number
  interestAmount: number
  invoiceTotal: number
  isCancelled: boolean
  rowVersion: string
}

export interface CreateInvoiceBatch {
  periodYYMM: number
  caption: string
  month: number
  year: number
  place: string
  issueDate: string
  serviceDateFrom: string
  serviceDateTo: string
  transactionDate: string
  dueDate: string
  exchangeRateNbs: number
  extraordinaryInvoiceMarker: string | null
  balanceAsOfDate: string | null
  previousValueDate: string | null
  isInterestCalculated: boolean
  paymentPurpose: string | null
}

/** Item 8 wizard request: the server computes all amounts (audit 9.1 R0..R3). */
export interface GenerateInvoicesRequest {
  periodYYMM: number
  extraordinaryMarker: string | null
  place: string
  issueDate: string
  dueDate: string
  serviceDateFrom: string
  serviceDateTo: string
  transactionDate: string
  exchangeRateNbs: number
}

export interface InvoiceBatchPreview {
  companyId: number
  periodYYMM: number
  customerCount: number
  netTotal: number
  vatTotal: number
  interestTotal: number
  total: number
  customers: Array<{ customerId: number; customerName: string; net: number; vat: number; interest: number; total: number }>
}

export interface GenerateInvoicesResult { invoiceBatchId: number; alreadyGenerated: boolean; invoiceIds: number[] }
