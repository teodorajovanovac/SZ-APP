import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Autocomplete, Button, IconButton, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState } from '@tanstack/react-table'
import { useMemo, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useCompanyRole } from '../companies/useCompanyRole'
import type { ShortListItem } from '../master-data/types'
import { useShortList } from '../master-data/useShortList'

/** ShortList catalogue (one table for ~30 small lookup lists). Editing is Root-only; the route is RoleGuard'ed. */
export function ShortListsPage() {
  const { t } = useTranslation()
  const { companyId } = useCompanyRole()
  const queryClient = useQueryClient()
  const base = `/api/v1/companies/${companyId}/short-lists`
  const [tableName, setTableName] = useState<string | null>(null)
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 50 })
  const [editing, setEditing] = useState<ShortListItem | 'new'>()
  const [deleting, setDeleting] = useState<ShortListItem>()
  const tables = useQuery({ queryKey: ['short-list-tables', companyId], queryFn: () => apiRequest<string[]>(base) })
  const rows = useShortList(companyId, tableName ?? '')
  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: ['master-data'] })
    void queryClient.invalidateQueries({ queryKey: ['short-list-tables'] })
  }
  const remove = useMutation({
    mutationFn: (item: ShortListItem) => apiRequest<void>(`${base}/${item.id}`, { method: 'DELETE' }),
    onSuccess: () => {
      setDeleting(undefined)
      refresh()
    },
  })
  const all = rows.data ?? []
  const page = all.slice(pagination.pageIndex * pagination.pageSize, (pagination.pageIndex + 1) * pagination.pageSize)

  const columns = useMemo<ColumnDef<ShortListItem>[]>(
    () => [
      { accessorKey: 'indexValue', header: t('shortLists_.indexValue'), enableSorting: false, meta: { numeric: true } },
      { accessorKey: 'caption', header: t('shortLists_.caption'), enableSorting: false },
      { accessorKey: 'shortName', header: t('shortLists_.shortName'), enableSorting: false },
      { accessorKey: 'indexKey', header: t('shortLists_.indexKey'), enableSorting: false },
      { accessorKey: 'indexSort', header: t('shortLists_.indexSort'), enableSorting: false, meta: { numeric: true } },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) => (
          <>
            <Tooltip title={t('ui.edit')}>
              <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}><EditIcon fontSize="small" /></IconButton>
            </Tooltip>
            <Tooltip title={t('shortLists_.delete')}>
              <IconButton size="small" aria-label={t('shortLists_.delete')} onClick={() => setDeleting(row.original)}><DeleteIcon fontSize="small" /></IconButton>
            </Tooltip>
          </>
        ),
      },
    ],
    [t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('shortLists_.title')}</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>{t('shortLists_.new')}</Button>
      </Stack>
      <Autocomplete
        size="small"
        options={tables.data ?? []}
        value={tableName}
        onChange={(_, value) => { setTableName(value); setPagination((p) => ({ ...p, pageIndex: 0 })) }}
        renderInput={(params) => <TextField {...params} label={t('shortLists_.table')} />}
        sx={{ maxWidth: 420 }}
      />
      {(tables.isError || rows.isError) && <Alert severity="error">{t('shortLists_.loadError')}</Alert>}
      {remove.isError && <Alert severity="error">{getErrorMessage(remove.error, t('shortLists_.saveError'))}</Alert>}
      {tableName && (
        <ServerDataTable
          ariaLabel={t('shortLists_.title')}
          rows={page}
          columns={columns}
          rowCount={all.length}
          pagination={pagination}
          sorting={[]}
          onPaginationChange={setPagination}
          onSortingChange={() => undefined}
          isLoading={rows.isLoading}
          emptyMessage={t('shortLists_.empty')}
          getRowId={(row) => String(row.id)}
          minWidth={640}
        />
      )}
      <FormDialog open={Boolean(editing)} title={editing === 'new' ? t('shortLists_.new') : t('shortLists_.editTitle')} onClose={() => setEditing(undefined)}>
        {editing && (
          <ShortListForm
            base={base}
            item={editing === 'new' ? undefined : editing}
            defaultTable={tableName ?? ''}
            onDone={(saved) => {
              setEditing(undefined)
              if (saved) setTableName(saved.tableName)
              refresh()
            }}
          />
        )}
      </FormDialog>
      <ConfirmDialog
        open={Boolean(deleting)}
        title={t('shortLists_.delete')}
        description={t('shortLists_.deleteConfirm', { caption: deleting?.caption })}
        confirmLabel={t('shortLists_.delete')}
        destructive
        pending={remove.isPending}
        onClose={() => setDeleting(undefined)}
        onConfirm={() => deleting && remove.mutate(deleting)}
      />
    </Stack>
  )
}

function ShortListForm({ base, item, defaultTable, onDone }: { base: string; item?: ShortListItem; defaultTable: string; onDone: (saved?: ShortListItem) => void }) {
  const { t } = useTranslation()
  const [form, setForm] = useState({
    tableName: item?.tableName ?? defaultTable,
    caption: item?.caption ?? '',
    shortName: item?.shortName ?? '',
    description: item?.description ?? '',
    indexValue: String(item?.indexValue ?? ''),
    indexSort: String(item?.indexSort ?? ''),
    indexKey: item?.indexKey ?? '',
  })
  const save = useMutation({
    mutationFn: () =>
      apiRequest<ShortListItem>(item ? `${base}/${item.id}` : base, {
        method: item ? 'PUT' : 'POST',
        body: JSON.stringify({
          tableName: form.tableName.trim(), caption: form.caption, shortName: form.shortName || null, description: form.description || null,
          indexValue: Number(form.indexValue), indexSort: Number(form.indexSort || 0), indexKey: form.indexKey || null,
        }),
      }),
    onSuccess: (saved) => onDone(saved),
  })
  const field = (name: keyof typeof form) => ({ value: form[name], onChange: (e: { target: { value: string } }) => setForm({ ...form, [name]: e.target.value }) })
  const submit = (event: FormEvent) => {
    event.preventDefault()
    save.mutate()
  }

  return (
    <Stack component="form" spacing={2} onSubmit={submit}>
      {save.isError && <Alert severity="error">{getErrorMessage(save.error, t('shortLists_.saveError'))}</Alert>}
      <TextField size="small" required label={t('shortLists_.table')} disabled={Boolean(item)} {...field('tableName')} />
      <TextField size="small" required type="number" label={t('shortLists_.indexValue')} disabled={Boolean(item)} helperText={item ? t('shortLists_.lockedHint') : ' '} {...field('indexValue')} />
      <TextField size="small" required label={t('shortLists_.caption')} {...field('caption')} />
      <TextField size="small" label={t('shortLists_.shortName')} slotProps={{ htmlInput: { maxLength: 50 } }} {...field('shortName')} />
      <TextField size="small" label={t('shortLists_.description')} {...field('description')} />
      <TextField size="small" type="number" label={t('shortLists_.indexSort')} {...field('indexSort')} />
      <TextField size="small" label={t('shortLists_.indexKey')} slotProps={{ htmlInput: { maxLength: 50 } }} {...field('indexKey')} />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Button onClick={() => onDone()}>{t('common.cancel')}</Button>
        <Button type="submit" variant="contained" disabled={save.isPending || !form.tableName.trim() || !form.caption.trim() || form.indexValue === ''}>{t('common.save')}</Button>
      </Stack>
    </Stack>
  )
}
