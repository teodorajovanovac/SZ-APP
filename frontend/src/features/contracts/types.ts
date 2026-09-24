export type ContractsMode = 'single' | 'all' | 'location'

export interface ContractsOverviewQuery {
  mode: ContractsMode
  companyId?: number
  locationCategoryId?: number
  idSearch?: string
  search?: string
  isActive?: boolean
  page: number
  pageSize: number
}

export interface ContractOverviewRow {
  companyId: number
  companyShortName: string
  partnerAccountId: number | null
  partnerAccountNumber: number | null
  buildingEntranceId: number | null
  buildingEntranceName: string | null
  partnerId: number | null
  partnerName: string | null
  unitId: number
  unitName: string | null
  unitTypeName: string | null
  contractId: number
  isActive: boolean
}
