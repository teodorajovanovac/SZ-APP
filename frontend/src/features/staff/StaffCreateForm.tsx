import { Alert, Button, Checkbox, FormControlLabel, MenuItem, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { useAuth } from '../auth/useAuth'
import { useActiveCompany } from '../companies/useActiveCompany'
import type { StaffRole } from '../master-data/types'
import { staffLanguages, useCreateStaff, type StaffDetail } from './staffApi'

interface StaffCreateFormProps {
  onSaved: (staff: StaffDetail) => void
  onCancel: () => void
}

export function StaffCreateForm({ onSaved, onCancel }: StaffCreateFormProps) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const { activeCompany } = useActiveCompany()
  const isRootCaller = user?.roles.includes('Root') ?? false
  const create = useCreateStaff()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [language, setLanguage] = useState<string>('sr-Latn')
  const [isRoot, setIsRoot] = useState(false)
  // '' = no initial grant (Root only). Non-Root must attach the user to a company they manage.
  const [companyId, setCompanyId] = useState<number | ''>(activeCompany.id)
  const [role, setRole] = useState<StaffRole>('Review')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    create.mutate(
      {
        email,
        temporaryPassword: password,
        preferredLanguage: language,
        isRoot,
        companyId: companyId === '' ? null : companyId,
        staffRole: companyId === '' ? null : role,
      },
      { onSuccess: onSaved },
    )
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {create.isError && <Alert severity="error">{getErrorMessage(create.error, t('staffAdmin_.saveError'))}</Alert>}
      <TextField size="small" type="email" required label={t('staffAdmin_.email')} value={email} onChange={(e) => setEmail(e.target.value)} />
      <TextField size="small" type="password" autoComplete="new-password" required label={t('staffAdmin_.tempPassword')} helperText={t('staffAdmin_.tempPasswordHint')} value={password} onChange={(e) => setPassword(e.target.value)} />
      <TextField size="small" select label={t('staffAdmin_.language')} value={language} onChange={(e) => setLanguage(e.target.value)}>
        {staffLanguages.map((code) => <MenuItem key={code} value={code}>{code}</MenuItem>)}
      </TextField>
      <TextField size="small" select label={t('staffAdmin_.company')} value={companyId} required={!isRootCaller} onChange={(e) => setCompanyId(e.target.value === '' ? '' : Number(e.target.value))}>
        {isRootCaller && <MenuItem value="">{t('staffAdmin_.noCompany')}</MenuItem>}
        {user?.companies.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
      </TextField>
      {companyId !== '' && (
        <TextField size="small" select label={t('staffAdmin_.role')} value={role} onChange={(e) => setRole(e.target.value as StaffRole)}>
          <MenuItem value="Upravnik">{t('roles.Upravnik')}</MenuItem>
          <MenuItem value="Moderator">{t('roles.Moderator')}</MenuItem>
          <MenuItem value="Review">{t('roles.Review')}</MenuItem>
        </TextField>
      )}
      {isRootCaller && (
        <FormControlLabel control={<Checkbox checked={isRoot} onChange={(e) => setIsRoot(e.target.checked)} />} label={t('staffAdmin_.root')} />
      )}
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button type="submit" variant="contained" disabled={create.isPending}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}
