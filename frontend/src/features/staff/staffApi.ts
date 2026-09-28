import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import type { PagedResponse, StaffRole } from '../master-data/types'

export interface StaffListItem {
  id: number
  email: string
  isActive: boolean
  preferredLanguage: string
  isRoot: boolean
  companyCount: number
  lastLoginAt: string | null
}

export interface StaffGrant {
  accessId: number
  companyId: number
  companyName: string
  staffRole: StaffRole
}

export interface StaffDetail {
  id: number
  email: string
  phoneNumber: string | null
  isActive: boolean
  preferredLanguage: string
  isRoot: boolean
  mustChangePassword: boolean
  isLockedOut: boolean
  lastLoginAt: string | null
  lastIp: string | null
  canManage: boolean
  grants: StaffGrant[]
}

export interface CreateStaff {
  email: string
  temporaryPassword: string
  preferredLanguage: string
  isRoot: boolean
  companyId: number | null
  staffRole: StaffRole | null
}

export interface UpdateStaff {
  email: string
  phoneNumber: string | null
  preferredLanguage: string
  isActive: boolean
  isRoot: boolean
}

export const staffLanguages = ['sr-Latn', 'sr-Cyrl', 'en'] as const

const base = '/api/v1/staff'

export function useStaffList(page: number, pageSize: number, search: string, descending?: boolean) {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (search.trim()) params.set('search', search.trim())
  if (descending) params.set('descending', 'true')
  return useQuery({
    queryKey: ['staff', 'list', params.toString()],
    queryFn: () => apiRequest<PagedResponse<StaffListItem>>(`${base}?${params}`),
    placeholderData: keepPreviousData,
  })
}

export function useStaffDetail(staffId: number) {
  return useQuery({ queryKey: ['staff', staffId], queryFn: () => apiRequest<StaffDetail>(`${base}/${staffId}`), enabled: staffId > 0 })
}

function useStaffMutation<T>(fn: (value: T) => Promise<StaffDetail>) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: (detail) => {
      queryClient.setQueryData(['staff', detail.id], detail)
      void queryClient.invalidateQueries({ queryKey: ['staff', 'list'] })
    },
  })
}

export const useCreateStaff = () =>
  useStaffMutation((value: CreateStaff) => apiRequest<StaffDetail>(base, { method: 'POST', body: JSON.stringify(value) }))

export const useUpdateStaff = (staffId: number) =>
  useStaffMutation((value: UpdateStaff) => apiRequest<StaffDetail>(`${base}/${staffId}`, { method: 'PUT', body: JSON.stringify(value) }))

export const useResetStaffPassword = (staffId: number) =>
  useStaffMutation((temporaryPassword: string) =>
    apiRequest<StaffDetail>(`${base}/${staffId}/reset-password`, { method: 'POST', body: JSON.stringify({ temporaryPassword }) }))
