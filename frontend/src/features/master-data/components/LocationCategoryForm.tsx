import { useState, type FormEvent } from 'react'
import { Alert, Autocomplete, Button, Stack, TextField } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../api/problemDetails'
import { masterDataApi } from '../masterDataApi'
import { invalidateLocationCategories } from '../useMasterData'
import type { LocationCategory, SaveLocationCategory } from '../types'

interface LocationCategoryFormProps {
  companyId: number
  category?: LocationCategory
  all: LocationCategory[]
  onSaved: () => void
}

export function LocationCategoryForm({ companyId, category, all, onSaved }: LocationCategoryFormProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [name, setName] = useState(category?.name ?? '')
  const [parentId, setParentId] = useState<number | null>(category?.parentId ?? null)
  const [sortIndex, setSortIndex] = useState(String(category?.sortIndex ?? 0))
  const save = useMutation({
    mutationFn: (value: SaveLocationCategory) =>
      category
        ? masterDataApi.locationCategories.update(companyId, category.id, value)
        : masterDataApi.locationCategories.create(companyId, value),
    onSuccess: () => {
      void invalidateLocationCategories(queryClient)
      onSaved()
    },
  })
  const parents = all.filter((item) => item.id !== category?.id)

  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate({ name: name.trim(), parentId, sortIndex: Number(sortIndex) || 0 })
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit} noValidate>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('locations_.saveError'))}</Alert>}
      <TextField size="small" required label={t('locations_.name')} value={name} onChange={(event) => setName(event.target.value)} slotProps={{ htmlInput: { maxLength: 255 } }} />
      <Autocomplete
        size="small"
        options={parents}
        getOptionLabel={(option) => option.name}
        isOptionEqualToValue={(option, value) => option.id === value.id}
        value={parents.find((item) => item.id === parentId) ?? null}
        onChange={(_, option) => setParentId(option?.id ?? null)}
        renderInput={(params) => <TextField {...params} label={t('locations_.parent')} />}
      />
      <TextField size="small" type="number" label={t('locations_.sortIndex')} value={sortIndex} onChange={(event) => setSortIndex(event.target.value)} />
      <Button type="submit" variant="contained" disabled={save.isPending || !name.trim()} sx={{ alignSelf: 'flex-end' }}>
        {t('common.save')}
      </Button>
    </Stack>
  )
}
