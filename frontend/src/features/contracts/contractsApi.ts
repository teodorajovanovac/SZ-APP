import { apiRequest } from '../../api/generated/client'
import type { PagedResponse } from '../master-data/types'
import type { ContractOverviewRow, ContractsOverviewQuery } from './types'

export const contractsApi = {
  list: (query: ContractsOverviewQuery) => {
    const params = new URLSearchParams({
      mode: query.mode,
      page: String(query.page),
      pageSize: String(query.pageSize),
    })
    if (query.companyId != null) params.set('companyId', String(query.companyId))
    if (query.locationCategoryId != null) params.set('locationCategoryId', String(query.locationCategoryId))
    if (query.idSearch) params.set('idSearch', query.idSearch)
    if (query.search) params.set('search', query.search)
    if (query.isActive != null) params.set('isActive', String(query.isActive))
    return apiRequest<PagedResponse<ContractOverviewRow>>(`/api/v1/contracts?${params.toString()}`)
  },
}
