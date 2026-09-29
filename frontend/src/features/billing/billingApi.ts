import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import '../../app/i18n.invoicePdf'
import type { BillingPage, CreateInvoiceBatch, InvoiceBatch, InvoiceBatchPreview, InvoiceGenerationRequest, InvoiceSummary } from './types'

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

export function usePreviewInvoiceBatch(companyId: number, batchId: number) {
  return useMutation({
    mutationFn: (request: InvoiceGenerationRequest) =>
      mutate<InvoiceBatchPreview>(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/preview`, request),
  })
}

export function useGenerateInvoiceBatch(companyId: number, batchId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: InvoiceGenerationRequest) =>
      mutate(`/api/v1/companies/${companyId}/invoice-batches/${batchId}/generate`, request, true),
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
