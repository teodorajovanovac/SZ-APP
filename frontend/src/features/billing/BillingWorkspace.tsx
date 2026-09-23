import { useMemo, useState } from 'react'
import { Alert, Box, Button, Paper, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useInvoiceBatches, useInvoices, usePostInvoiceBatch } from './billingApi'
import { InvoiceBatchForm } from './InvoiceBatchForm'
import type { InvoiceBatch, InvoiceSummary } from './types'

export function BillingWorkspace({ companyId, canPost }: { companyId: number; canPost: boolean }) {
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
      { accessorKey: 'periodYYMM', header: 'Period' },
      { accessorKey: 'caption', header: 'Naziv' },
      { accessorKey: 'status', header: 'Status' },
      {
        id: 'actions',
        header: 'Akcija',
        cell: ({ row }) =>
          canPost && row.original.status === 'Generated' ? (
            <Button size="small" disabled={post.isPending} onClick={() => post.mutate(row.original.id)}>
              Knjiži
            </Button>
          ) : null,
      },
    ],
    [canPost, post],
  )

  const invoiceColumns = useMemo<ColumnDef<InvoiceSummary>[]>(
    () => [
      { accessorKey: 'sequenceNumber', header: 'Broj' },
      { accessorKey: 'partnerName', header: 'Partner' },
      { accessorKey: 'issueDate', header: 'Datum' },
      {
        accessorKey: 'invoiceTotal',
        header: 'Ukupno',
        cell: ({ row }) => formatMoney(row.original.invoiceTotal, row.original.currency),
      },
    ],
    [],
  )

  return (
    <Stack spacing={3}>
      <Typography component="h1" variant="h1">Fakturisanje</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, 'Podaci fakturisanja nisu dostupni.')}</Alert> : null}
      <Paper sx={{ p: 3 }}><InvoiceBatchForm companyId={companyId} /></Paper>
      <Box>
        <Typography component="h2" variant="h6" sx={{ mb: 1 }}>Serije računa</Typography>
        <ServerDataTable
          ariaLabel="Serije računa"
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
        <Typography component="h2" variant="h6" sx={{ mb: 1 }}>Računi</Typography>
        <ServerDataTable
          ariaLabel="Računi"
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
