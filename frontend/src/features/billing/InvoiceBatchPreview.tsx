import { Alert, Button, Card, CardContent, Divider, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { useGenerateInvoiceBatch, usePreviewInvoiceBatch } from './billingApi'
import type { InvoiceGenerationRequest } from './types'

export function InvoiceBatchPreview({ companyId, batchId, request }: { companyId: number; batchId: number; request: InvoiceGenerationRequest }) {
  const { t } = useTranslation()
  const preview = usePreviewInvoiceBatch(companyId, batchId)
  const generate = useGenerateInvoiceBatch(companyId, batchId)
  const error = preview.error ?? generate.error

  return (
    <Stack spacing={2}>
      {error ? <Alert severity="error">{getErrorMessage(error, t('billing_.preview.processingFailed'))}</Alert> : null}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <Button variant="outlined" onClick={() => preview.mutate(request)} disabled={preview.isPending}>{t('billing_.preview.previewButton')}</Button>
        <Button variant="contained" onClick={() => generate.mutate(request)} disabled={!preview.data || generate.isPending}>{t('billing_.preview.generateButton')}</Button>
      </Stack>
      {preview.data ? (
        <Card>
          <CardContent>
            <Typography component="h2" variant="h6">{t('billing_.preview.title')}</Typography>
            <Typography>{t('billing_.preview.invoiceCount', { count: preview.data.invoiceCount })}</Typography>
            <Divider sx={{ my: 1 }} />
            <Typography>{t('billing_.preview.net')}: {formatMoney(preview.data.netAmount)}</Typography>
            <Typography>{t('billing_.preview.vat')}: {formatMoney(preview.data.vatAmount)}</Typography>
            <Typography>{t('billing_.preview.interest')}: {formatMoney(preview.data.interestAmount)}</Typography>
            <Typography fontWeight={700}>{t('billing_.preview.total')}: {formatMoney(preview.data.totalAmount)}</Typography>
          </CardContent>
        </Card>
      ) : null}
    </Stack>
  )
}
