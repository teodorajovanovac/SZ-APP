export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface PageRequest {
  page: number
  pageSize: number
  search?: string
  sortBy?: string
  descending?: boolean
}

export interface CompanyDetail {
  id: number
  partnerId: number
  managerId: number | null
  shortName: string
  printName: string
  relativeFolderName: string | null
  companyTypeId: number | null
  vatTypeId: number | null
  ledgerEntryDate: string | null
  locationCategoryId: number | null
  note: string | null
  sortIndex: number | null
  externalAccount: string | null
  rowVersion: string
}

export type SaveCompany = Omit<CompanyDetail, 'id' | 'rowVersion'> & { rowVersion?: string }

export interface LocationCategory {
  id: number
  name: string
  parentId: number | null
  sortIndex: number
}

export interface ShortListItem {
  id: number
  tableName: string
  caption: string
  shortName: string | null
  description: string | null
  indexValue: number
  indexSort: number
  indexKey: string | null
}

export interface Partner {
  id: number
  companyId: number
  shortName: string
  name: string
  registrationNumber: string | null
  taxNumber: string | null
  jbkjs: string | null
  maskedIdCardNumber: string | null
  maskedJmbg: string | null
  partnerTypeId: number | null
  language: string
  note: string | null
}

export interface SavePartner {
  shortName: string
  name: string
  registrationNumber?: string | null
  taxNumber?: string | null
  jbkjs?: string | null
  idCardNumber?: string | null
  jmbg?: string | null
  partnerTypeId?: number | null
  language: string
  note?: string | null
}

export interface Address {
  id: number
  streetAddress: string
  postalCode: string | null
  city: string
  countryCode: string
}

export type SaveAddress = Omit<Address, 'id'>

export type StaffRole = 'Upravnik' | 'Moderator' | 'Review'

export interface StaffAccess {
  id: number
  staffId: number
  staffEmail: string
  companyId: number
  staffRole: StaffRole
}

export interface SaveStaffAccess {
  staffId: number
  staffRole: StaffRole
}

export interface BuildingEntrance {
  id: number
  companyId: number
  buildingName: string | null
  entranceName: string | null
  addressId: number | null
  buildingLabel: string | null
  description: string | null
  sortIndex: number | null
  rowVersion: string
}

export type SaveBuildingEntrance = Omit<BuildingEntrance, 'id' | 'companyId' | 'rowVersion'> & { rowVersion?: string }

export interface Unit {
  id: number
  companyId: number
  name: string | null
  contractId: number | null
  unitTypeId: number | null
  buildingEntranceId: number | null
  note: string | null
  sortingNumber: number | null
  k1: number | null
  k2: number | null
  k3: number | null
  k4: number | null
  k5: number | null
  floorNumber: number | null
  rowVersion: string
}

export type SaveUnit = Omit<Unit, 'id' | 'companyId' | 'contractId' | 'rowVersion'> & { rowVersion?: string }

export interface Contract {
  id: number
  unitId: number
  accountNumber: number | null
  ownerPartnerId: number | null
  invoicePartnerId: number | null
  tenantPartnerId: number | null
  contractDate: string
  contractEndDate: string | null
  invoiceStartDate: string | null
  invoiceEndDate: string | null
  isActive: boolean
  note: string | null
  invoiceDeliveryLocation: string | null
  invoiceDeliveryUnitId: number | null
  isPrintInvoiceMandatory: boolean
  isPrintInvoiceToPostOffice: boolean
  isPrintInvoiceSkipped: boolean
  exportExternalAccount: string | null
  rowVersion: string
}

export interface ReplaceContract {
  accountNumber: number | null
  ownerPartnerId: number | null
  invoicePartnerId: number | null
  tenantPartnerId: number | null
  effectiveFrom: string
  invoiceStartDate: string | null
  invoiceEndDate: string | null
  note: string | null
  invoiceDeliveryLocation: string | null
  invoiceDeliveryUnitId: number | null
  isPrintInvoiceMandatory: boolean
  isPrintInvoiceToPostOffice: boolean
  isPrintInvoiceSkipped: boolean
  exportExternalAccount: string | null
  currentContractRowVersion: string | null
}

export interface PartnerAccount {
  id: number
  companyId: number | null
  account: string
  partnerId: number
  contractId: number | null
  accountNumber: number
  rowVersion: string
}

export type SavePartnerAccount = Omit<PartnerAccount, 'id' | 'companyId' | 'rowVersion'> & { rowVersion?: string }

export interface BankAccount {
  id: number
  companyId: number | null
  accountNumber: string | null
  isActive: boolean
  partnerId: number | null
  sortIndex: number | null
  currency: string
  rowVersion: string
}

export type SaveBankAccount = Omit<BankAccount, 'id' | 'companyId' | 'rowVersion'> & { rowVersion?: string }
