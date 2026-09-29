import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import '../../app/i18n.invoicePdf'
import type { BillingPage, CreateInvoiceBatch, GenerateInvoicesRequest, GenerateInvoicesResult, InvoiceBatch, InvoiceBatchPreview, InvoiceSummary } from './types'

// apiRequest attaches the antiforgery header automatically for unsafe methods; only
// the idempotency key needs adding here.
async function mutate<T>(path: string, body?: unknown, idempotent = false): Promise<T> {
  return apiRequest<T>(path, {
    method: 'POST',
    body: body === undefined ? undefined : JSON.stringify(body),
    headers: idempotent ? { 'Idempotency-Key': crypto.randomUUID() } : undefined,
  })
}

export const billingKeys = {
  batches: (companyId: number) => ['companies', companyId, 'invoice-batches'] as const,
  invoices: (companyId: number) => ['companies', companyId, 'invoices'] as const,
}

export function useInvoiceBatches(companyId: number, page = 1, pageSize = 25) {
  return useQuery({
    queryKey: [...billingKeys.batches(companyId), page, pageSize],
    queryFn: () => apiRequest<BillingPage<InvoiceBatch>>(`/api/v1/companies/${companyId}/invoice-batches?page=${page}&pageSize=${pageSize}`),
  })
}

export function useInvoices(companyId: number, page = 1, pageSize = 25) {
  return useQuery({
    queryKey: [...billingKeys.invoices(companyId), page, pageSize],
    queryFn: () => apiRequest<BillingPage<InvoiceSummary>>(`/api/v1/companies/${companyId}/invoices?page=${page}&pageSize=${pageSize}`),
  })
}

export function useCreateInvoiceBatch(companyId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: CreateInvoiceBatch) => mutate<InvoiceBatch>(`/api/v1/companies/${companyId}/invoice-batches`, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: billingKeys.batches(companyId) }),
  })
}

/** Item 8 wizard: server computes invoices (R0..R3) from supplier invoices and units. */
export function usePreviewInvoiceBatch(companyId: number) {
  return useMutation({
    mutationFn: (request: GenerateInvoicesRequest) =>
      mutate<InvoiceBatchPreview>(`/api/v1/companies/${companyId}/invoice-batches/preview`, request),
  })
}

export function useGenerateInvoiceBatch(companyId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: GenerateInvoicesRequest) =>
      mutate<GenerateInvoicesResult>(`/api/v1/companies/${companyId}/invoice-batches/generate`, request, true),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: billingKeys.batches(companyId) })
      await queryClient.invalidateQueries({ queryKey: billingKeys.invoices(companyId) })
    },
  })
}

export function usePostInvoiceBatch(companyId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (batchId: number) => mutate<InvoiceBatch>(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/post`, undefined, true),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: billingKeys.batches(companyId) }),
  })
}

export type InterestPresetKey = 'fromPreviousDueDate' | 'wholeMonth' | 'dueToDue' | 'custom'
export interface InterestPeriodPresets {
  defaultPreset: InterestPresetKey
  lastRunEnd: string | null
  presets: { key: InterestPresetKey; start: string; end: string }[]
}
export interface InterestRun {
  invoiceBatchId: number
  periodStart: string
  periodEnd: string
  rowCount: number
  totalInterest: number
  totals: { partnerAccountId: number; subAccountId: string; interest: number }[]
}

export interface InterestPresetParams { periodYYMM: number; previousValueDate?: string; balanceAsOfDate?: string; dueDate?: string }

/** P10: suggested interest periods for a billing period; the run always takes explicit dates. */
export function interestPresetsQuery(companyId: number, params: InterestPresetParams) {
  const search = new URLSearchParams({ periodYYMM: String(params.periodYYMM) })
  if (params.previousValueDate) search.set('previousValueDate', params.previousValueDate)
  if (params.balanceAsOfDate) search.set('balanceAsOfDate', params.balanceAsOfDate)
  if (params.dueDate) search.set('dueDate', params.dueDate)
  return {
    queryKey: ['companies', companyId, 'interest-presets', search.toString()],
    queryFn: () => apiRequest<InterestPeriodPresets>(`/api/v1/companies/${companyId}/interest/period-presets?${search.toString()}`),
  }
}

export function useInterestPeriodPresets(companyId: number, params: InterestPresetParams, enabled: boolean) {
  return useQuery({ ...interestPresetsQuery(companyId, params), enabled })
}

/** Resolves the period the run will use: a preset's dates, or the edited custom dates. */
export function resolveInterestPeriod(
  values: { interestPreset: InterestPresetKey | ''; interestStart: string; interestEnd: string },
  presets: { key: InterestPresetKey; start: string; end: string }[] | undefined,
  defaultPreset: InterestPresetKey | undefined,
) {
  const key = values.interestPreset || defaultPreset || 'fromPreviousDueDate'
  if (key === 'custom') return { key, start: values.interestStart, end: values.interestEnd }
  const preset = presets?.find((p) => p.key === key)
  return { key, start: preset?.start ?? '', end: preset?.end ?? '' }
}

/** Idempotent per batch: re-running replaces the batch's interest rows. */
export function useRunInterest(companyId: number) {
  return useMutation({
    mutationFn: (request: { invoiceBatchId: number; periodStart: string; periodEnd: string }) =>
      mutate<InterestRun>(`/api/v1/companies/${companyId}/interest/runs`, request),
  })
}

/** FIN-02: per-invoice red storno (negative amounts, same side, type 7). */
export function useCancelInvoice(companyId: number, invoiceId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ reason, rowVersion }: { reason: string; rowVersion: string }) =>
      mutate<InvoiceSummary>(`/api/v1/companies/${companyId}/invoices/${invoiceId}/cancel`, { reason, rowVersion }, true),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: billingKeys.invoices(companyId) }),
  })
}

// --- Invoice PDF / bulk email (GAP-02/03/26) ---

export interface InvoiceEmailPreview { totalInvoices: number; withEmail: number; missingEmail: number; missingCustomerNames: string[] }
export interface InvoiceEmailSendResult { enqueued: number; skipped: number }

// File downloads need the raw Response (Blob), so they fetch their own CSRF token
// like reportsApi's download() does.
async function downloadFile(path: string, method: 'GET' | 'POST', fileName: string) {
  const headers: Record<string, string> = {}
  if (method === 'POST') {
    const token = await apiRequest<{ token: string; headerName: string }>('/api/v1/auth/antiforgery')
    headers[token.headerName] = token.token
  }
  const response = await fetch(path, { method, credentials: 'include', headers })
  if (!response.ok) throw new Error(response.statusText)
  const url = URL.createObjectURL(await response.blob())
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.click()
  URL.revokeObjectURL(url)
}

export const downloadInvoicePdf = (companyId: number, invoiceId: number, sequenceNumber: string) =>
  downloadFile(`/api/v1/companies/${companyId}/invoices/${invoiceId}/pdf`, 'GET', `${sequenceNumber}.pdf`)

export const downloadBatchPdfZip = (companyId: number, batchId: number, periodYYMM: number) =>
  downloadFile(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/pdf`, 'POST', `racuni-${periodYYMM}.zip`)

export function usePreviewBatchEmails(companyId: number) {
  return useMutation({
    mutationFn: (batchId: number) =>
      mutate<InvoiceEmailPreview>(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/emails/preview`),
  })
}

export function useSendBatchEmails(companyId: number) {
  return useMutation({
    mutationFn: (batchId: number) =>
      mutate<InvoiceEmailSendResult>(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/emails/send`, undefined, true),
  })
}
