import { useMemo, useState } from 'react'
import { Alert, Box, Button, Chip, MenuItem, Select, Stack, Typography } from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
import { FormDialog } from '../../shared/components/FormDialog'
import { MoneyField } from '../../shared/components/MoneyField'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useShortList } from '../master-data/useShortList'
import { NoticeCostsPanel } from './NoticeCostsPanel'
import { NoticeDocumentsToolbar, NoticeRowDocuments } from './NoticeDocumentsToolbar'
import { useCreateNoticeBatch, useGenerateNotices, useNoticeCommand, useNoticeTemplates, useNotices } from './noticeApi'
import type { CreateNoticeBatch, Notice } from './noticeApi'

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
  const [showNewBatch, setShowNewBatch] = useState(false)

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
        id: 'documents',
        header: t('noticeDocs_.lawsuit'),
        cell: ({ row }) => <NoticeRowDocuments companyId={companyId} notice={row.original} canWrite={canWrite} />,
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
    [canWrite, command, companyId, t],
  )

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('notices_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('notices_.subtitle')}</Typography>
        <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
          <Button variant="outlined" aria-expanded={showCosts} onClick={() => setShowCosts((v) => !v)}>
            {t('noticeCosts_.title')}
          </Button>
          {canWrite ? (
            <Button variant="contained" onClick={() => setShowNewBatch(true)}>
              {t('notices_.newBatch')}
            </Button>
          ) : null}
        </Stack>
      </Box>
      {showCosts ? <NoticeCostsPanel companyId={companyId} /> : null}
      {/* Server enforces CompanyPost for cost posting; hidden only for read-only users here. */}
      <NoticeDocumentsToolbar companyId={companyId} canWrite={canWrite} canPost={canWrite} />
      <FormDialog open={showNewBatch} title={t('notices_.newBatch')} onClose={() => setShowNewBatch(false)}>
        {showNewBatch ? <NewNoticeBatchForm companyId={companyId} onDone={() => setShowNewBatch(false)} /> : null}
      </FormDialog>
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

const today = () => new Date().toISOString().slice(0, 10)

/** FIN-12: only cutoffs + thresholds go to the server; debt/lines are computed from the ledger. */
function NewNoticeBatchForm({ companyId, onDone }: { companyId: number; onDone: () => void }) {
  const { t } = useTranslation()
  const templates = useNoticeTemplates(companyId)
  const noticeTypes = useShortList(companyId, 'NoticeType')
  const createBatch = useCreateNoticeBatch(companyId)
  const generate = useGenerateNotices(companyId)
  const { control, handleSubmit, watch, setValue } = useForm<CreateNoticeBatch>({
    defaultValues: {
      title: '', date: today(),
      minUnpaidInvoiceCount: 3, debtTolerance: 1, debtToleranceByMonth: 1,
      noticeTemplateId: 0, noticeTypeId: 0,
      upToClaimDate: today(), upToPaymentDate: today(),
      invoiceBatchId: null, customCaptionOnSlip: null,
    },
  })
  const noticeTemplateId = watch('noticeTemplateId')
  const noticeTypeId = watch('noticeTypeId')
  const pending = createBatch.isPending || generate.isPending
  const error = createBatch.error ?? generate.error

  const submit = async (value: CreateNoticeBatch) => {
    const batch = await createBatch.mutateAsync(value)
    await generate.mutateAsync({ batchId: batch.id, confirm: false })
    onDone()
  }

  return (
    <Stack component="form" spacing={2} noValidate onSubmit={handleSubmit(submit)}>
      {error ? (
        <Alert severity="error">
          {getErrorMessage(error, t('notices_.createFailed'))}
        </Alert>
      ) : null}
      <ControlledTextField control={control} name="title" label={t('notices_.form.title')} required />
      <ControlledTextField control={control} name="date" label={t('notices_.form.date')} type="date" required />
      <Select
        displayEmpty
        size="small"
        value={noticeTemplateId ? String(noticeTemplateId) : ''}
        onChange={(event: SelectChangeEvent) => setValue('noticeTemplateId', Number(event.target.value))}
      >
        <MenuItem value="" disabled>{t('notices_.form.template')}</MenuItem>
        {(templates.data ?? []).filter((x) => x.isActive).map((x) => (
          <MenuItem key={x.id} value={String(x.id)}>{x.name}</MenuItem>
        ))}
      </Select>
      <Select
        displayEmpty
        size="small"
        value={noticeTypeId ? String(noticeTypeId) : ''}
        onChange={(event: SelectChangeEvent) => setValue('noticeTypeId', Number(event.target.value))}
      >
        <MenuItem value="" disabled>{t('notices_.form.type')}</MenuItem>
        {(noticeTypes.data ?? []).map((x) => (
          <MenuItem key={x.id} value={String(x.id)}>{x.caption}</MenuItem>
        ))}
      </Select>
      <ControlledTextField control={control} name="upToClaimDate" label={t('notices_.form.claimDate')} type="date" required />
      <ControlledTextField control={control} name="upToPaymentDate" label={t('notices_.form.paymentDate')} type="date" required />
      <ControlledTextField control={control} name="minUnpaidInvoiceCount" label={t('notices_.form.minBnr')} type="number" required />
      <MoneyField control={control} name="debtTolerance" label={t('notices_.form.debtTolerance')} required />
      <MoneyField control={control} name="debtToleranceByMonth" label={t('notices_.form.debtToleranceByMonth')} required />
      <Stack direction="row" justifyContent="flex-end">
        <Button type="submit" variant="contained" disabled={pending || !noticeTemplateId || !noticeTypeId}>
          {t('notices_.createAndGenerate')}
        </Button>
      </Stack>
    </Stack>
  )
}
