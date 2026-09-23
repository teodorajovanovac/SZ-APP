import { useMemo, useState } from 'react'
import { Alert, Button, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useNoticeCommand, useNotices } from './noticeApi'
import type { Notice } from './noticeApi'

export function NoticeList({ companyId, canWrite }: { companyId: number; canWrite: boolean }) {
  const { t } = useTranslation()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const query = useNotices(companyId, pagination.pageIndex + 1, pagination.pageSize)
  const command = useNoticeCommand(companyId)
  const error = query.error ?? command.error

  const columns = useMemo<ColumnDef<Notice>[]>(
    () => [
      { accessorKey: 'partnerAccountId', header: t('notices_.columns.partnerAccount') },
      { accessorKey: 'unpaidInvoiceCount', header: t('notices_.columns.invoiceCount') },
      { accessorKey: 'total', header: t('notices_.columns.debt'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'deliveryStatus', header: t('notices_.columns.status') },
      {
        id: 'actions',
        header: t('notices_.columns.action'),
        cell: ({ row }) => (
          <>
            {canWrite && row.original.deliveryStatus === 'Draft' ? (
              <Button size="small" onClick={() => command.mutate({ noticeId: row.original.id, command: 'render' })}>{t('notices_.render')}</Button>
            ) : null}
            {canWrite && row.original.deliveryStatus === 'Rendered' ? (
              <Button size="small" onClick={() => command.mutate({ noticeId: row.original.id, command: 'send' })}>{t('notices_.send')}</Button>
            ) : null}
          </>
        ),
      },
    ],
    [canWrite, command, t],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">{t('notices_.title')}</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, t('notices_.processingFailed'))}</Alert> : null}
      <ServerDataTable
        ariaLabel={t('notices_.title')}
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
