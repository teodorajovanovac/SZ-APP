import { useMemo, useState } from 'react'
import { Alert, CircularProgress, Stack, TextField, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { usePartners } from '../useMasterData'
import type { Partner } from '../types'

interface PartnerListProps {
  companyId: number
  onSelect?: (partner: Partner) => void
}

export function PartnerList({ companyId, onSelect }: PartnerListProps) {
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([{ id: 'shortName', desc: false }])
  const sort = sorting[0]
  const partners = usePartners(companyId, {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search,
    sortBy: sort?.id,
    descending: sort?.desc,
  })

  const columns = useMemo<ColumnDef<Partner>[]>(
    () => [
      {
        accessorKey: 'shortName',
        header: 'Kratak naziv',
        cell: ({ row, getValue }) => (
          <button type="button" className="MuiButtonBase-root" onClick={() => onSelect?.(row.original)}>
            {String(getValue())}
          </button>
        ),
      },
      { accessorKey: 'name', header: 'Pun naziv' },
      { accessorKey: 'taxNumber', header: 'PIB' },
      { accessorKey: 'registrationNumber', header: 'Matični broj', enableSorting: false },
      { accessorKey: 'language', header: 'Jezik', enableSorting: false },
    ],
    [onSelect],
  )

  return (
    <Stack spacing={2}>
      <Typography variant="h5" component="h2">Partneri</Typography>
      <TextField
        label="Pretraga po nazivu ili PIB-u"
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
          setPagination((value) => ({ ...value, pageIndex: 0 }))
        }}
        size="small"
      />
      {partners.isLoading && <CircularProgress aria-label="Učitavanje partnera" />}
      {partners.isError && <Alert severity="error">Partneri nisu mogli da se učitaju.</Alert>}
      <ServerDataTable
        ariaLabel="Partneri"
        rows={partners.data?.items ?? []}
        columns={columns}
        rowCount={partners.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}

