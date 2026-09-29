import { Alert, Button, Card, CardContent, Divider, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { useGenerateInvoiceBatch, usePreviewInvoiceBatch } from './billingApi'
import type { GenerateInvoicesRequest } from './types'

/** Item 8 wizard: server-computed preview, then generate the same period. */
export function InvoiceBatchPreview({ companyId, request }: { companyId: number; request: GenerateInvoicesRequest }) {
  const { t } = useTranslation()
  const preview = usePreviewInvoiceBatch(companyId)
  const generate = useGenerateInvoiceBatch(companyId)
  const error = preview.error ?? generate.error
  const data = preview.data

  return (
    <Stack spacing={2}>
      <Typography component="h2" variant="h6">{t('billing2_.title')}</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, t('billing2_.previewFailed'))}</Alert> : null}
      {generate.data ? (
        <Alert severity={generate.data.alreadyGenerated ? 'info' : 'success'}>
          {t(generate.data.alreadyGenerated ? 'billing2_.alreadyGenerated' : 'billing2_.generated', { count: generate.data.invoiceIds.length })}
        </Alert>
      ) : null}
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <Button variant="outlined" onClick={() => preview.mutate(request)} disabled={preview.isPending}>{t('billing2_.previewButton')}</Button>
        <Button variant="contained" onClick={() => generate.mutate(request)} disabled={!data || generate.isPending || generate.isSuccess}>
          {t('billing2_.generateButton')}
        </Button>
      </Stack>
      {data ? (
        <Card variant="outlined">
          <CardContent>
            <Typography>{t('billing2_.customers', { count: data.customerCount })}</Typography>
            <Divider sx={{ my: 1 }} />
            <Typography>{t('billing2_.net')}: {formatMoney(data.netTotal)}</Typography>
            <Typography>{t('billing2_.vat')}: {formatMoney(data.vatTotal)}</Typography>
            <Typography>{t('billing2_.interest')}: {formatMoney(data.interestTotal)} <Typography component="span" variant="caption" color="text.secondary">({t('billing2_.interestNote')})</Typography></Typography>
            <Typography fontWeight={700}>{t('billing2_.total')}: {formatMoney(data.total)}</Typography>
            <Table size="small" sx={{ mt: 2 }}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('billing2_.customer')}</TableCell>
                  <TableCell align="right">{t('billing2_.net')}</TableCell>
                  <TableCell align="right">{t('billing2_.vat')}</TableCell>
                  <TableCell align="right">{t('billing2_.total')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.customers.map((c) => (
                  <TableRow key={c.customerId}>
                    <TableCell>{c.customerName}</TableCell>
                    <TableCell align="right">{formatMoney(c.net)}</TableCell>
                    <TableCell align="right">{formatMoney(c.vat)}</TableCell>
                    <TableCell align="right">{formatMoney(c.total)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ) : null}
    </Stack>
  )
}
