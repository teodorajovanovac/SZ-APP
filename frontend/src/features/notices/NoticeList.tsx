import { useMemo, useState } from 'react'
import { Alert, Button, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useNoticeCommand, useNotices } from './noticeApi'
import type { Notice } from './noticeApi'

export function NoticeList({ companyId, canWrite }: { companyId: number; canWrite: boolean }) {
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const query = useNotices(companyId, pagination.pageIndex + 1, pagination.pageSize)
  const command = useNoticeCommand(companyId)
  const error = query.error ?? command.error

  const columns = useMemo<ColumnDef<Notice>[]>(
    () => [
      { accessorKey: 'partnerAccountId', header: 'Partner konto' },
      { accessorKey: 'unpaidInvoiceCount', header: 'Broj računa' },
      { accessorKey: 'total', header: 'Dug', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'deliveryStatus', header: 'Status' },
      {
        id: 'actions',
        header: 'Akcija',
        cell: ({ row }) => (
          <>
            {canWrite && row.original.deliveryStatus === 'Draft' ? (
              <Button size="small" onClick={() => command.mutate({ noticeId: row.original.id, command: 'render' })}>Renderuj</Button>
            ) : null}
            {canWrite && row.original.deliveryStatus === 'Rendered' ? (
              <Button size="small" onClick={() => command.mutate({ noticeId: row.original.id, command: 'send' })}>Pošalji</Button>
            ) : null}
          </>
        ),
      },
    ],
    [canWrite, command],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">Opomene</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, 'Obrada opomene nije uspela.')}</Alert> : null}
      <ServerDataTable
        ariaLabel="Opomene"
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
