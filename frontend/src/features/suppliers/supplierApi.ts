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
  unitTypeIds: number[]
  rowVersion: string
}

export interface CreateSupplierInvoice {
  invoiceNo: number; codeName: string; caption: string; supplierPartnerAccountId: number; calculationTypeId: number
  periodYYMM: number; invoiceTotalCalculationAmountEur: number; invoiceTotalCalculationAmountRsd: number
  calculationAmountByCoefficientEur: number; calculationAmountByCoefficientRsd: number; paymentPriority: number
  subAccountId: string | null; documentTypeId: number; extraordinaryInvoiceMarker: string | null
  invoiceNameRule: string | null; invoiceDate: string; transactionDate: string; paymentDate: string | null
  invoiceDescription: string | null; paymentReference: string | null; unitTypeIds: number[]
}

export function useSupplierInvoices(companyId: number, page = 1, pageSize = 25) {
  return useQuery({
    queryKey: ['companies', companyId, 'supplier-invoices', page, pageSize],
    queryFn: () => apiRequest<BillingPage<SupplierInvoice>>(`/api/v1/companies/${companyId}/supplier-invoices?page=${page}&pageSize=${pageSize}`),
  })
}

export function useCreateSupplierInvoice(companyId: number) {
  const client = useQueryClient()
  return useMutation({
    mutationFn: async (request: CreateSupplierInvoice) => {
      const token = await apiRequest<{ token: string; headerName: string }>('/api/v1/auth/antiforgery')
      return apiRequest<SupplierInvoice>(`/api/v1/companies/${companyId}/supplier-invoices`, {
        method: 'POST', body: JSON.stringify(request), headers: { [token.headerName]: token.token },
      })
    },
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'supplier-invoices'] }),
  })
}
