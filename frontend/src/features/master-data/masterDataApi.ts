import { apiRequest } from '../../api/generated/client'
import type {
  Address,
  BankAccount,
  BuildingEntrance,
  CompanyDetail,
  Contract,
  PageRequest,
  PagedResponse,
  Partner,
  PartnerAccount,
  ReplaceContract,
  SaveAddress,
  SaveBankAccount,
  SaveBuildingEntrance,
  SaveCompany,
  SavePartner,
  SavePartnerAccount,
  SaveStaffAccess,
  SaveUnit,
  StaffAccess,
  Unit,
} from './types'

function pageQuery(request: PageRequest): string {
  const params = new URLSearchParams({
    page: String(request.page),
    pageSize: String(request.pageSize),
  })
  if (request.search) params.set('search', request.search)
  if (request.sortBy) params.set('sortBy', request.sortBy)
  if (request.descending) params.set('descending', 'true')
  return params.toString()
}

const companyBase = (companyId: number) => `/api/v1/companies/${companyId}`

export const masterDataApi = {
  company: {
    get: (companyId: number) => apiRequest<CompanyDetail>(`${companyBase(companyId)}/`),
    update: (companyId: number, value: SaveCompany) =>
      apiRequest<CompanyDetail>(`${companyBase(companyId)}/`, {
        method: 'PUT',
        body: JSON.stringify(value),
      }),
    remove: (companyId: number, rowVersion: string) =>
      apiRequest<void>(`${companyBase(companyId)}/`, {
        method: 'DELETE',
        headers: { 'If-Match': `"${rowVersion}"` },
      }),
  },
  partners: {
    list: (companyId: number, request: PageRequest) =>
      apiRequest<PagedResponse<Partner>>(`${companyBase(companyId)}/partners?${pageQuery(request)}`),
    get: (companyId: number, partnerId: number) =>
      apiRequest<Partner>(`${companyBase(companyId)}/partners/${partnerId}`),
    create: (companyId: number, value: SavePartner) =>
      apiRequest<Partner>(`${companyBase(companyId)}/partners`, {
        method: 'POST',
        body: JSON.stringify(value),
      }),
    update: (companyId: number, partnerId: number, value: SavePartner) =>
      apiRequest<Partner>(`${companyBase(companyId)}/partners/${partnerId}`, {
        method: 'PUT',
        body: JSON.stringify(value),
      }),
    remove: (companyId: number, partnerId: number) =>
      apiRequest<void>(`${companyBase(companyId)}/partners/${partnerId}`, { method: 'DELETE' }),
  },
  addresses: {
    list: (companyId: number, request: PageRequest) =>
      apiRequest<PagedResponse<Address>>(`${companyBase(companyId)}/addresses?${pageQuery(request)}`),
    create: (companyId: number, value: SaveAddress) =>
      apiRequest<Address>(`${companyBase(companyId)}/addresses`, {
        method: 'POST',
        body: JSON.stringify(value),
      }),
    update: (companyId: number, addressId: number, value: SaveAddress) =>
      apiRequest<Address>(`${companyBase(companyId)}/addresses/${addressId}`, {
        method: 'PUT',
        body: JSON.stringify(value),
      }),
    remove: (companyId: number, addressId: number) =>
      apiRequest<void>(`${companyBase(companyId)}/addresses/${addressId}`, { method: 'DELETE' }),
  },
  staffAccess: {
    list: (companyId: number, request: PageRequest) =>
      apiRequest<PagedResponse<StaffAccess>>(`${companyBase(companyId)}/staff-access?${pageQuery(request)}`),
    create: (companyId: number, value: SaveStaffAccess) =>
      apiRequest<StaffAccess>(`${companyBase(companyId)}/staff-access`, {
        method: 'POST',
        body: JSON.stringify(value),
      }),
    update: (companyId: number, accessId: number, value: SaveStaffAccess) =>
      apiRequest<StaffAccess>(`${companyBase(companyId)}/staff-access/${accessId}`, {
        method: 'PUT',
        body: JSON.stringify(value),
      }),
    remove: (companyId: number, accessId: number) =>
      apiRequest<void>(`${companyBase(companyId)}/staff-access/${accessId}`, { method: 'DELETE' }),
  },
  buildingEntrances: crudResource<BuildingEntrance, SaveBuildingEntrance>('building-entrances'),
  units: crudResource<Unit, SaveUnit>('units'),
  contracts: {
    list: (companyId: number, unitId: number) =>
      apiRequest<Contract[]>(`${companyBase(companyId)}/units/${unitId}/contracts`),
    get: (companyId: number, contractId: number) =>
      apiRequest<Contract>(`${companyBase(companyId)}/contracts/${contractId}`),
    replace: (companyId: number, unitId: number, value: ReplaceContract) =>
      apiRequest<Contract>(`${companyBase(companyId)}/units/${unitId}/contracts/replace`, {
        method: 'POST',
        body: JSON.stringify(value),
      }),
  },
  partnerAccounts: crudResource<PartnerAccount, SavePartnerAccount>('partner-accounts'),
  bankAccounts: crudResource<BankAccount, SaveBankAccount>('bank-accounts'),
}

function crudResource<T extends { id: number; rowVersion: string }, TSave>(segment: string) {
  return {
    list: (companyId: number, request: PageRequest) =>
      apiRequest<PagedResponse<T>>(`${companyBase(companyId)}/${segment}?${pageQuery(request)}`),
    get: (companyId: number, id: number) => apiRequest<T>(`${companyBase(companyId)}/${segment}/${id}`),
    create: (companyId: number, value: TSave) =>
      apiRequest<T>(`${companyBase(companyId)}/${segment}`, { method: 'POST', body: JSON.stringify(value) }),
    update: (companyId: number, id: number, value: TSave) =>
      apiRequest<T>(`${companyBase(companyId)}/${segment}/${id}`, { method: 'PUT', body: JSON.stringify(value) }),
    remove: (companyId: number, value: T) =>
      apiRequest<void>(`${companyBase(companyId)}/${segment}/${value.id}`, {
        method: 'DELETE',
        headers: { 'If-Match': `"${value.rowVersion}"` },
      }),
  }
}
