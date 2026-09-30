import { apiRequest } from '../../api/generated/client'

const base = (companyId: number) => `/api/v1/companies/${companyId}`
const json = (method: string, body: unknown, extra: Record<string, string> = {}): RequestInit => ({
  method, body: JSON.stringify(body), headers: extra,
})

export interface JournalLineInput {
  account: string; debitAmount: number; creditAmount: number; dueDate: string | null; documentRef: string | null
  subAccountId: string | null; partnerAccountId: number | null; note: string | null; paymentReference: string | null; description: string | null
}
export interface JournalInput {
  postingDate: string; dueDate: string | null; description: string; currency: string; journalEntryTypeId: number | null; lines: JournalLineInput[]
}
export interface JournalSummary { id: number; postingDate: string; description: string; isPosted: boolean; rowVersion: string; journalEntryTypeId: number | null }
export interface JournalLineOut {
  id: number; account: string; dueDate: string | null; debitAmount: number; creditAmount: number; subAccountId: string | null
  partnerAccountId: number | null; paymentReference: string | null; description: string | null
}
export interface JournalDetail { header: JournalSummary; lines: JournalLineOut[] }
export interface PartnerLookup { id: number; accountNumber: number; account: string; partnerName: string }
export interface PaymentOrder {
  id: number; supplierInvoiceId: number | null; payerName: string; recipientName: string; recipientAccountNumber: string
  amount: number; recipientPaymentReference: string | null; paymentPurpose: string; date: string; isArchived: boolean
}
export interface ReclassRow { partnerAccountId: number; accountNumber: number; partnerName: string; advance: number; openAmount: number; amount: number; referenceCount: number }
export interface AccountBalance { account: string; name: string | null; debit: number; credit: number; balance: number }

async function openFile(path: string, download?: string) {
  const response = await fetch(`${(import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')}${path}`, { credentials: 'include' })
  if (!response.ok) throw new Error(String(response.status))
  const url = URL.createObjectURL(await response.blob())
  if (download) {
    const a = document.createElement('a')
    a.href = url; a.download = download; a.click()
  } else window.open(url, '_blank')
  setTimeout(() => URL.revokeObjectURL(url), 60_000)
}

export const accountingApi = {
  copySuppliers: (c: number, body: { fromPeriodYYMM: number; toPeriodYYMM: number; scope: number }) =>
    apiRequest<{ copied: number; skipped: number }>(`${base(c)}/supplier-invoices/copy-to-period`, json('POST', body)),
  createOrders: (c: number, periodYYMM: number) =>
    apiRequest<{ created: number; skipped: { supplierInvoiceId: number; caption: string; reason: string }[] }>(
      `${base(c)}/supplier-invoices/payment-orders`, json('POST', { supplierInvoiceIds: null, periodYYMM, date: null })),
  orders: (c: number) => apiRequest<PaymentOrder[]>(`${base(c)}/payment-orders/supplier`),
  printOrders: (c: number, ids: number[]) => openFile(`${base(c)}/payment-orders/print?ids=${ids.join(',')}`),
  exportOrders: (c: number, ids: number[]) => openFile(`${base(c)}/payment-orders/export?ids=${ids.join(',')}`, 'virmani.csv'),
  archiveOrder: (c: number, id: number) => apiRequest<void>(`${base(c)}/payment-orders/${id}/archive`, { method: 'POST' }),
  deleteOrder: (c: number, id: number) => apiRequest<void>(`${base(c)}/payment-orders/${id}`, { method: 'DELETE' }),
  reclassPreview: (c: number) => apiRequest<ReclassRow[]>(`${base(c)}/advance-reclassification/preview`),
  reclass: (c: number) => apiRequest<{ journalCount: number; totalAmount: number }>(`${base(c)}/advance-reclassification`,
    json('POST', { postingDate: null, partnerAccountIds: null }, { 'Idempotency-Key': crypto.randomUUID() })),
  balances: (c: number, asOf: string) => apiRequest<AccountBalance[]>(`${base(c)}/account-balances?asOf=${asOf}`),
  lookup: (c: number, numbers: number[]) => apiRequest<PartnerLookup[]>(`${base(c)}/partner-accounts/lookup?numbers=${numbers.join(',')}`),
  journal: (c: number, id: number) => apiRequest<JournalDetail>(`${base(c)}/journal-entries/${id}`),
  createDraft: (c: number, body: JournalInput) => apiRequest<{ header: JournalSummary }>(`${base(c)}/journal-entries`, json('POST', body)),
  updateDraft: (c: number, id: number, rowVersion: string, journal: JournalInput) =>
    apiRequest<{ header: JournalSummary }>(`${base(c)}/journal-entries/${id}`, json('PUT', { rowVersion, journal })),
  deleteDraft: (c: number, id: number, rowVersion: string) =>
    apiRequest<void>(`${base(c)}/journal-entries/${id}?rowVersion=${encodeURIComponent(rowVersion)}`, { method: 'DELETE' }),
  post: (c: number, id: number, rowVersion: string) =>
    apiRequest<unknown>(`${base(c)}/journal-entries/${id}/post`, json('POST', { rowVersion }, { 'Idempotency-Key': crypto.randomUUID() })),
}

/** Serbian number input: "1.234,56" → 1234.56; empty → 0. */
export function parseAmount(text: string): number {
  const n = Number(text.trim().replace(/\./g, '').replace(',', '.'))
  return text.trim() === '' || Number.isNaN(n) ? 0 : n
}
