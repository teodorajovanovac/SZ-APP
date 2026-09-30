import { useState } from 'react'
import { Alert, Button, Checkbox, MenuItem, Select, Stack, TextField, Typography } from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import {
  downloadLawsuitCsv, downloadNoticePdf, downloadNoticeZip, useNoticeBatchAction, useNoticeBatches, useSetNoticeLawsuit,
} from './noticeApi'
import type { Notice, NoticeEmailPreview, NoticeEmailSendResult } from './noticeApi'

type Pending = { kind: 'email'; preview: NoticeEmailPreview } | { kind: 'post' } | { kind: 'cancel' } | null

/** GAP-09 batch documents (ZIP, bulk email), optional notice-cost posting, GAP-20 lawsuit export. */
export function NoticeDocumentsToolbar({ companyId, canWrite, canPost }: { companyId: number; canWrite: boolean; canPost: boolean }) {
  const { t } = useTranslation()
  const batches = useNoticeBatches(companyId)
  const action = useNoticeBatchAction(companyId)
  const [batchId, setBatchId] = useState(0)
  const [pending, setPending] = useState<Pending>(null)
  const [message, setMessage] = useState<string | null>(null)
  const batch = batches.data?.find((x) => x.id === batchId)
  const fail = () => setMessage(t('noticeDocs_.failed'))

  const previewEmails = async () => {
    const preview = (await action.mutateAsync({ batchId, action: 'emails/preview' })) as NoticeEmailPreview
    setPending({ kind: 'email', preview })
  }

  const confirm = async () => {
    const current = pending
    setPending(null)
    if (!current) return
    if (current.kind === 'email') {
      const result = (await action.mutateAsync({ batchId, action: 'emails/send' })) as NoticeEmailSendResult
      setMessage(t('noticeDocs_.sent', { enqueued: result.enqueued, skipped: result.skipped }))
    } else {
      await action.mutateAsync({ batchId, action: current.kind === 'post' ? 'costs/post' : 'costs/cancel' })
    }
  }

  return (
    <Stack spacing={1}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} alignItems={{ sm: 'center' }} flexWrap="wrap" useFlexGap>
        <Select
          displayEmpty
          size="small"
          aria-label={t('noticeDocs_.batch')}
          value={batchId ? String(batchId) : ''}
          onChange={(event: SelectChangeEvent) => setBatchId(Number(event.target.value))}
          sx={{ minWidth: 240 }}
        >
          <MenuItem value="" disabled>{t('noticeDocs_.pickBatch')}</MenuItem>
          {(batches.data ?? []).map((x) => (
            <MenuItem key={x.id} value={String(x.id)}>{`${x.title} (${x.date}, ${x.noticeCount})`}</MenuItem>
          ))}
        </Select>
        <Button variant="outlined" disabled={!batchId} onClick={() => downloadNoticeZip(companyId, batchId).catch(fail)}>
          {t('noticeDocs_.zip')}
        </Button>
        {canWrite ? (
          <Button variant="outlined" disabled={!batchId || action.isPending} onClick={() => previewEmails().catch(fail)}>
            {t('noticeDocs_.email')}
          </Button>
        ) : null}
        {canPost && batch && batch.costsJournalEntryId === null ? (
          <Button variant="outlined" disabled={action.isPending || batch.totalCosts === 0} onClick={() => setPending({ kind: 'post' })}>
            {t('noticeDocs_.postCosts')}
          </Button>
        ) : null}
        {canPost && batch && batch.costsJournalEntryId !== null ? (
          <Button variant="outlined" color="warning" disabled={action.isPending} onClick={() => setPending({ kind: 'cancel' })}>
            {t('noticeDocs_.cancelCosts')}
          </Button>
        ) : null}
        <Button variant="text" onClick={() => downloadLawsuitCsv(companyId, batchId || undefined).catch(fail)}>
          {t('noticeDocs_.exportLawsuit')}
        </Button>
      </Stack>
      {batch && batch.costsJournalEntryId !== null ? (
        <Typography variant="body2" color="text.secondary">{t('noticeDocs_.costsPosted', { id: batch.costsJournalEntryId })}</Typography>
      ) : null}
      {message ? <Alert severity="info" onClose={() => setMessage(null)}>{message}</Alert> : null}
      {action.error ? <Alert severity="error">{getErrorMessage(action.error, t('noticeDocs_.failed'))}</Alert> : null}
      <ConfirmDialog
        open={pending !== null}
        title={t(pending?.kind === 'email' ? 'noticeDocs_.emailTitle' : pending?.kind === 'post' ? 'noticeDocs_.postTitle' : 'noticeDocs_.cancelTitle')}
        description={
          pending?.kind === 'email'
            ? t('noticeDocs_.emailBody', { withEmail: pending.preview.withEmail, total: pending.preview.totalNotices, missing: pending.preview.missingEmail })
            : pending?.kind === 'post'
              ? t('noticeDocs_.postBody', { amount: formatMoney(batch?.totalCosts ?? 0) })
              : t('noticeDocs_.cancelBody')
        }
        confirmLabel={t(pending?.kind === 'email' ? 'noticeDocs_.email' : pending?.kind === 'post' ? 'noticeDocs_.postCosts' : 'noticeDocs_.cancelCosts')}
        pending={action.isPending}
        onClose={() => setPending(null)}
        onConfirm={() => { confirm().catch(() => undefined) }}
      />
    </Stack>
  )
}

/** Per-row: notice PDF download + "za utuženje" flag with optional lawyer cost (GAP-20). */
export function NoticeRowDocuments({ companyId, notice, canWrite }: { companyId: number; notice: Notice; canWrite: boolean }) {
  const { t } = useTranslation()
  const setLawsuit = useSetNoticeLawsuit(companyId)
  const [cost, setCost] = useState(notice.lawyerCost === null ? '' : String(notice.lawyerCost))
  const save = (isForLawsuit: boolean) =>
    setLawsuit.mutate({ noticeId: notice.id, isForLawsuit, lawyerCost: isForLawsuit && cost !== '' ? Number(cost.replace(',', '.')) : null })

  return (
    <Stack direction="row" spacing={1} alignItems="center">
      <Button size="small" onClick={() => { downloadNoticePdf(companyId, notice.id).catch(() => undefined) }}>{t('noticeDocs_.pdf')}</Button>
      <Checkbox
        size="small"
        checked={notice.isForLawsuit}
        disabled={!canWrite || setLawsuit.isPending}
        onChange={(event) => save(event.target.checked)}
        slotProps={{ input: { 'aria-label': t('noticeDocs_.lawsuit') } }}
      />
      {notice.isForLawsuit ? (
        <TextField
          size="small"
          label={t('noticeDocs_.lawyerCost')}
          value={cost}
          disabled={!canWrite}
          onChange={(event) => setCost(event.target.value)}
          onBlur={() => save(true)}
          sx={{ width: 130 }}
        />
      ) : null}
    </Stack>
  )
}
