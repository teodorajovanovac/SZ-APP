import { Alert, Box } from '@mui/material'
import { Outlet } from 'react-router-dom'
import type { UserRole } from '../../api/generated/client'
import { useAuth } from '../../features/auth/useAuth'

interface RoleGuardProps {
  allowedRoles: UserRole[]
}

export function RoleGuard({ allowedRoles }: RoleGuardProps) {
  const { user } = useAuth()
  const hasAccess = user?.roles.some((role) => allowedRoles.includes(role)) ?? false

  if (!hasAccess) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">Nemate dozvolu za pristup ovom modulu.</Alert>
      </Box>
    )
  }

  return <Outlet />
}
