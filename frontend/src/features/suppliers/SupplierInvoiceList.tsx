import { useMemo, useState } from 'react'
import { Alert, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useSupplierInvoices, type SupplierInvoice } from './supplierApi'

export function SupplierInvoiceList({ companyId }: { companyId: number }) {
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const query = useSupplierInvoices(companyId, pagination.pageIndex + 1, pagination.pageSize)

  const columns = useMemo<ColumnDef<SupplierInvoice>[]>(
    () => [
      { accessorKey: 'invoiceNo', header: 'R.br.' },
      { accessorKey: 'periodYYMM', header: 'Period' },
      { accessorKey: 'caption', header: 'Naziv' },
      // amountRsd shown at 2 decimals like every other money value (no sub-cent precision requirement found).
      { accessorKey: 'amountRsd', header: 'RSD', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'postedAmount', header: 'Raspoređeno', cell: ({ getValue }) => formatMoney(getValue<number>()) },
    ],
    [],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">Dobavljački računi</Typography>
      {query.error ? <Alert severity="error">{getErrorMessage(query.error, 'Dobavljački računi nisu učitani.')}</Alert> : null}
      <ServerDataTable
        ariaLabel="Dobavljački računi"
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={query.isLoading}
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}
