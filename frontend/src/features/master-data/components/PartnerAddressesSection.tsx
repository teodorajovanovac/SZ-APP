import { useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, Checkbox, Chip, FormControlLabel, IconButton, MenuItem, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { getErrorMessage } from '../../../api/problemDetails'
import { useDeletePartnerAddress, usePartnerAddresses, useSavePartnerAddress } from '../useMasterData'
import { useShortList } from '../useShortList'
import type { PartnerAddress, SavePartnerAddress } from '../types'

interface PartnerAddressesSectionProps {
  companyId: number
  partnerId: number
}

// Rendered inside PartnerForm's <form>, so the inline editor deliberately uses no nested
// <form> and only type="button" buttons.
export function PartnerAddressesSection({ companyId, partnerId }: PartnerAddressesSectionProps) {
  const addresses = usePartnerAddresses(companyId, partnerId)
  const types = useShortList(companyId, 'AddressType')
  const remove = useDeletePartnerAddress(companyId, partnerId)
  const [editing, setEditing] = useState<PartnerAddress | 'new'>()
  const typeCaption = (id: number) => types.data?.find((item) => item.id === id)?.caption ?? ''

  return (
    <Stack spacing={1.5}>
      {addresses.isError && <Alert severity="error">Adrese nisu mogle da se učitaju.</Alert>}
      {remove.isError && <Alert severity="error">{getErrorMessage(remove.error, 'Adresa nije obrisana.')}</Alert>}
      {addresses.data?.length === 0 && !editing && <Typography color="text.secondary" variant="body2">Partner nema unetih adresa.</Typography>}
      {addresses.data?.map((item) => (
        <Stack key={item.id} direction="row" spacing={1} alignItems="center" justifyContent="space-between">
          <Typography variant="body2">
            {item.streetAddress}, {item.postalCode ? `${item.postalCode} ` : ''}{item.city}, {item.countryCode}
            {typeCaption(item.addressTypeId) && <Chip size="small" label={typeCaption(item.addressTypeId)} sx={{ ml: 1 }} />}
            {item.isDefault && <Chip size="small" color="primary" variant="outlined" label="Podrazumevana" sx={{ ml: 1 }} />}
          </Typography>
          <Stack direction="row">
            <Tooltip title="Izmeni">
              <IconButton size="small" aria-label="Izmeni adresu" onClick={() => setEditing(item)}><EditIcon fontSize="small" /></IconButton>
            </Tooltip>
            <Tooltip title="Obriši">
              <IconButton size="small" aria-label="Obriši adresu" disabled={remove.isPending} onClick={() => remove.mutate(item.id)}><DeleteOutlineIcon fontSize="small" /></IconButton>
            </Tooltip>
          </Stack>
        </Stack>
      ))}
      {editing ? (
        <AddressEditor
          key={editing === 'new' ? 'new' : editing.id}
          companyId={companyId}
          partnerId={partnerId}
          address={editing === 'new' ? undefined : editing}
          types={types.data ?? []}
          onDone={() => setEditing(undefined)}
        />
      ) : (
        <Button type="button" size="small" startIcon={<AddIcon />} onClick={() => setEditing('new')} sx={{ alignSelf: 'flex-start' }}>Dodaj adresu</Button>
      )}
    </Stack>
  )
}

interface AddressEditorProps {
  companyId: number
  partnerId: number
  address?: PartnerAddress
  types: { id: number; caption: string }[]
  onDone: () => void
}

function AddressEditor({ companyId, partnerId, address, types, onDone }: AddressEditorProps) {
  const save = useSavePartnerAddress(companyId, partnerId, address?.id)
  const [streetAddress, setStreetAddress] = useState(address?.streetAddress ?? '')
  const [postalCode, setPostalCode] = useState(address?.postalCode ?? '')
  const [city, setCity] = useState(address?.city ?? '')
  const [countryCode, setCountryCode] = useState(address?.countryCode ?? 'RS')
  const [addressTypeId, setAddressTypeId] = useState<number | ''>(address?.addressTypeId ?? '')
  const [isDefault, setIsDefault] = useState(address?.isDefault ?? false)

  const submit = () => {
    if (addressTypeId === '') return
    const value: SavePartnerAddress = { addressTypeId, isDefault, streetAddress, postalCode: postalCode || null, city, countryCode }
    save.mutate(value, { onSuccess: onDone })
  }
  // Enter inside the inline editor must not submit the surrounding partner form.
  const swallowEnter = (event: React.KeyboardEvent) => { if (event.key === 'Enter') { event.preventDefault(); submit() } }

  return (
    <Stack spacing={1.5} sx={{ border: 1, borderColor: 'divider', borderRadius: 1, p: 2 }} onKeyDown={swallowEnter}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, 'Adresa nije sačuvana.')}</Alert>}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField size="small" label="Adresa" required fullWidth value={streetAddress} onChange={(e) => setStreetAddress(e.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
        <TextField select size="small" label="Tip adrese" required value={addressTypeId} onChange={(e) => setAddressTypeId(Number(e.target.value))} sx={{ minWidth: 180 }}>
          {types.map((type) => <MenuItem key={type.id} value={type.id}>{type.caption}</MenuItem>)}
        </TextField>
      </Stack>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField size="small" label="Poštanski broj" value={postalCode} onChange={(e) => setPostalCode(e.target.value)} slotProps={{ htmlInput: { maxLength: 20 } }} sx={{ width: { sm: 180 } }} />
        <TextField size="small" label="Grad" required fullWidth value={city} onChange={(e) => setCity(e.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
        <TextField size="small" label="Država" required value={countryCode} onChange={(e) => setCountryCode(e.target.value.toUpperCase())} slotProps={{ htmlInput: { maxLength: 2 } }} sx={{ width: { sm: 120 } }} />
      </Stack>
      <FormControlLabel control={<Checkbox size="small" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)} />} label="Podrazumevana adresa za ovaj tip" />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button type="button" onClick={onDone}>Odustani</Button>
        <Button type="button" variant="contained" onClick={submit} disabled={save.isPending || addressTypeId === '' || !streetAddress.trim() || !city.trim() || countryCode.length !== 2}>Sačuvaj adresu</Button>
      </Stack>
    </Stack>
  )
}
