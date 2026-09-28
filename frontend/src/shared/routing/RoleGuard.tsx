import { Alert, Box, CircularProgress } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Outlet } from 'react-router-dom'
import type { UserRole } from '../../api/generated/client'
import { useAuth } from '../../features/auth/useAuth'
import { useCompanyRole } from '../../features/companies/useCompanyRole'

interface RoleGuardProps {
  allowedRoles: UserRole[]
}

// UI gating only; the API enforces authorization server-side. Access = global Identity role
// (Root) OR the caller's StaffAccess role in the active company.
export function RoleGuard({ allowedRoles }: RoleGuardProps) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const { role, isLoading } = useCompanyRole()
  const hasGlobal = user?.roles.some((r) => allowedRoles.includes(r)) ?? false
  const hasAccess = hasGlobal || (role !== undefined && allowedRoles.includes(role as UserRole))

  if (!hasAccess && isLoading) {
    return (
      <Box sx={{ p: 3 }}>
        <CircularProgress size={24} />
      </Box>
    )
  }

  if (!hasAccess) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">{t('roleGuard.noAccess')}</Alert>
      </Box>
    )
  }

  return <Outlet />
}
