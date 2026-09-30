import { useState } from 'react'
import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { formatDate } from '../../shared/format/date'
import { accountingApi } from '../journals/accountingApi'

const nextPeriod = (p: number) => (p % 100 === 12 ? p + 89 : p + 1)
const currentPeriod = () => { const d = new Date(); return (d.getFullYear() % 100) * 100 + d.getMonth() + 1 }

/** GAP-33 copy to next month + GAP-34 virmans (create, list, print, CSV export, archive). */
export function SupplierWorkflowActions({ companyId, periodYYMM }: { companyId: number; periodYYMM?: number }) {
  const { t } = useTranslation()
  const client = useQueryClient()
  const [copyOpen, setCopyOpen] = useState(false)
  const [ordersOpen, setOrdersOpen] = useState(false)
  const [from, setFrom] = useState(String(periodYYMM ?? currentPeriod()))
  const [to, setTo] = useState(String(nextPeriod(periodYYMM ?? currentPeriod())))
  const [scope, setScope] = useState(1)
  const copy = useMutation({
    mutationFn: () => accountingApi.copySuppliers(companyId, { fromPeriodYYMM: Number(from), toPeriodYYMM: Number(to), scope }),
    onSuccess: () => client.invalidateQueries({ queryKey: ['companies', companyId, 'supplier-invoices'] }),
  })
  const create = useMutation({
    mutationFn: () => accountingApi.createOrders(companyId, periodYYMM!),
    onSuccess: () => client.invalidateQueries({ queryKey: ['payment-orders', companyId] }),
  })
  const openCopy = () => {
    const p = periodYYMM ?? currentPeriod()
    setFrom(String(p)); setTo(String(nextPeriod(p))); copy.reset(); setCopyOpen(true)
  }

  return (
    <>
      <Button variant="outlined" onClick={openCopy}>{t('acct_.copy')}</Button>
      <Button variant="outlined" onClick={() => { create.reset(); setOrdersOpen(true) }}>{t('acct_.orders')}</Button>

      <Dialog open={copyOpen} onClose={() => setCopyOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>{t('acct_.copyTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography variant="body2" color="text.secondary">{t('acct_.copyHint')}</Typography>
            <TextField size="small" label={t('acct_.from')} value={from} onChange={(e) => setFrom(e.target.value)} />
            <TextField size="small" label={t('acct_.to')} value={to} onChange={(e) => setTo(e.target.value)} />
            <TextField size="small" select label={t('acct_.scope')} value={scope} onChange={(e) => setScope(Number(e.target.value))}>
              <MenuItem value={0}>{t('acct_.scopeAll')}</MenuItem>
              <MenuItem value={1}>{t('acct_.scopeRegular')}</MenuItem>
              <MenuItem value={2}>{t('acct_.scopeExtra')}</MenuItem>
            </TextField>
            {copy.error ? <Alert severity="error">{getErrorMessage(copy.error, t('acct_.failed'))}</Alert> : null}
            {copy.data ? <Alert severity="success">{t('acct_.copyDone', copy.data)}</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCopyOpen(false)}>{t('acct_.cancel')}</Button>
          <Button variant="contained" disabled={copy.isPending || !!copy.data} onClick={() => copy.mutate()}>{t('acct_.confirm')}</Button>
        </DialogActions>
      </Dialog>

      {ordersOpen ? (
        <Dialog open onClose={() => setOrdersOpen(false)} maxWidth="lg" fullWidth>
          <DialogTitle>{t('acct_.virmansTitle')}</DialogTitle>
          <DialogContent>
            <Stack spacing={2}>
              <Typography variant="body2" color="text.secondary">{t('acct_.virmansHint')}</Typography>
              <Stack direction="row" spacing={1}>
                <Button variant="contained" disabled={!periodYYMM || create.isPending} onClick={() => create.mutate()}>
                  {t('acct_.virmans')}{periodYYMM ? ` (${periodYYMM})` : ''}
                </Button>
              </Stack>
              {!periodYYMM ? <Alert severity="info">{t('acct_.virmansNeedPeriod')}</Alert> : null}
              {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('acct_.failed'))}</Alert> : null}
              {create.data ? (
                <Alert severity={create.data.skipped.length ? 'warning' : 'success'}>
                  {t('acct_.virmansDone', { created: create.data.created })}
                  {create.data.skipped.map((s) => <div key={s.supplierInvoiceId}>{t('acct_.skipped')}: {s.caption} — {s.reason}</div>)}
                </Alert>
              ) : null}
              <PaymentOrderTable companyId={companyId} />
            </Stack>
          </DialogContent>
          <DialogActions><Button onClick={() => setOrdersOpen(false)}>{t('acct_.cancel')}</Button></DialogActions>
        </Dialog>
      ) : null}
    </>
  )
}

function PaymentOrderTable({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const client = useQueryClient()
  const orders = useQuery({ queryKey: ['payment-orders', companyId], queryFn: () => accountingApi.orders(companyId) })
  const act = useMutation({
    mutationFn: ({ id, remove }: { id: number; remove: boolean }) => remove ? accountingApi.deleteOrder(companyId, id) : accountingApi.archiveOrder(companyId, id),
    onSuccess: () => client.invalidateQueries({ queryKey: ['payment-orders', companyId] }),
  })
  const rows = orders.data ?? []
  const ids = rows.map((x) => x.id)
  return (
    <Stack spacing={1}>
      <Stack direction="row" spacing={1}>
        <Button size="small" disabled={!ids.length} onClick={() => void accountingApi.printOrders(companyId, ids)}>{t('acct_.print')}</Button>
        <Button size="small" disabled={!ids.length} onClick={() => void accountingApi.exportOrders(companyId, ids)}>{t('acct_.exportCsv')}</Button>
      </Stack>
      {act.error ? <Alert severity="error">{getErrorMessage(act.error, t('acct_.failed'))}</Alert> : null}
      {rows.length === 0 ? <Typography color="text.secondary">{t('acct_.ordersEmpty')}</Typography> : (
        <Table size="small" aria-label={t('acct_.orders')}>
          <TableHead><TableRow>
            <TableCell>{t('acct_.date')}</TableCell><TableCell>{t('acct_.recipient')}</TableCell><TableCell>{t('acct_.account')}</TableCell>
            <TableCell>{t('acct_.reference')}</TableCell><TableCell>{t('acct_.purpose')}</TableCell><TableCell align="right">{t('acct_.amount')}</TableCell><TableCell />
          </TableRow></TableHead>
          <TableBody>
            {rows.map((o) => (
              <TableRow key={o.id}>
                <TableCell>{formatDate(o.date)}</TableCell>
                <TableCell>{o.recipientName.split('\n')[0]}</TableCell>
                <TableCell>{o.recipientAccountNumber}</TableCell>
                <TableCell>{o.recipientPaymentReference}</TableCell>
                <TableCell>{o.paymentPurpose}</TableCell>
                <TableCell align="right">{formatMoney(o.amount)}</TableCell>
                <TableCell>
                  <Button size="small" disabled={act.isPending} onClick={() => act.mutate({ id: o.id, remove: false })}>{t('acct_.archive')}</Button>
                  <Button size="small" color="warning" disabled={act.isPending} onClick={() => act.mutate({ id: o.id, remove: true })}>{t('acct_.remove')}</Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Stack>
  )
}
