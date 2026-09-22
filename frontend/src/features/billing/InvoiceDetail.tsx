import { Alert, Chip, Paper, Stack, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import type { InvoiceSummary } from './types'

export function InvoiceDetail({ companyId, invoiceId }: { companyId: number; invoiceId: number }) {
  const query = useQuery({
    queryKey: ['companies', companyId, 'invoices', invoiceId],
    queryFn: () => apiRequest<InvoiceSummary>(`/api/v1/companies/${companyId}/invoices/${invoiceId}`),
  })
  if (query.error) return <Alert severity="error">{getErrorMessage(query.error, 'Račun nije učitan.')}</Alert>
  if (!query.data) return <Typography role="status">Učitavanje…</Typography>
  const invoice = query.data
  return (
    <Paper component="article" sx={{ p: 3 }}>
      <Stack spacing={1}>
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography component="h2" variant="h5">Račun {invoice.sequenceNumber}</Typography>
          {invoice.isCancelled ? <Chip color="error" label="Storniran" /> : null}
        </Stack>
        <Typography>{invoice.partnerName}</Typography>
        <Typography color="text.secondary">{invoice.address}, {invoice.postalCode} {invoice.city}</Typography>
        <Typography>Rok plaćanja: {invoice.dueDate}</Typography>
        <Typography fontWeight={700}>Ukupno: {invoice.invoiceTotal.toFixed(2)} {invoice.currency}</Typography>
      </Stack>
    </Paper>
  )
}
