import { useMemo, useState } from 'react'
import { Alert, Button, Stack, TextField, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useAddresses } from '../useMasterData'
import type { Address } from '../types'

interface AddressListProps {
  companyId: number
  onSelect?: (address: Address) => void
}

export function AddressList({ companyId, onSelect }: AddressListProps) {
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
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
          <Button variant="text" size="small" onClick={() => onSelect?.(row.original)}>
            {String(getValue())}
          </Button>
        ),
      },
      { accessorKey: 'postalCode', header: 'Poštanski broj', enableSorting: false },
      { accessorKey: 'city', header: 'Grad' },
      { accessorKey: 'countryCode', header: 'Država', enableSorting: false },
    ],
    [onSelect],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">Adrese</Typography>
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
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}

