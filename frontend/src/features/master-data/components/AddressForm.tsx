import { Alert, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../api/problemDetails'
import { FormActions, isSaveNewSubmit } from '../../../shared/components/FormActions'
import { useSaveAddress } from '../useMasterData'
import type { Address } from '../types'

interface AddressFormProps {
  companyId: number
  address?: Address
  onSaved?: (address: Address) => void
  /** Offers "Sačuvaj i novi" (Ctrl+Enter). */
  onSavedNew?: (address: Address) => void
  onCancel?: () => void
}

export function AddressForm({ companyId, address, onSaved, onSavedNew, onCancel }: AddressFormProps) {
  const { t } = useTranslation()
  const save = useSaveAddress(companyId, address?.id)
  const [streetAddress, setStreetAddress] = useState(address?.streetAddress ?? '')
  const [postalCode, setPostalCode] = useState(address?.postalCode ?? '')
  const [city, setCity] = useState(address?.city ?? '')
  const [countryCode, setCountryCode] = useState(address?.countryCode ?? 'RS')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const andNew = Boolean(onSavedNew) && isSaveNewSubmit(event)
    save.mutate(
      { streetAddress, postalCode: postalCode || null, city, countryCode },
      { onSuccess: andNew ? onSavedNew : onSaved },
    )
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('addresses_.saveError'))}</Alert>}
      <TextField size="small" label={t('fields.address')} required value={streetAddress} onChange={(event) => setStreetAddress(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField size="small" label={t('fields.postalCode')} value={postalCode} onChange={(event) => setPostalCode(event.target.value)} slotProps={{ htmlInput: { maxLength: 20 } }} sx={{ width: { sm: 180 } }} />
        <TextField size="small" label={t('fields.city')} required value={city} onChange={(event) => setCity(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} fullWidth />
        <TextField size="small" label={t('fields.country')} required value={countryCode} onChange={(event) => setCountryCode(event.target.value.toUpperCase())} slotProps={{ htmlInput: { maxLength: 2 } }} sx={{ width: { sm: 120 } }} />
      </Stack>
      <FormActions
        onCancel={onCancel}
        pending={save.isPending}
        disabled={!streetAddress.trim() || !city.trim() || countryCode.length !== 2}
        saveNew={Boolean(onSavedNew)}
      />
    </Stack>
  )
}
