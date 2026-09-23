import { Alert, Button, MenuItem, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveStaffAccess } from '../useMasterData'
import type { StaffAccess, StaffRole } from '../types'

interface StaffAccessFormProps {
  companyId: number
  access?: StaffAccess
  onSaved?: (access: StaffAccess) => void
}

export function StaffAccessForm({ companyId, access, onSaved }: StaffAccessFormProps) {
  const save = useSaveStaffAccess(companyId, access?.id)
  const [staffId, setStaffId] = useState(access?.staffId ? String(access.staffId) : '')
  const [staffRole, setStaffRole] = useState<StaffRole>(access?.staffRole ?? 'Review')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate({ staffId: Number(staffId), staffRole }, { onSuccess: onSaved })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, 'Pristup nije sačuvan.')}</Alert>}
      <TextField label="ID zaposlenog" type="number" required value={staffId} disabled={!!access} onChange={(event) => setStaffId(event.target.value)} inputProps={{ min: 1 }} />
      <TextField select label="Uloga" value={staffRole} onChange={(event) => setStaffRole(event.target.value as StaffRole)}>
        <MenuItem value="Upravnik">Upravnik</MenuItem>
        <MenuItem value="Moderator">Moderator</MenuItem>
        <MenuItem value="Review">Review</MenuItem>
      </TextField>
      <Button type="submit" variant="contained" disabled={save.isPending || Number(staffId) <= 0}>Sačuvaj</Button>
    </Stack>
  )
}

