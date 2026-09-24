import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import type { BillingPage } from '../billing/types'

export interface SupplierInvoice {
  id: number
  invoiceNo: number
  codeName: string
  caption: string
  calculationTypeId: number
  periodYYMM: number
  amountEur: number
  amountRsd: number
  postedAmount: number
  invoiceDate: string
  transactionDate: string
  supplierPartnerAccountId: number
  documentTypeId: number
  extraordinaryInvoiceMarker: string | null
  unitTypeIds: number[]
  rowVersion: string
}

export interface SupplierInvoiceFilters {
  periodYYMM?: number
  supplierPartnerAccountId?: number
  hasExtraordinaryMarker?: boolean
  documentTypeId?: number
}

export interface CreateSupplierInvoice {
  invoiceNo: number; codeName: string; caption: string; supplierPartnerAccountId: number; calculationTypeId: number
  periodYYMM: number; invoiceTotalCalculationAmountEur: number; invoiceTotalCalculationAmountRsd: number
  calculationAmountByCoefficientEur: number; calculationAmountByCoefficientRsd: number; paymentPriority: number
  subAccountId: string | null; documentTypeId: number; extraordinaryInvoiceMarker: string | null
  invoiceNameRule: string | null; invoiceDate: string; transactionDate: string; paymentDate: string | null
  invoiceDescription: string | null; paymentReference: string | null; unitTypeIds: number[]
}

export function useSupplierInvoices(companyId: number, page = 1, pageSize = 25, filters: SupplierInvoiceFilters = {}) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (filters.periodYYMM != null) params.set('periodYYMM', String(filters.periodYYMM))
  if (filters.supplierPartnerAccountId != null) params.set('supplierPartnerAccountId', String(filters.supplierPartnerAccountId))
  if (filters.hasExtraordinaryMarker != null) params.set('hasExtraordinaryMarker', String(filters.hasExtraordinaryMarker))
  if (filters.documentTypeId != null) params.set('documentTypeId', String(filters.documentTypeId))
  return useQuery({
    queryKey: ['companies', companyId, 'supplier-invoices', page, pageSize, filters],
    queryFn: () => apiRequest<BillingPage<SupplierInvoice>>(`/api/v1/companies/${companyId}/supplier-invoices?${params.toString()}`),
  })
}

export function useCreateSupplierInvoice(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    // apiRequest attaches the antiforgery header automatically for unsafe methods.
    mutationFn: (request: CreateSupplierInvoice) =>
      apiRequest<SupplierInvoice>(`/api/v1/companies/${companyId}/supplier-invoices`, {
        method: 'POST', body: JSON.stringify(request),
      }),
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'supplier-invoices'] }),
  })
}
