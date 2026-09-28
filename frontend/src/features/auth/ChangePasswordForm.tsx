import { Alert, Button, Stack, TextField } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { api } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { authQueryKey } from './authContext'

interface ChangePasswordFormProps {
  onDone?: () => void
  onCancel?: () => void
}

/** Self-service password change; also the forced step after signing in with a temporary password. */
export function ChangePasswordForm({ onDone, onCancel }: ChangePasswordFormProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [confirm, setConfirm] = useState('')
  const change = useMutation({
    mutationFn: () => api.auth.changePassword(current, next),
    onSuccess: (user) => {
      queryClient.setQueryData(authQueryKey, user)
      onDone?.()
    },
  })
  const mismatch = confirm.length > 0 && confirm !== next

  const submit = (event: FormEvent) => {
    event.preventDefault()
    if (!mismatch) change.mutate()
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {change.isError && <Alert severity="error">{getErrorMessage(change.error, t('staffAdmin_.saveError'))}</Alert>}
      <TextField size="small" type="password" autoComplete="current-password" required label={t('staffAdmin_.currentPassword')} value={current} onChange={(e) => setCurrent(e.target.value)} />
      <TextField size="small" type="password" autoComplete="new-password" required label={t('staffAdmin_.newPassword')} helperText={t('staffAdmin_.passwordPolicy')} value={next} onChange={(e) => setNext(e.target.value)} />
      <TextField size="small" type="password" autoComplete="new-password" required label={t('staffAdmin_.confirmPassword')} value={confirm} error={mismatch} helperText={mismatch ? t('staffAdmin_.mismatch') : ' '} onChange={(e) => setConfirm(e.target.value)} />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>{t('common.cancel')}</Button>}
        <Button type="submit" variant="contained" disabled={change.isPending || mismatch || !current || !next}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}
