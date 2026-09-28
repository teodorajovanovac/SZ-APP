import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, Link, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'
import { getErrorMessage } from '../../../api/problemDetails'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { FormDialog } from '../../../shared/components/FormDialog'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { masterDataApi } from '../masterDataApi'
import { useBuildingEntrances } from '../useMasterData'
import { BuildingEntranceForm } from './BuildingEntranceForm'
import type { BuildingEntrance } from '../types'

const display = (value: unknown) => (value === null || value === undefined || value === '' ? '—' : String(value))

export function BuildingEntranceList({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  // Backend orders by SortIndex only; the header toggles direction.
  const [sorting, setSorting] = useState<SortingState>([{ id: 'sortIndex', desc: false }])
  const [editing, setEditing] = useState<BuildingEntrance | 'new'>()
  const [deleting, setDeleting] = useState<BuildingEntrance>()
  const entrances = useBuildingEntrances(companyId, {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search,
    descending: sorting[0]?.desc,
  })
  const remove = useMutation({
    mutationFn: (value: BuildingEntrance) => masterDataApi.buildingEntrances.remove(companyId, value),
    onSuccess: () => {
      setDeleting(undefined)
      void queryClient.invalidateQueries({ queryKey: ['master-data', companyId, 'building-entrances'] })
    },
  })

  const columns = useMemo<ColumnDef<BuildingEntrance>[]>(
    () => [
      { accessorKey: 'sortIndex', header: t('entrances_.sortIndex'), meta: { numeric: true }, cell: ({ getValue }) => display(getValue()) },
      {
        accessorKey: 'buildingName',
        header: t('entrances_.buildingName'),
        enableSorting: false,
        cell: ({ row }) => (
          <Link component={RouterLink} to={`/building-entrances/${companyId}/${row.original.id}`} underline="hover">
            {row.original.buildingName || `#${row.original.id}`}
          </Link>
        ),
      },
      { accessorKey: 'entranceName', header: t('entrances_.entranceName'), enableSorting: false, cell: ({ getValue }) => display(getValue()) },
      { accessorKey: 'buildingLabel', header: t('entrances_.buildingLabel'), enableSorting: false, cell: ({ getValue }) => display(getValue()) },
      { accessorKey: 'description', header: t('entrances_.description'), enableSorting: false, meta: { ellipsis: true }, cell: ({ getValue }) => display(getValue()) },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) => (
          <>
            <Tooltip title={t('ui.edit')}>
              <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}>
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title={t('entrances_.delete')}>
              <IconButton size="small" aria-label={t('entrances_.delete')} onClick={() => setDeleting(row.original)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </>
        ),
      },
    ],
    [companyId, t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('entrances_.title')}</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
          {t('entrances_.new')}
        </Button>
      </Stack>
      <TextField
        label={t('entrances_.search')}
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
          setPagination((value) => ({ ...value, pageIndex: 0 }))
        }}
        size="small"
      />
      {entrances.isError && <Alert severity="error">{t('entrances_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('entrances_.title')}
        rows={entrances.data?.items ?? []}
        columns={columns}
        rowCount={entrances.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={entrances.isLoading}
        emptyMessage={search ? undefined : t('entrances_.empty')}
        getRowId={(row) => String(row.id)}
      />
      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('entrances_.new') : t('entrances_.editTitle')}
        onClose={() => setEditing(undefined)}
      >
        {editing ? (
          <BuildingEntranceForm companyId={companyId} entrance={editing === 'new' ? undefined : editing} onSaved={() => setEditing(undefined)} />
        ) : null}
      </FormDialog>
      <ConfirmDialog
        open={Boolean(deleting)}
        title={t('entrances_.deleteTitle')}
        description={
          <>
            {t('entrances_.deleteBody', { name: [deleting?.buildingName, deleting?.entranceName].filter(Boolean).join(' / ') || `#${deleting?.id}` })}
            {remove.isError && <Alert severity="error" sx={{ mt: 2 }}>{getErrorMessage(remove.error, t('entrances_.deleteError'))}</Alert>}
          </>
        }
        confirmLabel={t('entrances_.delete')}
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
