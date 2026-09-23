import { Alert, Button, Card, CardContent, Divider, Stack, Typography } from '@mui/material'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { useGenerateInvoiceBatch, usePreviewInvoiceBatch } from './billingApi'
import type { InvoiceGenerationRequest } from './types'

export function InvoiceBatchPreview({ companyId, batchId, request }: { companyId: number; batchId: number; request: InvoiceGenerationRequest }) {
  const preview = usePreviewInvoiceBatch(companyId, batchId)
  const generate = useGenerateInvoiceBatch(companyId, batchId)
  const error = preview.error ?? generate.error

  return (
    <Stack spacing={2}>
      {error ? <Alert severity="error">{getErrorMessage(error, 'Obrada serije nije uspela.')}</Alert> : null}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <Button variant="outlined" onClick={() => preview.mutate(request)} disabled={preview.isPending}>Prikaži obračun</Button>
        <Button variant="contained" onClick={() => generate.mutate(request)} disabled={!preview.data || generate.isPending}>Generiši račune</Button>
      </Stack>
      {preview.data ? (
        <Card variant="outlined">
          <CardContent>
            <Typography component="h2" variant="h6">Pregled serije</Typography>
            <Typography>{preview.data.invoiceCount} računa</Typography>
            <Divider sx={{ my: 1 }} />
            <Typography>Neto: {formatMoney(preview.data.netAmount)}</Typography>
            <Typography>PDV: {formatMoney(preview.data.vatAmount)}</Typography>
            <Typography>Kamata: {formatMoney(preview.data.interestAmount)}</Typography>
            <Typography fontWeight={700}>Ukupno: {formatMoney(preview.data.totalAmount)}</Typography>
          </CardContent>
        </Card>
      ) : null}
    </Stack>
  )
}
