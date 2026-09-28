import { useQuery } from '@tanstack/react-query'
import { api } from '../../api/generated/client'
import { useActiveCompany } from './useActiveCompany'

// SEC-02: effective role in the *active* company. Global Identity roles are only ever Root
// (Program.cs bootstrap); everyone else gets a per-company StaffAccess role. The context
// endpoint resolves both (Root for platform admins, otherwise the StaffAccess role).
export function useCompanyRole() {
  const { activeCompany } = useActiveCompany()
  const context = useQuery({
    queryKey: ['companies', activeCompany.id, 'context'],
    queryFn: () => api.companies.context(activeCompany.id),
  })
  const role = context.data?.role
  return {
    companyId: activeCompany.id,
    role,
    isLoading: context.isLoading,
    canWrite: role === 'Root' || role === 'Upravnik' || role === 'Moderator',
    // P13: only Upravnik/Root may unlock a posting period.
    canAdmin: role === 'Root' || role === 'Upravnik',
  }
}
