import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Stack, TextField } from '@mui/material'
import { useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { clearSessionExpired, useSessionExpired } from './sessionExpiry'
import { useAuth } from './useAuth'

/**
 * UX-04: the session cookie expired while working. Sign in again on top of the current
 * page so unsaved form input stays where it is, then refetch what failed.
 */
export function ReloginDialog() {
  const { t } = useTranslation()
  const expired = useSessionExpired()
  const { user, login, logout } = useAuth()
  const queryClient = useQueryClient()
  const [password, setPassword] = useState('')
  const [error, setError] = useState<unknown>()
  const [pending, setPending] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    if (!user) return
    setPending(true)
    setError(undefined)
    try {
      await login({ email: user.email, password, rememberMe: false })
      setPassword('')
      clearSessionExpired()
      await queryClient.invalidateQueries({ predicate: (query) => query.state.status === 'error' })
    } catch (err) {
      setError(err)
    } finally {
      setPending(false)
    }
  }

  return (
    <Dialog open={expired && Boolean(user)} maxWidth="xs" fullWidth>
      <form onSubmit={(event) => void submit(event)}>
        <DialogTitle>{t('ui.session.title')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 0.5 }}>
            <DialogContentText>{t('ui.session.body')}</DialogContentText>
            {error ? <Alert severity="error">{getErrorMessage(error, t('login.failed'))}</Alert> : null}
            <TextField label={t('login.email')} value={user?.email ?? ''} disabled size="small" />
            <TextField
              label={t('login.password')}
              type="password"
              autoComplete="current-password"
              autoFocus
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              size="small"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { clearSessionExpired(); void logout() }}>{t('logout')}</Button>
          <Button type="submit" variant="contained" disabled={pending || !password}>{t('login.submit')}</Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}
