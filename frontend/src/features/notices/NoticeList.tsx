import { useMemo, useState } from 'react'
import { Alert, Box, Button, Chip, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { NoticeCostsPanel } from './NoticeCostsPanel'
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
  const [pendingSend, setPendingSend] = useState<Notice | null>(null)
  const [showCosts, setShowCosts] = useState(false)

  const columns = useMemo<ColumnDef<Notice>[]>(
    () => [
      { accessorKey: 'partnerAccountId', header: t('notices_.columns.partnerAccount') },
      { accessorKey: 'unpaidInvoiceCount', header: t('notices_.columns.invoiceCount') },
      { accessorKey: 'debt', header: t('notices_.columns.debt'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'additionalCosts', header: t('noticeCosts_.column'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'total', header: t('noticeCosts_.total'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
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
                onClick={() => setPendingSend(row.original)}
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
        <Button sx={{ mt: 1 }} variant="outlined" aria-expanded={showCosts} onClick={() => setShowCosts((v) => !v)}>
          {t('noticeCosts_.title')}
        </Button>
      </Box>
      {showCosts ? <NoticeCostsPanel companyId={companyId} /> : null}
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
      <ConfirmDialog
        open={pendingSend !== null}
        title={t('notices_.sendConfirmTitle')}
        description={t('notices_.sendConfirmBody', {
          account: pendingSend?.partnerAccountId,
          debt: formatMoney(pendingSend?.total ?? 0),
        })}
        confirmLabel={t('notices_.send')}
        pending={command.isPending}
        onClose={() => setPendingSend(null)}
        onConfirm={() => {
          if (pendingSend) command.mutate({ noticeId: pendingSend.id, command: 'send' })
          setPendingSend(null)
        }}
      />
    </Stack>
  )
}
