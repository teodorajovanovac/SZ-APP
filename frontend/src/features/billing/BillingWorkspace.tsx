import { useMemo, useState } from 'react'
import { Alert, Box, Button, Paper, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useInvoiceBatches, useInvoices, usePostInvoiceBatch } from './billingApi'
import { InvoiceBatchForm } from './InvoiceBatchForm'
import type { InvoiceBatch, InvoiceSummary } from './types'

export function BillingWorkspace({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const { t } = useTranslation()
  const [batchPagination, setBatchPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [batchSorting, setBatchSorting] = useState<SortingState>([])
  const [invoicePagination, setInvoicePagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [invoiceSorting, setInvoiceSorting] = useState<SortingState>([])

  const batches = useInvoiceBatches(companyId, batchPagination.pageIndex + 1, batchPagination.pageSize)
  const invoices = useInvoices(companyId, invoicePagination.pageIndex + 1, invoicePagination.pageSize)
  const post = usePostInvoiceBatch(companyId)
  const error = batches.error ?? invoices.error ?? post.error

  const batchColumns = useMemo<ColumnDef<InvoiceBatch>[]>(
    () => [
      { accessorKey: 'periodYYMM', header: t('billing_.columns.period') },
      { accessorKey: 'caption', header: t('billing_.columns.caption') },
      { accessorKey: 'status', header: t('billing_.columns.status') },
      {
        id: 'actions',
        header: t('billing_.columns.action'),
        cell: ({ row }) =>
          canPost && row.original.status === 'Generated' ? (
            <Button size="small" disabled={post.isPending} onClick={() => post.mutate(row.original.id)}>
              {t('billing_.post')}
            </Button>
          ) : null,
      },
    ],
    [canPost, post, t],
  )

  const invoiceColumns = useMemo<ColumnDef<InvoiceSummary>[]>(
    () => [
      { accessorKey: 'sequenceNumber', header: t('billing_.columns.number') },
      { accessorKey: 'partnerName', header: t('billing_.columns.partner') },
      { accessorKey: 'issueDate', header: t('billing_.columns.date') },
      {
        accessorKey: 'invoiceTotal',
        header: t('billing_.columns.total'),
        cell: ({ row }) => formatMoney(row.original.invoiceTotal, row.original.currency),
      },
    ],
    [t],
  )

  return (
    <Stack spacing={3}>
      <Typography component="h1" variant="h1">{t('billing_.title')}</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, t('billing_.dataUnavailable'))}</Alert> : null}
      <Paper sx={{ p: 3 }}><InvoiceBatchForm companyId={companyId} /></Paper>
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
          getRowId={(row) => String(row.id)}
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
          getRowId={(row) => String(row.id)}
        />
      </Box>
    </Stack>
  )
}
