import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { masterDataApi } from './masterDataApi'
import type {
  PageRequest,
  ReplaceContract,
  SaveAddress,
  SaveBankAccount,
  SaveBuildingEntrance,
  SaveCompany,
  SavePartner,
  SavePartnerAccount,
  SaveStaffAccess,
  SaveUnit,
} from './types'

export const masterDataKeys = {
  company: (companyId: number) => ['master-data', companyId, 'company'] as const,
  locationCategories: (companyId: number) => ['master-data', companyId, 'location-categories'] as const,
  partners: (companyId: number, page: PageRequest) => ['master-data', companyId, 'partners', page] as const,
  addresses: (companyId: number, page: PageRequest) => ['master-data', companyId, 'addresses', page] as const,
  staffAccess: (companyId: number, page: PageRequest) => ['master-data', companyId, 'staff-access', page] as const,
  buildingEntrances: (companyId: number, page: PageRequest) => ['master-data', companyId, 'building-entrances', page] as const,
  units: (companyId: number, page: PageRequest) => ['master-data', companyId, 'units', page] as const,
  contracts: (companyId: number, unitId: number) => ['master-data', companyId, 'units', unitId, 'contracts'] as const,
  partnerAccounts: (companyId: number, page: PageRequest) => ['master-data', companyId, 'partner-accounts', page] as const,
  bankAccounts: (companyId: number, page: PageRequest) => ['master-data', companyId, 'bank-accounts', page] as const,
}

export function useCompanyDetail(companyId: number) {
  return useQuery({
    queryKey: masterDataKeys.company(companyId),
    queryFn: () => masterDataApi.company.get(companyId),
    enabled: companyId > 0,
  })
}

export function useUpdateCompany(companyId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveCompany) => masterDataApi.company.update(companyId, value),
    onSuccess: (company) => queryClient.setQueryData(masterDataKeys.company(companyId), company),
  })
}

export function useLocationCategories(companyId: number) {
  return useQuery({
    queryKey: masterDataKeys.locationCategories(companyId),
    queryFn: () => masterDataApi.company.locationCategories(companyId),
    enabled: companyId > 0,
    staleTime: 5 * 60_000,
  })
}

export function usePartners(companyId: number, page: PageRequest) {
  return useQuery({
    queryKey: masterDataKeys.partners(companyId, page),
    queryFn: () => masterDataApi.partners.list(companyId, page),
    enabled: companyId > 0,
    placeholderData: (previous) => previous,
  })
}

export function usePartnerDetail(companyId: number, partnerId: number) {
  return useQuery({
    queryKey: ['master-data', companyId, 'partners', partnerId],
    queryFn: () => masterDataApi.partners.get(companyId, partnerId),
    enabled: companyId > 0 && partnerId > 0,
    retry: false,
  })
}

export function useSavePartner(companyId: number, partnerId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SavePartner) =>
      partnerId
        ? masterDataApi.partners.update(companyId, partnerId, value)
        : masterDataApi.partners.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'partners'] }),
  })
}

export function useDeletePartner(companyId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (partnerId: number) => masterDataApi.partners.remove(companyId, partnerId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'partners'] }),
  })
}

export function useAddresses(companyId: number, page: PageRequest) {
  return useQuery({
    queryKey: masterDataKeys.addresses(companyId, page),
    queryFn: () => masterDataApi.addresses.list(companyId, page),
    enabled: companyId > 0,
    placeholderData: (previous) => previous,
  })
}

export function useSaveAddress(companyId: number, addressId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveAddress) =>
      addressId
        ? masterDataApi.addresses.update(companyId, addressId, value)
        : masterDataApi.addresses.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'addresses'] }),
  })
}

export function useStaffAccess(companyId: number, page: PageRequest) {
  return useQuery({
    queryKey: masterDataKeys.staffAccess(companyId, page),
    queryFn: () => masterDataApi.staffAccess.list(companyId, page),
    enabled: companyId > 0,
    placeholderData: (previous) => previous,
  })
}

export function useSaveStaffAccess(companyId: number, accessId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveStaffAccess) =>
      accessId
        ? masterDataApi.staffAccess.update(companyId, accessId, value)
        : masterDataApi.staffAccess.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'staff-access'] }),
  })
}

export function useBuildingEntrances(companyId: number, page: PageRequest) {
  return useQuery({ queryKey: masterDataKeys.buildingEntrances(companyId, page), queryFn: () => masterDataApi.buildingEntrances.list(companyId, page), enabled: companyId > 0, placeholderData: (previous) => previous })
}

export function useBuildingEntranceDetail(companyId: number, entranceId: number) {
  return useQuery({
    queryKey: ['master-data', companyId, 'building-entrances', entranceId],
    queryFn: () => masterDataApi.buildingEntrances.get(companyId, entranceId),
    enabled: companyId > 0 && entranceId > 0,
    retry: false,
  })
}

export function useSaveBuildingEntrance(companyId: number, entranceId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveBuildingEntrance) => entranceId ? masterDataApi.buildingEntrances.update(companyId, entranceId, value) : masterDataApi.buildingEntrances.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'building-entrances'] }),
  })
}

export function useUnits(companyId: number, page: PageRequest) {
  return useQuery({ queryKey: masterDataKeys.units(companyId, page), queryFn: () => masterDataApi.units.list(companyId, page), enabled: companyId > 0, placeholderData: (previous) => previous })
}

export function useUnitDetail(companyId: number, unitId: number) {
  return useQuery({
    queryKey: ['master-data', companyId, 'units', unitId],
    queryFn: () => masterDataApi.units.get(companyId, unitId),
    enabled: companyId > 0 && unitId > 0,
    retry: false,
  })
}

export function useSaveUnit(companyId: number, unitId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveUnit) => unitId ? masterDataApi.units.update(companyId, unitId, value) : masterDataApi.units.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'units'] }),
  })
}

export function useContractHistory(companyId: number, unitId: number) {
  return useQuery({ queryKey: masterDataKeys.contracts(companyId, unitId), queryFn: () => masterDataApi.contracts.list(companyId, unitId), enabled: companyId > 0 && unitId > 0 })
}

export function useReplaceContract(companyId: number, unitId: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: ReplaceContract) => masterDataApi.contracts.replace(companyId, unitId, value),
    onSuccess: () => Promise.all([
      queryClient.invalidateQueries({ queryKey: masterDataKeys.contracts(companyId, unitId) }),
      queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'units'] }),
    ]),
  })
}

export function usePartnerAccounts(companyId: number, page: PageRequest) {
  return useQuery({ queryKey: masterDataKeys.partnerAccounts(companyId, page), queryFn: () => masterDataApi.partnerAccounts.list(companyId, page), enabled: companyId > 0, placeholderData: (previous) => previous })
}

export function useSavePartnerAccount(companyId: number, accountId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SavePartnerAccount) => accountId ? masterDataApi.partnerAccounts.update(companyId, accountId, value) : masterDataApi.partnerAccounts.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'partner-accounts'] }),
  })
}

export function useBankAccounts(companyId: number, page: PageRequest) {
  return useQuery({ queryKey: masterDataKeys.bankAccounts(companyId, page), queryFn: () => masterDataApi.bankAccounts.list(companyId, page), enabled: companyId > 0, placeholderData: (previous) => previous })
}

export function useSaveBankAccount(companyId: number, accountId?: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (value: SaveBankAccount) => accountId ? masterDataApi.bankAccounts.update(companyId, accountId, value) : masterDataApi.bankAccounts.create(companyId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'bank-accounts'] }),
  })
}
