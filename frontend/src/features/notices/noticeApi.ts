import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import type { BillingPage } from '../billing/types'
import { downloadFile } from '../billing/billingApi'

// --- Notice documents / cost posting / lawsuit (GAP-09, GAP-20) ---
export interface NoticeBatchListItem {
  id: number
  title: string
  date: string
  noticeCount: number
  totalCosts: number
  costsJournalEntryId: number | null
}
export interface NoticeEmailPreview { totalNotices: number; withEmail: number; missingEmail: number; missingCustomerNames: string[] }
export interface NoticeEmailSendResult { enqueued: number; skipped: number }
export type NoticeBatchAction = 'emails/preview' | 'emails/send' | 'costs/post' | 'costs/cancel'

const base = (companyId: number) => `/api/v1/companies/${companyId}`

export function useNoticeBatches(companyId: number) {
  return useQuery({
    queryKey: ['companies', companyId, 'notice-batches'],
    queryFn: () => apiRequest<NoticeBatchListItem[]>(`${base(companyId)}/notice-batches`),
  })
}

export function useNoticeBatchAction(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: ({ batchId, action }: { batchId: number; action: NoticeBatchAction }) =>
      apiRequest<unknown>(`${base(companyId)}/notice-batches/${batchId}/${action}`, {
        method: 'POST',
        headers: action === 'emails/preview' ? undefined : { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: ['companies', companyId, 'notice-batches'] })
      void client.invalidateQueries({ queryKey: ['companies', companyId, 'notices'] })
    },
  })
}

export function useSetNoticeLawsuit(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: ({ noticeId, isForLawsuit, lawyerCost }: { noticeId: number; isForLawsuit: boolean; lawyerCost: number | null }) =>
      apiRequest<unknown>(`${base(companyId)}/notices/${noticeId}/lawsuit`, { method: 'PUT', body: JSON.stringify({ isForLawsuit, lawyerCost }) }),
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'notices'] }),
  })
}

export const downloadNoticePdf = (companyId: number, noticeId: number) =>
  downloadFile(`${base(companyId)}/notices/${noticeId}/pdf`, 'GET', `opomena-${noticeId}.pdf`)
export const downloadNoticeZip = (companyId: number, batchId: number) =>
  downloadFile(`${base(companyId)}/notice-batches/${batchId}/pdf`, 'POST', `opomene-${batchId}.zip`)
export const downloadLawsuitCsv = (companyId: number, batchId?: number) =>
  downloadFile(`${base(companyId)}/notices/lawsuit/export${batchId ? `?batchId=${batchId}` : ''}`, 'GET', 'za-utuzenje.csv')

export interface Notice {
  id: number
  noticeBatchId: number
  partnerAccountId: number
  unpaidInvoiceCount: number
  debt: number
  additionalCosts: number
  total: number
  paymentReference: string
  deliveryStatus: 'Draft' | 'Rendered' | 'Queued' | 'Sent' | 'Failed'
  renderedDocumentPath: string | null
  rowVersion: string
  isForLawsuit: boolean
  lawyerCost: number | null
}

// apiRequest attaches the antiforgery header automatically for unsafe methods.
function noticeCommand(companyId: number, noticeId: number, command: 'render' | 'send') {
  return apiRequest<Notice>(`/api/v1/companies/${companyId}/notices/${noticeId}/${command}`, {
    method: 'POST',
  })
}

export function useNotices(companyId: number, page = 1, pageSize = 25) {
  return useQuery({
    queryKey: ['companies', companyId, 'notices', page, pageSize],
    queryFn: () => apiRequest<BillingPage<Notice>>(`/api/v1/companies/${companyId}/notices?page=${page}&pageSize=${pageSize}`),
  })
}

export function useNoticeCommand(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: ({ noticeId, command }: { noticeId: number; command: 'render' | 'send' }) => noticeCommand(companyId, noticeId, command),
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'notices'] }),
  })
}

export interface NoticeTemplate {
  id: number
  name: string
  body: string
  isActive: boolean
  rowVersion: string
}

export function useNoticeTemplates(companyId: number) {
  return useQuery({
    queryKey: ['companies', companyId, 'notice-templates'],
    queryFn: () => apiRequest<NoticeTemplate[]>(`/api/v1/companies/${companyId}/notice-templates`),
  })
}

export interface NoticeBatch {
  id: number
  title: string
  date: string
  noticeTemplateId: number
  noticeTypeId: number
  aditionalCostsLowerAmount: number | null
  aditionalCostsLowerLimit: number | null
  aditionalCostsUpperAmount: number | null
  rowVersion: string
}

/** FIN-12: only the cutoffs and thresholds go to the server -- debt/lines are computed from the GL. */
export interface CreateNoticeBatch {
  title: string
  date: string
  minUnpaidInvoiceCount: number
  debtTolerance: number
  debtToleranceByMonth: number
  noticeTemplateId: number
  noticeTypeId: number
  upToClaimDate: string
  upToPaymentDate: string
  invoiceBatchId: number | null
  customCaptionOnSlip: string | null
}

export function useCreateNoticeBatch(companyId: number) {
  return useMutation({
    mutationFn: (request: CreateNoticeBatch) =>
      apiRequest<NoticeBatch>(`/api/v1/companies/${companyId}/notice-batches`, { method: 'POST', body: JSON.stringify(request) }),
  })
}

export function useGenerateNotices(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: ({ batchId, confirm }: { batchId: number; confirm: boolean }) =>
      apiRequest<{ batchId: number; alreadyGenerated: boolean; noticeIds: number[] }>(
        `/api/v1/companies/${companyId}/notice-batches/${batchId}/generate`,
        { method: 'POST', body: JSON.stringify({ confirm }), headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'notices'] }),
  })
}

/** P11 NoticeAditionalCosts row (boss's spelling). companyId null = global (Root only). */
export interface NoticeCost {
  id: number
  dateStart: string
  dateEnd: string | null
  companyId: number | null
  aditionalCostsLowerAmount: number
  aditionalCostsLowerLimit: number
  aditionalCostsUpperAmount: number
  rowVersion: string
}

export interface SaveNoticeCost {
  dateStart: string
  dateEnd: string | null
  isGlobal: boolean
  aditionalCostsLowerAmount: number
  aditionalCostsLowerLimit: number
  aditionalCostsUpperAmount: number
  rowVersion: string | null
}

const noticeCostsKey = (companyId: number) => ['companies', companyId, 'notice-additional-costs'] as const

export function useNoticeCosts(companyId: number) {
  return useQuery({
    queryKey: noticeCostsKey(companyId),
    queryFn: () => apiRequest<NoticeCost[]>(`/api/v1/companies/${companyId}/notice-additional-costs`),
  })
}

export function useSaveNoticeCost(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: ({ id, request }: { id?: number; request: SaveNoticeCost }) =>
      apiRequest<NoticeCost>(`/api/v1/companies/${companyId}/notice-additional-costs${id ? `/${id}` : ''}`, {
        method: id ? 'PUT' : 'POST',
        body: JSON.stringify(request),
      }),
    onSuccess: () => client.invalidateQueries({ queryKey: noticeCostsKey(companyId) }),
  })
}

export function useDeleteNoticeCost(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => apiRequest<void>(`/api/v1/companies/${companyId}/notice-additional-costs/${id}`, { method: 'DELETE' }),
    onSuccess: () => client.invalidateQueries({ queryKey: noticeCostsKey(companyId) }),
  })
}
