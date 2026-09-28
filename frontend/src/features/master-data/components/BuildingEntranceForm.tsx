import { useState, type FormEvent } from 'react'
import { Alert, Button, Grid, Stack, TextField } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../api/problemDetails'
import { useSaveBuildingEntrance } from '../useMasterData'
import type { BuildingEntrance } from '../types'

interface BuildingEntranceFormProps {
  companyId: number
  entrance?: BuildingEntrance
  onSaved: () => void
}

const optional = (value: string) => (value.trim() === '' ? null : value.trim())
const optionalNumber = (value: string) => (value === '' ? null : Number(value))

export function BuildingEntranceForm({ companyId, entrance, onSaved }: BuildingEntranceFormProps) {
  const { t } = useTranslation()
  const save = useSaveBuildingEntrance(companyId, entrance?.id)
  const [buildingName, setBuildingName] = useState(entrance?.buildingName ?? '')
  const [entranceName, setEntranceName] = useState(entrance?.entranceName ?? '')
  const [buildingLabel, setBuildingLabel] = useState(entrance?.buildingLabel ?? '')
  const [addressId, setAddressId] = useState(entrance?.addressId?.toString() ?? '')
  const [sortIndex, setSortIndex] = useState(entrance?.sortIndex?.toString() ?? '')
  const [description, setDescription] = useState(entrance?.description ?? '')

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate(
      {
        buildingName: optional(buildingName),
        entranceName: optional(entranceName),
        buildingLabel: optional(buildingLabel),
        addressId: optionalNumber(addressId),
        sortIndex: optionalNumber(sortIndex),
        description: optional(description),
        rowVersion: entrance?.rowVersion,
      },
      { onSuccess: onSaved },
    )
  }

  const field = (label: string, value: string, set: (value: string) => void, type?: string) => (
    <TextField fullWidth size="small" type={type} label={label} value={value} onChange={(event) => set(event.target.value)} slotProps={type ? undefined : { htmlInput: { maxLength: 255 } }} />
  )

  return (
    <Stack component="form" spacing={2} onSubmit={submit} noValidate>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('entrances_.saveError'))}</Alert>}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>{field(t('entrances_.buildingName'), buildingName, setBuildingName)}</Grid>
        <Grid size={{ xs: 12, sm: 6 }}>{field(t('entrances_.entranceName'), entranceName, setEntranceName)}</Grid>
        <Grid size={{ xs: 12, sm: 6 }}>{field(t('entrances_.buildingLabel'), buildingLabel, setBuildingLabel)}</Grid>
        <Grid size={{ xs: 12, sm: 3 }}>{field(t('entrances_.addressId'), addressId, setAddressId, 'number')}</Grid>
        <Grid size={{ xs: 12, sm: 3 }}>{field(t('entrances_.sortIndex'), sortIndex, setSortIndex, 'number')}</Grid>
        <Grid size={12}>{field(t('entrances_.description'), description, setDescription)}</Grid>
      </Grid>
      <Button type="submit" variant="contained" disabled={save.isPending} sx={{ alignSelf: 'flex-end' }}>
        {t('common.save')}
      </Button>
    </Stack>
  )
}
