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

export interface InvoiceLineSeed {
  supplierInvoiceId: number | null
  name: string
  quantity: number
  unitOfMeasureId: number | null
  unitPrice: number
  vatRate: number
  k1: number
  k2: number
  k3: number
  k4: number
  k5: number
  sortIndex: number
}

export interface InvoiceSeed {
  partnerId: number
  sequenceNumber: string
  partnerName: string
  address: string
  postalCode: string | null
  city: string
  taxNumber: string | null
  registrationNumber: string | null
  currency: string
  invoiceDeliveryLocation: string | null
  deliveryLocation: string | null
  paymentReference: string | null
  previousBalance: number
  benefitAmount: number
  interestAmount: number
  sortIndex: number
  contractIds: number[]
  lines: InvoiceLineSeed[]
}

export interface InvoiceGenerationRequest { invoices: InvoiceSeed[] }

export interface InvoiceBatchPreview {
  batchId: number
  fingerprint: string
  invoiceCount: number
  netAmount: number
  vatAmount: number
  interestAmount: number
  totalAmount: number
  invoices: Array<{
    partnerId: number
    sequenceNumber: string
    netAmount: number
    benefitAmount: number
    vatAmount: number
    interestAmount: number
    totalAmount: number
  }>
}
