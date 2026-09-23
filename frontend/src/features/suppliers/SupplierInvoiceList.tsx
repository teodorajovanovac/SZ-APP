import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import { Alert, Button, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { SupplierInvoiceForm } from './SupplierInvoiceForm'
import { useSupplierInvoices, type SupplierInvoice } from './supplierApi'

export function SupplierInvoiceList({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [creating, setCreating] = useState(false)
  const query = useSupplierInvoices(companyId, pagination.pageIndex + 1, pagination.pageSize)

  const columns = useMemo<ColumnDef<SupplierInvoice>[]>(
    () => [
      { accessorKey: 'invoiceNo', header: t('suppliers_.columns.number'), enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'periodYYMM', header: t('suppliers_.columns.period'), enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'caption', header: t('suppliers_.columns.caption'), enableSorting: false, meta: { ellipsis: true } },
      // amountRsd shown at 2 decimals like every other money value (no sub-cent precision requirement found).
      { accessorKey: 'amountRsd', header: t('suppliers_.columns.rsd'), enableSorting: false, meta: { numeric: true }, cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'postedAmount', header: t('suppliers_.columns.posted'), enableSorting: false, meta: { numeric: true }, cell: ({ getValue }) => formatMoney(getValue<number>()) },
    ],
    [t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('suppliers_.title')}</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreating(true)}>
          {t('suppliersUi.new')}
        </Button>
      </Stack>
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
        emptyMessage={t('suppliersUi.empty')}
        getRowId={(row) => String(row.id)}
      />

      <FormDialog open={creating} title={t('suppliersUi.newTitle')} onClose={() => setCreating(false)} maxWidth="md">
        {creating ? <SupplierInvoiceForm companyId={companyId} onSaved={() => setCreating(false)} /> : null}
      </FormDialog>
    </Stack>
  )
}
