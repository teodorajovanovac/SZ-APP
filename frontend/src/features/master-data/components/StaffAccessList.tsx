import { useMemo, useState } from 'react'
import { Alert, Button, Stack, TextField, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useStaffAccess } from '../useMasterData'
import type { StaffAccess } from '../types'

interface StaffAccessListProps {
  companyId: number
  onSelect?: (access: StaffAccess) => void
}

export function StaffAccessList({ companyId, onSelect }: StaffAccessListProps) {
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
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
          <Button variant="text" size="small" onClick={() => onSelect?.(row.original)}>
            {String(getValue())}
          </Button>
        ),
      },
      { accessorKey: 'staffRole', header: 'Uloga', enableSorting: false },
    ],
    [onSelect],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">Pristup zaposlenih</Typography>
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
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}

