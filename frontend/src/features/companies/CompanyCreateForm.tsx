import { useMemo, useState, type FormEvent } from 'react'
import { Alert, Autocomplete, Button, Grid, Stack, TextField } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { authQueryKey } from '../auth/authContext'
import { masterDataApi } from '../master-data/masterDataApi'
import { useLocationCategories } from '../master-data/useMasterData'
import { useActiveCompany } from './useActiveCompany'
import { flattenLocationCategories } from '../master-data/locationCategoryPaths'

// ponytail: PartnerId is typed as a number -- there is no global-partner (CompanyId == null) lookup
// endpoint yet; swap for an Autocomplete once one exists.
export function CompanyCreateForm({ onSaved }: { onSaved: () => void }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { activeCompany } = useActiveCompany()
  const locations = useLocationCategories(activeCompany.id)
  const locationOptions = useMemo(() => flattenLocationCategories(locations.data ?? []), [locations.data])
  const [id, setId] = useState('')
  const [partnerId, setPartnerId] = useState('')
  const [shortName, setShortName] = useState('')
  const [printName, setPrintName] = useState('')
  const [locationCategoryId, setLocationCategoryId] = useState<number | null>(null)
  const create = useMutation({
    mutationFn: () =>
      masterDataApi.company.create({
        id: id === '' ? null : Number(id),
        partnerId: Number(partnerId),
        managerId: null,
        shortName: shortName.trim(),
        printName: printName.trim(),
        relativeFolderName: null,
        companyTypeId: null,
        vatTypeId: null,
        ledgerEntryDate: null,
        locationCategoryId,
        note: null,
        sortIndex: null,
        externalAccount: null,
      }),
    onSuccess: () => {
      // The header company picker reads /auth/me's company list.
      void queryClient.invalidateQueries({ queryKey: authQueryKey })
      onSaved()
    },
  })

  const submit = (event: FormEvent) => {
    event.preventDefault()
    create.mutate()
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit} noValidate>
      {create.isError && <Alert severity="error">{getErrorMessage(create.error, t('companiesAdmin_.saveError'))}</Alert>}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" type="number" label={t('companiesAdmin_.idOptional')} helperText={t('companiesAdmin_.idHint')} value={id} onChange={(event) => setId(event.target.value)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" type="number" required label={t('companiesAdmin_.partnerId')} helperText={t('companiesAdmin_.partnerIdHint')} value={partnerId} onChange={(event) => setPartnerId(event.target.value)} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" required label={t('companiesAdmin_.name')} value={shortName} onChange={(event) => setShortName(event.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField fullWidth size="small" required label={t('companiesAdmin_.printName')} value={printName} onChange={(event) => setPrintName(event.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Autocomplete
            size="small"
            options={locationOptions}
            getOptionLabel={(option) => option.label}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            value={locationOptions.find((item) => item.id === locationCategoryId) ?? null}
            onChange={(_, option) => setLocationCategoryId(option?.id ?? null)}
            renderInput={(params) => <TextField {...params} label={t('companiesAdmin_.location')} />}
          />
        </Grid>
      </Grid>
      <Button
        type="submit"
        variant="contained"
        disabled={create.isPending || !partnerId || !shortName.trim() || !printName.trim()}
        sx={{ alignSelf: 'flex-end' }}
      >
        {t('common.save')}
      </Button>
    </Stack>
  )
}
