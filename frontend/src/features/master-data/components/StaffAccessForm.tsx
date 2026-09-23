import { Alert, Button, MenuItem, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveStaffAccess } from '../useMasterData'
import type { StaffAccess, StaffRole } from '../types'

interface StaffAccessFormProps {
  companyId: number
  access?: StaffAccess
  onSaved?: (access: StaffAccess) => void
  onCancel?: () => void
}

export function StaffAccessForm({ companyId, access, onSaved, onCancel }: StaffAccessFormProps) {
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
      {/* ponytail: raw numeric ID — no staff-directory list endpoint is exposed yet to back
          an Autocomplete (useStaffAccess lists existing grants, not the staff pool). Wire
          one up once a /staff or similar lookup endpoint exists. */}
      <TextField size="small" label="ID zaposlenog" type="number" required value={staffId} disabled={!!access} onChange={(event) => setStaffId(event.target.value)} slotProps={{ htmlInput: { min: 1 } }} />
      <TextField size="small" select label="Uloga" value={staffRole} onChange={(event) => setStaffRole(event.target.value as StaffRole)}>
        <MenuItem value="Upravnik">Upravnik</MenuItem>
        <MenuItem value="Moderator">Moderator</MenuItem>
        <MenuItem value="Review">Review</MenuItem>
      </TextField>
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>Odustani</Button>}
        <Button type="submit" variant="contained" disabled={save.isPending || Number(staffId) <= 0}>Sačuvaj</Button>
      </Stack>
    </Stack>
  )
}

