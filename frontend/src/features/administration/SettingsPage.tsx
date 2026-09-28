import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, Checkbox, FormControlLabel, IconButton, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState } from '@tanstack/react-table'
import { useMemo, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useCompanyRole } from '../companies/useCompanyRole'

interface Setting {
  id: number
  companyId: number | null
  name: string
  key: string
  value: string | null
  description: string | null
  category: string | null
  valueMax: string | null
  rowVersion: string
  isSecret: boolean
}

// Mirrors StaffManagementPolicy.IsSecretSettingKey on the server (which is what actually masks).
const isSecretKey = (key: string) => /password|passwd|pwd|secret|token|key|credential|connectionstring/i.test(key)

export function SettingsPage() {
  const { t } = useTranslation()
  const { companyId, role } = useCompanyRole()
  const isRoot = role === 'Root'
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [editing, setEditing] = useState<Setting | 'new'>()
  const settings = useQuery({
    queryKey: ['settings', companyId],
    queryFn: () => apiRequest<Setting[]>(`/api/v1/companies/${companyId}/settings`),
  })
  // Global rows and secrets: Root only. Company rows: Root or that company's Upravnik (server enforces).
  const canEdit = (s: Pick<Setting, 'companyId' | 'key'>) =>
    isRoot || (s.companyId !== null && !isSecretKey(s.key) && role === 'Upravnik')

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    const all = settings.data ?? []
    return q ? all.filter((s) => [s.key, s.name, s.category, s.description].some((v) => v?.toLowerCase().includes(q))) : all
  }, [settings.data, search])
  const page = filtered.slice(pagination.pageIndex * pagination.pageSize, (pagination.pageIndex + 1) * pagination.pageSize)

  const columns = useMemo<ColumnDef<Setting>[]>(
    () => [
      { accessorKey: 'key', header: t('settings_.key'), enableSorting: false },
      { accessorKey: 'name', header: t('settings_.name'), enableSorting: false },
      {
        accessorKey: 'value',
        header: t('settings_.value'),
        enableSorting: false,
        meta: { ellipsis: true },
        cell: ({ row }) => (row.original.isSecret ? t('settings_.masked') : row.original.value),
      },
      { accessorKey: 'category', header: t('settings_.category'), enableSorting: false },
      {
        id: 'scope',
        header: t('settings_.scope'),
        enableSorting: false,
        cell: ({ row }) => (row.original.companyId === null ? t('settings_.global') : t('settings_.company')),
      },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) =>
          canEdit(row.original) ? (
            <Tooltip title={t('ui.edit')}>
              <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}><EditIcon fontSize="small" /></IconButton>
            </Tooltip>
          ) : null,
      },
    ],
    // canEdit depends only on role/isRoot
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [t, role],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('settings_.title')}</Typography>
        {(isRoot || role === 'Upravnik') && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>{t('settings_.new')}</Button>
        )}
      </Stack>
      <TextField size="small" label={t('settings_.search')} value={search} onChange={(e) => { setSearch(e.target.value); setPagination((p) => ({ ...p, pageIndex: 0 })) }} />
      {settings.isError && <Alert severity="error">{t('settings_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('settings_.title')}
        rows={page}
        columns={columns}
        rowCount={filtered.length}
        pagination={pagination}
        sorting={[]}
        onPaginationChange={setPagination}
        onSortingChange={() => undefined}
        isLoading={settings.isLoading}
        emptyMessage={t('settings_.empty')}
        getRowId={(row) => String(row.id)}
        minWidth={720}
      />
      <FormDialog open={Boolean(editing)} title={editing === 'new' ? t('settings_.new') : t('settings_.editTitle')} onClose={() => setEditing(undefined)}>
        {editing && (
          <SettingForm companyId={companyId} setting={editing === 'new' ? undefined : editing} isRoot={isRoot} onDone={() => setEditing(undefined)} />
        )}
      </FormDialog>
    </Stack>
  )
}

function SettingForm({ companyId, setting, isRoot, onDone }: { companyId: number; setting?: Setting; isRoot: boolean; onDone: () => void }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [key, setKey] = useState(setting?.key ?? '')
  const [name, setName] = useState(setting?.name ?? '')
  const [value, setValue] = useState(setting?.isSecret ? '' : (setting?.value ?? ''))
  const [category, setCategory] = useState(setting?.category ?? '')
  const [description, setDescription] = useState(setting?.description ?? '')
  const [global, setGlobal] = useState(setting ? setting.companyId === null : false)
  const secret = isSecretKey(key)
  const save = useMutation({
    mutationFn: () =>
      apiRequest<Setting>(`/api/v1/companies/${companyId}/settings/${encodeURIComponent(key.trim())}${global ? '?global=true' : ''}`, {
        method: 'PUT',
        body: JSON.stringify({
          name, key, value: secret && !value ? null : value, category: category || null, description: description || null,
          valueMax: secret ? null : (setting?.valueMax ?? null), rowVersion: setting?.rowVersion ?? null,
        }),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['settings'] })
      onDone()
    },
  })
  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate()
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('settings_.saveError'))}</Alert>}
      <TextField size="small" required label={t('settings_.key')} value={key} disabled={Boolean(setting)} onChange={(e) => setKey(e.target.value)} slotProps={{ htmlInput: { maxLength: 50 } }} />
      <TextField size="small" required label={t('settings_.name')} value={name} onChange={(e) => setName(e.target.value)} />
      <TextField
        size="small"
        label={t('settings_.value')}
        type={secret ? 'password' : 'text'}
        autoComplete="off"
        multiline={!secret}
        value={value}
        helperText={secret ? t('settings_.secretHint') : ' '}
        onChange={(e) => setValue(e.target.value)}
      />
      <TextField size="small" label={t('settings_.category')} value={category} onChange={(e) => setCategory(e.target.value)} />
      <TextField size="small" multiline label={t('settings_.description')} value={description} onChange={(e) => setDescription(e.target.value)} />
      {isRoot && (
        <FormControlLabel disabled={Boolean(setting)} control={<Checkbox checked={global} onChange={(e) => setGlobal(e.target.checked)} />} label={t('settings_.global')} />
      )}
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button onClick={onDone}>{t('common.cancel')}</Button>
        <Button type="submit" variant="contained" disabled={save.isPending || !key.trim() || !name.trim()}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}
