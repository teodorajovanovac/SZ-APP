import { Alert, Button, Paper, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { getErrorMessage } from '../../api/problemDetails'
import { useNoticeCommand, useNotices } from './noticeApi'

export function NoticeList({ companyId, canWrite }: { companyId: number; canWrite: boolean }) {
  const query = useNotices(companyId)
  const command = useNoticeCommand(companyId)
  const error = query.error ?? command.error
  return (
    <Paper sx={{ p: 2, overflowX: 'auto' }}>
      <Typography component="h2" variant="h6">Opomene</Typography>
      {error ? <Alert severity="error">{getErrorMessage(error, 'Obrada opomene nije uspela.')}</Alert> : null}
      <Table size="small" aria-label="Opomene">
        <TableHead><TableRow><TableCell>Partner konto</TableCell><TableCell>Broj računa</TableCell><TableCell align="right">Dug</TableCell><TableCell>Status</TableCell><TableCell align="right">Akcija</TableCell></TableRow></TableHead>
        <TableBody>{query.data?.items.map((notice) => (
          <TableRow key={notice.id}>
            <TableCell>{notice.partnerAccountId}</TableCell><TableCell>{notice.unpaidInvoiceCount}</TableCell><TableCell align="right">{notice.total.toFixed(2)}</TableCell><TableCell>{notice.deliveryStatus}</TableCell>
            <TableCell align="right">{canWrite && notice.deliveryStatus === 'Draft' ? <Button onClick={() => command.mutate({ noticeId: notice.id, command: 'render' })}>Renderuj</Button> : null}{canWrite && notice.deliveryStatus === 'Rendered' ? <Button onClick={() => command.mutate({ noticeId: notice.id, command: 'send' })}>Pošalji</Button> : null}</TableCell>
          </TableRow>
        ))}</TableBody>
      </Table>
    </Paper>
  )
}
