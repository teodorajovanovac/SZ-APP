import { Alert, Button, Stack, TextField } from '@mui/material'
import { useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveAddress } from '../useMasterData'
import type { Address } from '../types'

interface AddressFormProps {
  companyId: number
  address?: Address
  onSaved?: (address: Address) => void
}

export function AddressForm({ companyId, address, onSaved }: AddressFormProps) {
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
      <TextField label="Adresa" required value={streetAddress} onChange={(event) => setStreetAddress(event.target.value)} inputProps={{ maxLength: 255 }} />
      <TextField label="Poštanski broj" value={postalCode} onChange={(event) => setPostalCode(event.target.value)} inputProps={{ maxLength: 20 }} />
      <TextField label="Grad" required value={city} onChange={(event) => setCity(event.target.value)} inputProps={{ maxLength: 255 }} />
      <TextField label="Država" required value={countryCode} onChange={(event) => setCountryCode(event.target.value.toUpperCase())} inputProps={{ maxLength: 2 }} />
      <Button type="submit" variant="contained" disabled={save.isPending || !streetAddress.trim() || !city.trim() || countryCode.length !== 2}>Sačuvaj</Button>
    </Stack>
  )
}

