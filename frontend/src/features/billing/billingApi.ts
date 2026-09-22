import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import type { BillingPage, CreateInvoiceBatch, InvoiceBatch, InvoiceBatchPreview, InvoiceGenerationRequest, InvoiceSummary } from './types'

interface AntiforgeryToken { token: string; headerName: string }

async function mutate<T>(path: string, body?: unknown, idempotent = false): Promise<T> {
  const token = await apiRequest<AntiforgeryToken>('/api/v1/auth/antiforgery')
  return apiRequest<T>(path, {
    method: 'POST',
    body: body === undefined ? undefined : JSON.stringify(body),
    headers: {
      [token.headerName]: token.token,
      ...(idempotent ? { 'Idempotency-Key': crypto.randomUUID() } : {}),
    },
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
