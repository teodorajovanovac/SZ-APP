import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { Alert, Button, Checkbox, Chip, CircularProgress, FormControlLabel, MenuItem, Paper, Stack, Switch, TextField, Typography } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { getErrorMessage } from '../../api/problemDetails'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { FormDialog } from '../../shared/components/FormDialog'
import { formatDate } from '../../shared/format/date'
import { useAuth } from '../auth/useAuth'
import { StaffGrantsSection } from './StaffGrantsSection'
import { staffLanguages, useResetStaffPassword, useStaffDetail, useUpdateStaff, type StaffDetail, type UpdateStaff } from './staffApi'

export function StaffDetailPage() {
  const { t } = useTranslation()
  const staffId = Number(useParams().staffId)
  const detail = useStaffDetail(staffId)

  return (
    <Stack spacing={2}>
      <Button component={RouterLink} to="/staff" startIcon={<ArrowBackIcon />} sx={{ alignSelf: 'flex-start' }}>{t('staffAdmin_.back')}</Button>
      {detail.isLoading && <CircularProgress size={24} />}
      {detail.isError && <Alert severity="error">{getErrorMessage(detail.error, t('staffAdmin_.loadError'))}</Alert>}
      {detail.data && <StaffDetailView key={detail.data.id} staff={detail.data} />}
    </Stack>
  )
}

function StaffDetailView({ staff }: { staff: StaffDetail }) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const isRootCaller = user?.roles.includes('Root') ?? false
  const update = useUpdateStaff(staff.id)
  const [form, setForm] = useState<UpdateStaff>({
    email: staff.email,
    phoneNumber: staff.phoneNumber,
    preferredLanguage: staff.preferredLanguage,
    isActive: staff.isActive,
    isRoot: staff.isRoot,
  })
  const [confirmDeactivate, setConfirmDeactivate] = useState(false)
  const [resetting, setResetting] = useState(false)
  const readOnly = !staff.canManage

  const save = () => update.mutate(form, { onSuccess: () => setConfirmDeactivate(false) })
  const submit = (event: FormEvent) => {
    event.preventDefault()
    if (staff.isActive && !form.isActive) setConfirmDeactivate(true)
    else save()
  }

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" useFlexGap>
        <Typography component="h1" variant="h1">{staff.email}</Typography>
        {staff.isRoot && <Chip size="small" color="primary" label="Root" />}
        {!staff.isActive && <Chip size="small" label={t('staffAdmin_.inactive')} />}
        {staff.mustChangePassword && <Chip size="small" color="warning" label={t('staffAdmin_.mustChange')} />}
        {staff.isLockedOut && <Chip size="small" color="error" label={t('staffAdmin_.lockedOut')} />}
      </Stack>
      <Typography variant="body2" color="text.secondary">
        {t('staffAdmin_.lastLogin')}: {formatDate(staff.lastLoginAt) || '—'}{staff.lastIp ? ` (${staff.lastIp})` : ''}
      </Typography>
      {readOnly && <Alert severity="info">{t('staffAdmin_.readOnly')}</Alert>}
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack component="form" spacing={2} onSubmit={submit} sx={{ maxWidth: 480 }}>
          {update.isError && <Alert severity="error">{getErrorMessage(update.error, t('staffAdmin_.saveError'))}</Alert>}
          {update.isSuccess && <Alert severity="success">{t('staffAdmin_.saved')}</Alert>}
          <TextField size="small" type="email" required disabled={readOnly} label={t('staffAdmin_.email')} value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          <TextField size="small" disabled={readOnly} label={t('staffAdmin_.phone')} value={form.phoneNumber ?? ''} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value || null })} />
          <TextField size="small" select disabled={readOnly} label={t('staffAdmin_.language')} value={form.preferredLanguage} onChange={(e) => setForm({ ...form, preferredLanguage: e.target.value })}>
            {staffLanguages.map((code) => <MenuItem key={code} value={code}>{code}</MenuItem>)}
          </TextField>
          <FormControlLabel disabled={readOnly} control={<Switch checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />} label={t('staffAdmin_.active')} />
          {isRootCaller && (
            <FormControlLabel disabled={readOnly} control={<Checkbox checked={form.isRoot} onChange={(e) => setForm({ ...form, isRoot: e.target.checked })} />} label={t('staffAdmin_.root')} />
          )}
          {!readOnly && (
            <Stack direction="row" spacing={1}>
              <Button type="submit" variant="contained" disabled={update.isPending}>{t('common.save')}</Button>
              <Button onClick={() => setResetting(true)}>{t('staffAdmin_.resetPassword')}</Button>
            </Stack>
          )}
        </Stack>
      </Paper>
      <StaffGrantsSection staff={staff} />
      <ConfirmDialog
        open={confirmDeactivate}
        title={t('staffAdmin_.deactivateTitle')}
        description={t('staffAdmin_.deactivateBody')}
        confirmLabel={t('common.save')}
        destructive
        pending={update.isPending}
        onClose={() => setConfirmDeactivate(false)}
        onConfirm={save}
      />
      <FormDialog open={resetting} title={t('staffAdmin_.resetPassword')} onClose={() => setResetting(false)} maxWidth="xs">
        {resetting && <ResetPasswordForm staffId={staff.id} onDone={() => setResetting(false)} />}
      </FormDialog>
    </Stack>
  )
}

function ResetPasswordForm({ staffId, onDone }: { staffId: number; onDone: () => void }) {
  const { t } = useTranslation()
  const reset = useResetStaffPassword(staffId)
  const [password, setPassword] = useState('')
  const submit = (event: FormEvent) => {
    event.preventDefault()
    reset.mutate(password)
  }
  if (reset.isSuccess) {
    return (
      <Stack spacing={2}>
        <Alert severity="success">{t('staffAdmin_.resetDone')}</Alert>
        <Button variant="contained" onClick={onDone} sx={{ alignSelf: 'flex-end' }}>{t('ui.close')}</Button>
      </Stack>
    )
  }
  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {reset.isError && <Alert severity="error">{getErrorMessage(reset.error, t('staffAdmin_.saveError'))}</Alert>}
      <TextField size="small" type="password" autoComplete="new-password" required label={t('staffAdmin_.tempPassword')} helperText={t('staffAdmin_.tempPasswordHint')} value={password} onChange={(e) => setPassword(e.target.value)} />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button onClick={onDone}>{t('common.cancel')}</Button>
        <Button type="submit" variant="contained" disabled={reset.isPending || !password}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}
