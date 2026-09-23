import { useMemo, useState } from 'react'
import { Alert, Box, Button, Link, Paper, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useInvoiceBatches, useInvoices, usePostInvoiceBatch } from './billingApi'
import { InvoiceBatchForm } from './InvoiceBatchForm'
import { InvoiceDetail } from './InvoiceDetail'
import type { InvoiceBatch, InvoiceSummary } from './types'

export function BillingWorkspace({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const { t } = useTranslation()
  const [batchPagination, setBatchPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [batchSorting, setBatchSorting] = useState<SortingState>([])
  const [invoicePagination, setInvoicePagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [invoiceSorting, setInvoiceSorting] = useState<SortingState>([])
  const [openInvoiceId, setOpenInvoiceId] = useState<number>()

  const batches = useInvoiceBatches(companyId, batchPagination.pageIndex + 1, batchPagination.pageSize)
  const invoices = useInvoices(companyId, invoicePagination.pageIndex + 1, invoicePagination.pageSize)
  const post = usePostInvoiceBatch(companyId)
  const error = batches.error ?? invoices.error ?? post.error

  // The list endpoints take page/pageSize only — no sortBy — so no column advertises sorting.
  const batchColumns = useMemo<ColumnDef<InvoiceBatch>[]>(
    () => [
      { accessorKey: 'periodYYMM', header: t('billing_.columns.period'), enableSorting: false, meta: { numeric: true } },
      { accessorKey: 'caption', header: t('billing_.columns.caption'), enableSorting: false, meta: { ellipsis: true } },
      { accessorKey: 'status', header: t('billing_.columns.status'), enableSorting: false },
      {
        id: 'actions',
        header: t('billing_.columns.action'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) =>
          canPost && row.original.status === 'Generated' ? (
            <Button size="small" variant="outlined" disabled={post.isPending} onClick={() => post.mutate(row.original.id)}>
              {t('billing_.post')}
            </Button>
          ) : null,
      },
    ],
    [canPost, post, t],
  )

  const invoiceColumns = useMemo<ColumnDef<InvoiceSummary>[]>(
    () => [
      {
        accessorKey: 'sequenceNumber',
        header: t('billing_.columns.number'),
        enableSorting: false,
        cell: ({ row, getValue }) => (
          <Link component="button" type="button" underline="hover" onClick={() => setOpenInvoiceId(row.original.id)}>
            {String(getValue())}
          </Link>
        ),
      },
      { accessorKey: 'partnerName', header: t('billing_.columns.partner'), enableSorting: false, meta: { ellipsis: true } },
      { accessorKey: 'issueDate', header: t('billing_.columns.date'), enableSorting: false },
      {
        accessorKey: 'invoiceTotal',
        header: t('billing_.columns.total'),
        enableSorting: false,
        meta: { numeric: true },
        cell: ({ row }) => formatMoney(row.original.invoiceTotal, row.original.currency),
      },
    ],
    [t],
  )

  return (
    <Stack spacing={3}>
      <Typography component="h1" variant="h1">{t('billing_.title')}</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, t('billing_.dataUnavailable'))}</Alert> : null}
      <Paper variant="outlined" sx={{ p: 3 }}><InvoiceBatchForm companyId={companyId} /></Paper>
      <Box>
        <Typography component="h2" variant="h6" sx={{ mb: 1 }}>{t('billing_.batchesTitle')}</Typography>
        <ServerDataTable
          ariaLabel={t('billing_.batchesTitle')}
          rows={batches.data?.items ?? []}
          columns={batchColumns}
          rowCount={batches.data?.totalCount ?? 0}
          pagination={batchPagination}
          sorting={batchSorting}
          onPaginationChange={setBatchPagination}
          onSortingChange={setBatchSorting}
          isLoading={batches.isLoading}
          emptyMessage={t('billingUi.batchesEmpty')}
          getRowId={(row) => String(row.id)}
          minWidth={560}
        />
      </Box>
      <Box>
        <Typography component="h2" variant="h6" sx={{ mb: 1 }}>{t('billing_.invoicesTitle')}</Typography>
        <ServerDataTable
          ariaLabel={t('billing_.invoicesTitle')}
          rows={invoices.data?.items ?? []}
          columns={invoiceColumns}
          rowCount={invoices.data?.totalCount ?? 0}
          pagination={invoicePagination}
          sorting={invoiceSorting}
          onPaginationChange={setInvoicePagination}
          onSortingChange={setInvoiceSorting}
          isLoading={invoices.isLoading}
          emptyMessage={t('billingUi.invoicesEmpty')}
          getRowId={(row) => String(row.id)}
          minWidth={560}
        />
      </Box>

      <FormDialog
        open={openInvoiceId !== undefined}
        title={t('billing_.invoicesTitle')}
        onClose={() => setOpenInvoiceId(undefined)}
      >
        {openInvoiceId !== undefined ? <InvoiceDetail companyId={companyId} invoiceId={openInvoiceId} /> : null}
      </FormDialog>
    </Stack>
  )
}
