import { Alert, Button, CircularProgress, Stack, TextField } from '@mui/material'
import { useEffect, useState, type FormEvent } from 'react'
import { useCompanyDetail, useUpdateCompany } from '../useMasterData'

interface CompanyFormProps {
  companyId: number
}

export function CompanyForm({ companyId }: CompanyFormProps) {
  const company = useCompanyDetail(companyId)
  const update = useUpdateCompany(companyId)
  const [shortName, setShortName] = useState('')
  const [printName, setPrintName] = useState('')
  const [relativeFolderName, setRelativeFolderName] = useState('')

  useEffect(() => {
    if (company.data) {
      setShortName(company.data.shortName)
      setPrintName(company.data.printName)
      setRelativeFolderName(company.data.relativeFolderName ?? '')
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
      rowVersion: company.data.rowVersion,
    })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {update.isError && <Alert severity="error">Izmena nije sačuvana. Osvežite podatke ako ih je drugi korisnik menjao.</Alert>}
      {update.isSuccess && <Alert severity="success">Kompanija je sačuvana.</Alert>}
      <TextField label="Kratak naziv" required value={shortName} onChange={(event) => setShortName(event.target.value)} inputProps={{ maxLength: 50 }} />
      <TextField label="Naziv za štampu" required value={printName} onChange={(event) => setPrintName(event.target.value)} inputProps={{ maxLength: 50 }} />
      <TextField label="Relativni folder" value={relativeFolderName} onChange={(event) => setRelativeFolderName(event.target.value)} inputProps={{ maxLength: 50 }} />
      <Button type="submit" variant="contained" disabled={update.isPending || !shortName.trim() || !printName.trim()}>Sačuvaj</Button>
    </Stack>
  )
}

