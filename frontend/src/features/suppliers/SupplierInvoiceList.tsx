import { Alert, Paper, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { getErrorMessage } from '../../api/problemDetails'
import { useSupplierInvoices } from './supplierApi'

export function SupplierInvoiceList({ companyId }: { companyId: number }) {
  const query = useSupplierInvoices(companyId)
  if (query.error) return <Alert severity="error">{getErrorMessage(query.error, 'Dobavljački računi nisu učitani.')}</Alert>
  return (
    <Paper sx={{ p: 2, overflowX: 'auto' }}>
      <Typography component="h2" variant="h6">Dobavljački računi</Typography>
      <Table size="small" aria-label="Dobavljački računi">
        <TableHead><TableRow><TableCell>R.br.</TableCell><TableCell>Period</TableCell><TableCell>Naziv</TableCell><TableCell align="right">RSD</TableCell><TableCell align="right">Raspoređeno</TableCell></TableRow></TableHead>
        <TableBody>{query.data?.items.map((invoice) => (
          <TableRow key={invoice.id}><TableCell>{invoice.invoiceNo}</TableCell><TableCell>{invoice.periodYYMM}</TableCell><TableCell>{invoice.caption}</TableCell><TableCell align="right">{invoice.amountRsd.toFixed(4)}</TableCell><TableCell align="right">{invoice.postedAmount.toFixed(2)}</TableCell></TableRow>
        ))}</TableBody>
      </Table>
    </Paper>
  )
}
