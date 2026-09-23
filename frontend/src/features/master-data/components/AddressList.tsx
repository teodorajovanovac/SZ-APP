import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, Link, Stack, TextField, Tooltip, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { FormDialog } from '../../../shared/components/FormDialog'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useAddresses } from '../useMasterData'
import { AddressForm } from './AddressForm'
import type { Address } from '../types'

interface AddressListProps {
  companyId: number
  onSelect?: (address: Address) => void
}

export function AddressList({ companyId, onSelect }: AddressListProps) {
  const { t } = useTranslation()
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [editing, setEditing] = useState<Address | 'new'>()
  const result = useAddresses(companyId, {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search,
    descending: sorting[0]?.desc,
  })
  const columns = useMemo<ColumnDef<Address>[]>(
    () => [
      {
        accessorKey: 'streetAddress',
        header: 'Adresa',
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
      { accessorKey: 'postalCode', header: 'Poštanski broj', enableSorting: false, meta: { numeric: true } },
      { accessorKey: 'city', header: 'Grad' },
      { accessorKey: 'countryCode', header: 'Država', enableSorting: false },
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
        <Typography component="h1" variant="h1">Adrese</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
          {t('addresses_.new')}
        </Button>
      </Stack>
      <Alert severity="info">Globalni adresar je dostupan samo Root korisniku dok se ne poveže tenant ownership.</Alert>
      <TextField label="Pretraga adresa" value={search} onChange={(event) => setSearch(event.target.value)} size="small" />
      {result.isError && <Alert severity="error">Adrese nisu mogle da se učitaju.</Alert>}
      <ServerDataTable
        ariaLabel="Adrese"
        rows={result.data?.items ?? []}
        columns={columns}
        rowCount={result.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={result.isLoading}
        emptyMessage={search ? undefined : t('addresses_.empty')}
        getRowId={(row) => String(row.id)}
      />

      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('addresses_.newTitle') : t('addresses_.editTitle')}
        onClose={() => setEditing(undefined)}
      >
        {editing ? (
          <AddressForm
            companyId={companyId}
            address={editing === 'new' ? undefined : editing}
            onSaved={() => setEditing(undefined)}
            onCancel={() => setEditing(undefined)}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
