import { Alert, Button, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { getErrorMessage } from '../../api/problemDetails'
import { useInvoiceBatches, useInvoices, usePostInvoiceBatch } from './billingApi'
import { InvoiceBatchForm } from './InvoiceBatchForm'

export function BillingWorkspace({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const batches = useInvoiceBatches(companyId)
  const invoices = useInvoices(companyId)
  const post = usePostInvoiceBatch(companyId)
  const error = batches.error ?? invoices.error ?? post.error

  return (
    <Stack spacing={3}>
      <Typography component="h1" variant="h1">Fakturisanje</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, 'Podaci fakturisanja nisu dostupni.')}</Alert> : null}
      <Paper sx={{ p: 3 }}><InvoiceBatchForm companyId={companyId} /></Paper>
      <Paper sx={{ p: 2, overflowX: 'auto' }}>
        <Typography component="h2" variant="h6">Serije računa</Typography>
        <Table size="small" aria-label="Serije računa">
          <TableHead><TableRow><TableCell>Period</TableCell><TableCell>Naziv</TableCell><TableCell>Status</TableCell><TableCell align="right">Akcija</TableCell></TableRow></TableHead>
          <TableBody>{batches.data?.items.map((batch) => (
            <TableRow key={batch.id}>
              <TableCell>{batch.periodYYMM}</TableCell><TableCell>{batch.caption}</TableCell><TableCell>{batch.status}</TableCell>
              <TableCell align="right">{canPost && batch.status === 'Generated' ? <Button onClick={() => post.mutate(batch.id)}>Knjiži</Button> : null}</TableCell>
            </TableRow>
          ))}</TableBody>
        </Table>
      </Paper>
      <Paper sx={{ p: 2, overflowX: 'auto' }}>
        <Typography component="h2" variant="h6">Računi</Typography>
        <Table size="small" aria-label="Računi">
          <TableHead><TableRow><TableCell>Broj</TableCell><TableCell>Partner</TableCell><TableCell>Datum</TableCell><TableCell align="right">Ukupno</TableCell></TableRow></TableHead>
          <TableBody>{invoices.data?.items.map((invoice) => (
            <TableRow key={invoice.id}><TableCell>{invoice.sequenceNumber}</TableCell><TableCell>{invoice.partnerName}</TableCell><TableCell>{invoice.issueDate}</TableCell><TableCell align="right">{invoice.invoiceTotal.toFixed(2)} {invoice.currency}</TableCell></TableRow>
          ))}</TableBody>
        </Table>
      </Paper>
    </Stack>
  )
}
