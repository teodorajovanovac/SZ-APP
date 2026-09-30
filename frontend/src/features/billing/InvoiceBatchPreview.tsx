import { Alert, Button, Card, CardContent, Divider, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { useGenerateInvoiceBatch, usePreviewInvoiceBatch } from './billingApi'
import type { GenerateInvoicesRequest } from './types'

/** Item 8 wizard: server-computed preview (one row per building, ±% vs previous batch), then generate the same scope. */
export function InvoiceBatchPreview({ companyId, request }: { companyId: number; request: GenerateInvoicesRequest }) {
  const { t } = useTranslation()
  const preview = usePreviewInvoiceBatch(companyId)
  const generate = useGenerateInvoiceBatch(companyId)
  const error = preview.error ?? generate.error
  const data = preview.data
  const results = generate.data?.companies ?? []
  const single = data?.buildings.length === 1 ? data.buildings[0] : null
  const only = results.length === 1 ? results[0] : undefined

  return (
    <Stack spacing={2}>
      <Typography component="h2" variant="h6">{t('billing2_.title')}</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, t('billing2_.previewFailed'))}</Alert> : null}
      {generate.data ? (
        only && !only.error ? (
          <Alert severity={only.alreadyGenerated ? 'info' : 'success'}>
            {t(only.alreadyGenerated ? 'billing2_.alreadyGenerated' : 'billing2_.generated', { count: only.invoiceCount })}
          </Alert>
        ) : (
          <Alert severity={results.some((r) => r.error) ? 'warning' : 'success'}>
            {t('invx_.generatedCompanies', {
              ok: results.filter((r) => !r.error).length,
              count: results.reduce((s, r) => s + r.invoiceCount, 0),
              failed: results.filter((r) => r.error).map((r) => `${r.companyName}: ${r.error}`).join('; ') || '0',
            })}
          </Alert>
        )
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
                  <TableCell>{t('invx_.building')}</TableCell>
                  <TableCell align="right">{t('billing2_.customer')}</TableCell>
                  <TableCell align="right">{t('billing2_.total')}</TableCell>
                  <TableCell align="right">{t('billing2_.interest')}</TableCell>
                  <TableCell align="right">{t('invx_.previous')}</TableCell>
                  <TableCell align="right">{t('invx_.change')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.buildings.map((b) => (
                  <TableRow key={b.companyId}>
                    <TableCell>{b.companyName}{b.error ? <Typography variant="caption" color="error" display="block">{b.error}</Typography> : null}</TableCell>
                    <TableCell align="right">{b.customerCount}</TableCell>
                    <TableCell align="right">{formatMoney(b.total)}</TableCell>
                    <TableCell align="right">{formatMoney(b.interestTotal)}</TableCell>
                    <TableCell align="right">{b.previousTotal == null ? '—' : formatMoney(b.previousTotal)}</TableCell>
                    <TableCell align="right">{b.changePercent == null ? '—' : `${b.changePercent > 0 ? '+' : ''}${b.changePercent.toFixed(2)} %`}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {single ? (
              <Table size="small" sx={{ mt: 2 }}>
                <TableHead>
                  <TableRow>
                    <TableCell>{t('billing2_.customer')}</TableCell>
                    <TableCell align="right">{t('billing2_.net')}</TableCell>
                    <TableCell align="right">{t('billing2_.vat')}</TableCell>
                    <TableCell align="right">{t('billing2_.interest')}</TableCell>
                    <TableCell align="right">{t('billing2_.total')}</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {single.customers.map((c) => (
                    <TableRow key={c.customerId}>
                      <TableCell>{c.customerName}</TableCell>
                      <TableCell align="right">{formatMoney(c.net)}</TableCell>
                      <TableCell align="right">{formatMoney(c.vat)}</TableCell>
                      <TableCell align="right">{formatMoney(c.interest)}</TableCell>
                      <TableCell align="right">{formatMoney(c.total)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : null}
          </CardContent>
        </Card>
      ) : null}
    </Stack>
  )
}
