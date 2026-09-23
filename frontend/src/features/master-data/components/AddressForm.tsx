import { Alert, Button, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveAddress } from '../useMasterData'
import type { Address } from '../types'

interface AddressFormProps {
  companyId: number
  address?: Address
  onSaved?: (address: Address) => void
  onCancel?: () => void
}

export function AddressForm({ companyId, address, onSaved, onCancel }: AddressFormProps) {
  const save = useSaveAddress(companyId, address?.id)
  const [streetAddress, setStreetAddress] = useState(address?.streetAddress ?? '')
  const [postalCode, setPostalCode] = useState(address?.postalCode ?? '')
  const [city, setCity] = useState(address?.city ?? '')
  const [countryCode, setCountryCode] = useState(address?.countryCode ?? 'RS')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate(
      { streetAddress, postalCode: postalCode || null, city, countryCode },
      { onSuccess: onSaved },
    )
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, 'Adresa nije sačuvana.')}</Alert>}
      <TextField size="small" label="Adresa" required value={streetAddress} onChange={(event) => setStreetAddress(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField size="small" label="Poštanski broj" value={postalCode} onChange={(event) => setPostalCode(event.target.value)} slotProps={{ htmlInput: { maxLength: 20 } }} sx={{ width: { sm: 180 } }} />
        <TextField size="small" label="Grad" required value={city} onChange={(event) => setCity(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} fullWidth />
        <TextField size="small" label="Država" required value={countryCode} onChange={(event) => setCountryCode(event.target.value.toUpperCase())} slotProps={{ htmlInput: { maxLength: 2 } }} sx={{ width: { sm: 120 } }} />
      </Stack>
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        {onCancel && <Button onClick={onCancel}>Odustani</Button>}
        <Button type="submit" variant="contained" disabled={save.isPending || !streetAddress.trim() || !city.trim() || countryCode.length !== 2}>Sačuvaj</Button>
      </Stack>
    </Stack>
  )
}

