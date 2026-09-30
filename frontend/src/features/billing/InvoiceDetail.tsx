import { useState } from 'react'
import { Alert, Box, Button, Chip, Paper, Stack, TextField, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import { billingKeys, downloadInvoicePdf, useCancelInvoice } from './billingApi'
import type { InvoiceSummary } from './types'

export function InvoiceDetail({ companyId, invoiceId, canPost = false }: { companyId: number; invoiceId: number; canPost?: boolean }) {
  const { t } = useTranslation()
  const [confirming, setConfirming] = useState(false)
  const [reason, setReason] = useState('')
  const cancel = useCancelInvoice(companyId, invoiceId)
  const [downloadError, setDownloadError] = useState(false)
  const query = useQuery({
    queryKey: [...billingKeys.invoices(companyId), invoiceId],
    queryFn: () => apiRequest<InvoiceSummary>(`/api/v1/companies/${companyId}/invoices/${invoiceId}`),
  })
  if (query.error) return <Alert severity="error">{getErrorMessage(query.error, t('billing_.detail.notLoaded'))}</Alert>
  if (!query.data) return <Typography role="status">{t('billing_.detail.loading')}</Typography>
  const invoice = query.data
  return (
    <Paper component="article" sx={{ p: 3 }}>
      <Stack spacing={1}>
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography component="h2" variant="h5">{t('billing_.columns.number')} {invoice.sequenceNumber}</Typography>
          {invoice.isCancelled ? <Chip color="error" label={t('billing_.detail.cancelled')} /> : null}
        </Stack>
        <Typography>{invoice.partnerName}</Typography>
        <Typography color="text.secondary">{invoice.address}, {invoice.postalCode} {invoice.city}</Typography>
        <Typography>{t('billing_.detail.dueDate')}: {formatDate(invoice.dueDate)}</Typography>
        <Typography>{t('billing_.detail.total')}: {formatMoney(invoice.total, invoice.currency)}</Typography>
        <Typography>{t('invx_.interest')}: {formatMoney(invoice.interestAmount, invoice.currency)}</Typography>
        <Typography fontWeight={700}>{t('invx_.totalWithInterest')}: {formatMoney(invoice.invoiceTotal, invoice.currency)}</Typography>
        {cancel.error ? <Alert severity="error">{getErrorMessage(cancel.error, t('posting_.cancelInvoice'))}</Alert> : null}
        {canPost && !invoice.isCancelled ? (
          <Stack direction="row">
            <Button variant="outlined" color="warning" disabled={cancel.isPending} onClick={() => setConfirming(true)}>
              {t('posting_.cancelInvoice')}
            </Button>
          </Stack>
        ) : null}
        <Box>
          <Button variant="outlined" onClick={() => { setDownloadError(false); downloadInvoicePdf(companyId, invoice.id, invoice.sequenceNumber).catch(() => setDownloadError(true)) }}>
            {t('invoicePdf.download')}
          </Button>
        </Box>
        {downloadError ? <Alert severity="error">{t('invoicePdf.failed')}</Alert> : null}
      </Stack>
      <ConfirmDialog
        open={confirming}
        title={t('posting_.cancelInvoiceTitle')}
        description={
          <Stack spacing={2}>
            <span>{t('posting_.cancelInvoiceBody', { number: invoice.sequenceNumber, amount: formatMoney(invoice.invoiceTotal, invoice.currency) })}</span>
            <TextField
              size="small"
              required
              label={t('posting_.cancelReason')}
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              slotProps={{ htmlInput: { maxLength: 500 } }}
            />
          </Stack>
        }
        confirmLabel={t('posting_.cancelInvoice')}
        destructive
        pending={cancel.isPending || reason.trim().length === 0}
        onClose={() => setConfirming(false)}
        onConfirm={() => {
          cancel.mutate({ reason: reason.trim(), rowVersion: invoice.rowVersion })
          setConfirming(false)
        }}
      />
    </Paper>
  )
}
