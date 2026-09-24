import { Alert, Autocomplete, Button, CircularProgress, Grid, Stack, TextField } from '@mui/material'
import { useEffect, useState, type FormEvent } from 'react'
import { getErrorMessage } from '../../../api/problemDetails'
import { useCompanyDetail, useLocationCategories, useUpdateCompany } from '../useMasterData'

interface CompanyFormProps {
  companyId: number
}

export function CompanyForm({ companyId }: CompanyFormProps) {
  const company = useCompanyDetail(companyId)
  const update = useUpdateCompany(companyId)
  const locationCategories = useLocationCategories(companyId)
  const [shortName, setShortName] = useState('')
  const [printName, setPrintName] = useState('')
  const [relativeFolderName, setRelativeFolderName] = useState('')
  const [locationCategoryId, setLocationCategoryId] = useState<number | null>(null)
  const [note, setNote] = useState('')
  const [sortIndex, setSortIndex] = useState('')
  const [externalAccount, setExternalAccount] = useState('')

  useEffect(() => {
    if (company.data) {
      setShortName(company.data.shortName)
      setPrintName(company.data.printName)
      setRelativeFolderName(company.data.relativeFolderName ?? '')
      setLocationCategoryId(company.data.locationCategoryId)
      setNote(company.data.note ?? '')
      setSortIndex(company.data.sortIndex === null ? '' : String(company.data.sortIndex))
      setExternalAccount(company.data.externalAccount ?? '')
    }
  }, [company.data])

  if (company.isLoading) return <CircularProgress aria-label="Učitavanje kompanije" />
  if (company.isError || !company.data) return <Alert severity="error">Kompanija nije mogla da se učita.</Alert>

  const submit = (event: FormEvent) => {
    event.preventDefault()
    update.mutate({
      partnerId: company.data.partnerId,
      managerId: company.data.managerId,
      shortName,
      printName,
      relativeFolderName: relativeFolderName || null,
      companyTypeId: company.data.companyTypeId,
      vatTypeId: company.data.vatTypeId,
      ledgerEntryDate: company.data.ledgerEntryDate,
      locationCategoryId,
      note: note || null,
      sortIndex: sortIndex === '' ? null : Number(sortIndex),
      externalAccount: externalAccount || null,
      rowVersion: company.data.rowVersion,
    })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit} noValidate>
      {update.isError && <Alert severity="error">{getErrorMessage(update.error, 'Izmena nije sačuvana. Osvežite podatke ako ih je drugi korisnik menjao.')}</Alert>}
      {update.isSuccess && <Alert severity="success">Kompanija je sačuvana.</Alert>}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Kratak naziv" required value={shortName} onChange={(event) => setShortName(event.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Naziv za štampu" required value={printName} onChange={(event) => setPrintName(event.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Relativni folder" value={relativeFolderName} onChange={(event) => setRelativeFolderName(event.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Autocomplete
            size="small"
            options={locationCategories.data ?? []}
            loading={locationCategories.isLoading}
            getOptionLabel={(option) => option.name}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            value={locationCategories.data?.find((item) => item.id === locationCategoryId) ?? null}
            onChange={(_, option) => setLocationCategoryId(option?.id ?? null)}
            renderInput={(params) => <TextField {...params} label="Lokacijska kategorija" />}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            fullWidth
            size="small"
            type="number"
            label="Redosled prikaza"
            value={sortIndex}
            onChange={(event) => setSortIndex(event.target.value)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" label="Spoljni konto (knjigovodstvena agencija)" value={externalAccount} onChange={(event) => setExternalAccount(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
        </Grid>
        <Grid size={12}>
          <TextField fullWidth size="small" multiline minRows={2} label="Napomena" value={note} onChange={(event) => setNote(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
        </Grid>
      </Grid>
      <Button type="submit" variant="contained" disabled={update.isPending || !shortName.trim() || !printName.trim()} sx={{ alignSelf: 'flex-end' }}>Sačuvaj</Button>
    </Stack>
  )
}
