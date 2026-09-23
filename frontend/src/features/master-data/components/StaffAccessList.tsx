import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, Link, Stack, TextField, Tooltip, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { FormDialog } from '../../../shared/components/FormDialog'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useStaffAccess } from '../useMasterData'
import { StaffAccessForm } from './StaffAccessForm'
import type { StaffAccess } from '../types'

interface StaffAccessListProps {
  companyId: number
  onSelect?: (access: StaffAccess) => void
}

export function StaffAccessList({ companyId, onSelect }: StaffAccessListProps) {
  const { t } = useTranslation()
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [editing, setEditing] = useState<StaffAccess | 'new'>()
  const result = useStaffAccess(companyId, {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search,
    descending: sorting[0]?.desc,
  })
  const columns = useMemo<ColumnDef<StaffAccess>[]>(
    () => [
      {
        accessorKey: 'staffEmail',
        header: 'Korisnik',
        cell: ({ row, getValue }) => (
          <Link
            component="button"
            type="button"
            underline="hover"
            textAlign="left"
            onClick={() => (onSelect ? onSelect(row.original) : setEditing(row.original))}
          >
            {String(getValue())}
          </Link>
        ),
      },
      { accessorKey: 'staffRole', header: 'Uloga', enableSorting: false },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) => (
          <Tooltip title={t('ui.edit')}>
            <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}>
              <EditIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [onSelect, t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">Pristup zaposlenih</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
          {t('staffAccess_.new')}
        </Button>
      </Stack>
      <TextField label="Pretraga po email adresi" value={search} onChange={(event) => setSearch(event.target.value)} size="small" />
      {result.isError && <Alert severity="error">Pristupi nisu mogli da se učitaju.</Alert>}
      <ServerDataTable
        ariaLabel="Pristup zaposlenih"
        rows={result.data?.items ?? []}
        columns={columns}
        rowCount={result.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={result.isLoading}
        emptyMessage={search ? undefined : t('staffAccess_.empty')}
        getRowId={(row) => String(row.id)}
        minWidth={480}
      />

      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('staffAccess_.newTitle') : t('staffAccess_.editTitle')}
        onClose={() => setEditing(undefined)}
        maxWidth="xs"
      >
        {editing ? (
          <StaffAccessForm
            companyId={companyId}
            access={editing === 'new' ? undefined : editing}
            onSaved={() => setEditing(undefined)}
            onCancel={() => setEditing(undefined)}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
