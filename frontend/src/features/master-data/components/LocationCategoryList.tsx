import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, Link, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../api/problemDetails'
import { useAuth } from '../../auth/useAuth'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { FormDialog } from '../../../shared/components/FormDialog'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { masterDataApi } from '../masterDataApi'
import { invalidateLocationCategories, useLocationCategories } from '../useMasterData'
import { LocationCategoryForm } from './LocationCategoryForm'
import type { LocationCategory } from '../types'

// The GET returns the whole (small) catalogue, so search and paging are client-side.
export function LocationCategoryList({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const isRoot = user?.roles.includes('Root') ?? false
  const queryClient = useQueryClient()
  const categories = useLocationCategories(companyId)
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [editing, setEditing] = useState<LocationCategory | 'new'>()
  const [deleting, setDeleting] = useState<LocationCategory>()
  const remove = useMutation({
    mutationFn: (value: LocationCategory) => masterDataApi.locationCategories.remove(companyId, value.id),
    onSuccess: () => {
      setDeleting(undefined)
      void invalidateLocationCategories(queryClient)
    },
  })

  const all = useMemo(() => categories.data ?? [], [categories.data])
  const names = useMemo(() => new Map(all.map((item) => [item.id, item.name])), [all])
  const filtered = useMemo(() => {
    const needle = search.trim().toLowerCase()
    return all.filter((item) => !needle || item.name.toLowerCase().includes(needle))
  }, [all, search])
  const page = filtered.slice(pagination.pageIndex * pagination.pageSize, (pagination.pageIndex + 1) * pagination.pageSize)

  const columns = useMemo<ColumnDef<LocationCategory>[]>(
    () => [
      { accessorKey: 'id', header: 'Id', enableSorting: false, meta: { numeric: true } },
      {
        accessorKey: 'name',
        header: t('locations_.name'),
        enableSorting: false,
        cell: ({ row }) =>
          isRoot ? (
            <Link component="button" type="button" underline="hover" textAlign="left" onClick={() => setEditing(row.original)}>
              {row.original.name}
            </Link>
          ) : (
            row.original.name
          ),
      },
      { accessorKey: 'parentId', header: t('locations_.parent'), enableSorting: false, cell: ({ getValue }) => names.get(getValue() as number) ?? '—' },
      { accessorKey: 'sortIndex', header: t('locations_.sortIndex'), enableSorting: false, meta: { numeric: true } },
      ...(isRoot
        ? [
            {
              id: 'actions',
              header: t('ui.actions'),
              enableSorting: false,
              meta: { align: 'right' as const },
              cell: ({ row }: { row: { original: LocationCategory } }) => (
                <>
                  <Tooltip title={t('ui.edit')}>
                    <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={t('locations_.delete')}>
                    <IconButton size="small" aria-label={t('locations_.delete')} onClick={() => setDeleting(row.original)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </>
              ),
            },
          ]
        : []),
    ],
    [isRoot, names, t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('locations_.title')}</Typography>
        {isRoot && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
            {t('locations_.new')}
          </Button>
        )}
      </Stack>
      {!isRoot && <Alert severity="info">{t('locations_.rootOnly')}</Alert>}
      <TextField
        label={t('locations_.search')}
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
          setPagination((value) => ({ ...value, pageIndex: 0 }))
        }}
        size="small"
      />
      {categories.isError && <Alert severity="error">{t('locations_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('locations_.title')}
        rows={page}
        columns={columns}
        rowCount={filtered.length}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={categories.isLoading}
        getRowId={(row) => String(row.id)}
      />
      <FormDialog open={Boolean(editing)} title={editing === 'new' ? t('locations_.new') : t('locations_.editTitle')} onClose={() => setEditing(undefined)}>
        {editing ? (
          <LocationCategoryForm companyId={companyId} category={editing === 'new' ? undefined : editing} all={all} onSaved={() => setEditing(undefined)} />
        ) : null}
      </FormDialog>
      <ConfirmDialog
        open={Boolean(deleting)}
        title={t('locations_.deleteTitle')}
        description={
          <>
            {t('locations_.deleteBody', { name: deleting?.name ?? '' })}
            {remove.isError && <Alert severity="error" sx={{ mt: 2 }}>{getErrorMessage(remove.error, t('locations_.deleteError'))}</Alert>}
          </>
        }
        confirmLabel={t('locations_.delete')}
        destructive
        pending={remove.isPending}
        onClose={() => {
          setDeleting(undefined)
          remove.reset()
        }}
        onConfirm={() => deleting && remove.mutate(deleting)}
      />
    </Stack>
  )
}
