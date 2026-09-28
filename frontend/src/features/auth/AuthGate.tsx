import { Alert, Box, Button, CircularProgress, Paper, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { ChangePasswordForm } from './ChangePasswordForm'
import { useAuth } from './useAuth'

export function AuthGate() {
  const { t } = useTranslation()
  const { user, isLoading, logout } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center' }} role="status" aria-label="Učitavanje">
        <CircularProgress />
      </Box>
    )
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: `${location.pathname}${location.search}` }} replace />
  }

  // Temporary (admin-set) password: the API refuses everything but /auth until it is changed.
  if (user.mustChangePassword) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 2 }}>
        <Paper variant="outlined" sx={{ p: 3, width: '100%', maxWidth: 420 }}>
          <Stack spacing={2}>
            <Typography component="h1" variant="h5">{t('staffAdmin_.changePassword')}</Typography>
            <Alert severity="info">{t('staffAdmin_.mustChangeInfo')}</Alert>
            <ChangePasswordForm />
            <Button onClick={() => void logout()}>{t('logout')}</Button>
          </Stack>
        </Paper>
      </Box>
    )
  }

  return <Outlet />
}
