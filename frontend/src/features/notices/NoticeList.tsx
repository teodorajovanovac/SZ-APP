import { useMemo, useState } from 'react'
import { Alert, Box, Button, Chip, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useNoticeCommand, useNotices } from './noticeApi'
import type { Notice } from './noticeApi'

const statusColor: Record<Notice['deliveryStatus'], 'default' | 'info' | 'success' | 'error'> = {
  Draft: 'default',
  Rendered: 'info',
  Queued: 'info',
  Sent: 'success',
  Failed: 'error',
}

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
      {
        accessorKey: 'deliveryStatus',
        header: t('notices_.columns.status'),
        cell: ({ row }) => (
          <Chip
            size="small"
            color={statusColor[row.original.deliveryStatus] ?? 'default'}
            label={t(`notices_.status.${row.original.deliveryStatus}`, { defaultValue: row.original.deliveryStatus })}
          />
        ),
      },
      {
        id: 'actions',
        header: t('notices_.columns.action'),
        cell: ({ row }) => (
          <>
            {canWrite && row.original.deliveryStatus === 'Draft' ? (
              <Button
                size="small"
                variant="outlined"
                disabled={command.isPending}
                onClick={() => command.mutate({ noticeId: row.original.id, command: 'render' })}
              >
                {t('notices_.render')}
              </Button>
            ) : null}
            {canWrite && row.original.deliveryStatus === 'Rendered' ? (
              <Button
                size="small"
                variant="contained"
                disabled={command.isPending}
                onClick={() => command.mutate({ noticeId: row.original.id, command: 'send' })}
              >
                {t('notices_.send')}
              </Button>
            ) : null}
          </>
        ),
      },
    ],
    [canWrite, command, t],
  )

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('notices_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('notices_.subtitle')}</Typography>
      </Box>
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
        emptyMessage={`${t('notices_.emptyTitle')} — ${t('notices_.emptyBody')}`}
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}
