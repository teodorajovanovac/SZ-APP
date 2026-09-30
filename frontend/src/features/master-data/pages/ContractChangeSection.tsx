import { useState } from 'react'
import {
  Alert, Button, Card, CardContent, Chip, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../api/problemDetails'
import { formatDate } from '../../../shared/format/date'
import { useContractHistory, useReplaceContract } from '../useMasterData'

const num = (v: string) => (v.trim() === '' ? null : Number(v))

/** GAP-16 / flow G: contract history + "change owner/tenant from date" (existing replace endpoint). */
export function ContractChangeSection({ companyId, unitId }: { companyId: number; unitId: number }) {
  const { t } = useTranslation()
  const history = useContractHistory(companyId, unitId)
  const replace = useReplaceContract(companyId, unitId)
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState({ from: '', owner: '', tenant: '', invoice: '', account: '' })
  const contracts = history.data ?? []
  const current = contracts.find((c) => c.isActive) ?? null

  const start = () => {
    setForm({
      from: new Date().toISOString().slice(0, 10), owner: String(current?.ownerPartnerId ?? ''), tenant: String(current?.tenantPartnerId ?? ''),
      invoice: '', account: String(current?.accountNumber ?? ''),
    })
    replace.reset(); setOpen(true)
  }
  const submit = () => replace.mutate({
    accountNumber: num(form.account), ownerPartnerId: num(form.owner), invoicePartnerId: num(form.invoice), tenantPartnerId: num(form.tenant),
    effectiveFrom: form.from, invoiceStartDate: form.from, invoiceEndDate: null, note: null,
    invoiceDeliveryLocation: current?.invoiceDeliveryLocation ?? null, invoiceDeliveryUnitId: current?.invoiceDeliveryUnitId ?? null,
    isPrintInvoiceMandatory: current?.isPrintInvoiceMandatory ?? false, isPrintInvoiceToPostOffice: current?.isPrintInvoiceToPostOffice ?? false,
    isPrintInvoiceSkipped: current?.isPrintInvoiceSkipped ?? false, exportExternalAccount: current?.exportExternalAccount ?? null,
    currentContractRowVersion: current?.rowVersion ?? null,
  }, { onSuccess: () => setOpen(false) })

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
          <Typography variant="h6" component="h2">{t('acct_.contracts')}</Typography>
          <Button variant="outlined" onClick={start}>{t('acct_.changeOwner')}</Button>
        </Stack>
        <Table size="small">
          <TableHead><TableRow>
            <TableCell>{t('acct_.effectiveFrom')}</TableCell><TableCell>—</TableCell><TableCell>{t('acct_.accountNumber')}</TableCell>
            <TableCell>{t('acct_.owner')}</TableCell><TableCell>{t('acct_.tenant')}</TableCell><TableCell />
          </TableRow></TableHead>
          <TableBody>
            {contracts.map((c) => (
              <TableRow key={c.id}>
                <TableCell>{formatDate(c.contractDate)}</TableCell>
                <TableCell>{c.contractEndDate ? formatDate(c.contractEndDate) : ''}</TableCell>
                <TableCell>{c.accountNumber}</TableCell>
                <TableCell>{c.ownerPartnerId}</TableCell>
                <TableCell>{c.tenantPartnerId}</TableCell>
                <TableCell><Chip size="small" color={c.isActive ? 'success' : 'default'} label={c.isActive ? t('acct_.active') : t('acct_.closed')} /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
      <Dialog open={open} onClose={() => setOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>{t('acct_.changeOwner')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography variant="body2" color="text.secondary">{t('acct_.changeHint')}</Typography>
            <TextField size="small" type="date" label={t('acct_.effectiveFrom')} value={form.from} onChange={(e) => setForm({ ...form, from: e.target.value })} slotProps={{ inputLabel: { shrink: true } }} />
            <TextField size="small" label={t('acct_.owner')} value={form.owner} onChange={(e) => setForm({ ...form, owner: e.target.value })} />
            <TextField size="small" label={t('acct_.tenant')} value={form.tenant} onChange={(e) => setForm({ ...form, tenant: e.target.value })} />
            <TextField size="small" label={t('acct_.invoicePartner')} value={form.invoice} onChange={(e) => setForm({ ...form, invoice: e.target.value })} />
            <TextField size="small" label={t('acct_.accountNumber')} value={form.account} onChange={(e) => setForm({ ...form, account: e.target.value })} />
            {replace.error ? <Alert severity="error">{getErrorMessage(replace.error, t('acct_.failed'))}</Alert> : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>{t('acct_.cancel')}</Button>
          <Button variant="contained" disabled={replace.isPending || !form.from} onClick={submit}>{t('acct_.confirm')}</Button>
        </DialogActions>
      </Dialog>
    </Card>
  )
}
