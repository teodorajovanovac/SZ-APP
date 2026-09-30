import { useTranslation } from 'react-i18next'
import { Alert, Button, MenuItem, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveStaffAccess } from '../useMasterData'
import type { StaffAccess, StaffRole } from '../types'

interface StaffAccessFormProps {
  companyId: number
  access?: StaffAccess
  /** Fixed staff member (e.g. when adding a grant from the staff detail page). */
  fixedStaffId?: number
  onSaved?: (access: StaffAccess) => void
  onCancel?: () => void
}

export function StaffAccessForm({ companyId, access, fixedStaffId, onSaved, onCancel }: StaffAccessFormProps) {
  const { t } = useTranslation()
  const save = useSaveStaffAccess(companyId, access?.id)
  const [staffId, setStaffId] = useState(String(access?.staffId ?? fixedStaffId ?? ''))
  const [staffRole, setStaffRole] = useState<StaffRole>(access?.staffRole ?? 'Review')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate({ staffId: Number(staffId), staffRole }, { onSuccess: onSaved })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('staffAccess_.saveError'))}</Alert>}
      {/* ponytail: raw numeric ID here; the /staff pages add grants with a fixed staff member,
          which is the normal path. Autocomplete over GET /api/v1/staff if this form stays in use. */}
      <TextField size="small" label={t('fields.staffId')} type="number" required value={staffId} disabled={!!access || fixedStaffId !== undefined} onChange={(event) => setStaffId(event.target.value)} slotProps={{ htmlInput: { min: 1 } }} />
      <TextField size="small" select label={t('fields.role')} value={staffRole} onChange={(event) => setStaffRole(event.target.value as StaffRole)}>
        <MenuItem value="Upravnik">{t('roles.Upravnik')}</MenuItem>
        <MenuItem value="Moderator">{t('roles.Moderator')}</MenuItem>
        <MenuItem value="Review">{t('roles.Review')}</MenuItem>
      </TextField>
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>{t('common.cancel')}</Button>}
        <Button type="submit" variant="contained" disabled={save.isPending || Number(staffId) <= 0}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}

