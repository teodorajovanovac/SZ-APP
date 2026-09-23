import { useMemo, useState } from 'react'
import { Alert, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useSupplierInvoices, type SupplierInvoice } from './supplierApi'

export function SupplierInvoiceList({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const query = useSupplierInvoices(companyId, pagination.pageIndex + 1, pagination.pageSize)

  const columns = useMemo<ColumnDef<SupplierInvoice>[]>(
    () => [
      { accessorKey: 'invoiceNo', header: t('suppliers_.columns.number') },
      { accessorKey: 'periodYYMM', header: t('suppliers_.columns.period') },
      { accessorKey: 'caption', header: t('suppliers_.columns.caption') },
      // amountRsd shown at 2 decimals like every other money value (no sub-cent precision requirement found).
      { accessorKey: 'amountRsd', header: t('suppliers_.columns.rsd'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'postedAmount', header: t('suppliers_.columns.posted'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
    ],
    [t],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">{t('suppliers_.title')}</Typography>
      {query.error ? <Alert severity="error">{getErrorMessage(query.error, t('suppliers_.notLoaded'))}</Alert> : null}
      <ServerDataTable
        ariaLabel={t('suppliers_.title')}
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
