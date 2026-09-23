import { Alert, Chip, Paper, Stack, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import type { InvoiceSummary } from './types'

export function InvoiceDetail({ companyId, invoiceId }: { companyId: number; invoiceId: number }) {
  const { t } = useTranslation()
  const query = useQuery({
    queryKey: ['companies', companyId, 'invoices', invoiceId],
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
        <Typography>{t('billing_.detail.dueDate')}: {invoice.dueDate}</Typography>
        <Typography fontWeight={700}>{t('billing_.detail.total')}: {formatMoney(invoice.invoiceTotal, invoice.currency)}</Typography>
      </Stack>
    </Paper>
  )
}
