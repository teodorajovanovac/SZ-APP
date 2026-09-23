import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import type { BillingPage } from '../billing/types'

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
