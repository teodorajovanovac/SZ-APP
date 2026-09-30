import { useState } from 'react'
import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { accountingApi } from './accountingApi'
import { ManualJournalDialog } from './ManualJournalDialog'

/** Toolbar above the journal list: manual journal, opening balance, advance reclassification, balances per account. */
export function JournalTools({ companyId, canWrite, canPost }: { companyId: number; canWrite: boolean; canPost: boolean }) {
  const { t } = useTranslation()
  const [editor, setEditor] = useState<null | 'manual' | 'opening'>(null)
  const [reclass, setReclass] = useState(false)
  const [balances, setBalances] = useState(false)
  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {canWrite ? <Button variant="contained" onClick={() => setEditor('manual')}>{t('acct_.newJournal')}</Button> : null}
      {canWrite ? <Button variant="outlined" onClick={() => setEditor('opening')}>{t('acct_.openingBalance')}</Button> : null}
      {canPost ? <Button variant="outlined" onClick={() => setReclass(true)}>{t('acct_.reclass')}</Button> : null}
      <Button variant="outlined" onClick={() => setBalances(true)}>{t('acct_.balances')}</Button>
      <ManualJournalDialog companyId={companyId} open={editor !== null} opening={editor === 'opening'} canPost={canPost} onClose={() => setEditor(null)} />
      {reclass ? <ReclassDialog companyId={companyId} onClose={() => setReclass(false)} /> : null}
      {balances ? <BalancesDialog companyId={companyId} onClose={() => setBalances(false)} /> : null}
    </Stack>
  )
}

function ReclassDialog({ companyId, onClose }: { companyId: number; onClose: () => void }) {
  const { t } = useTranslation()
  const client = useQueryClient()
  const preview = useQuery({ queryKey: ['advance-reclass', companyId], queryFn: () => accountingApi.reclassPreview(companyId), gcTime: 0 })
  const run = useMutation({
    mutationFn: () => accountingApi.reclass(companyId),
    onSuccess: () => client.invalidateQueries({ queryKey: ['ledger-journals', companyId] }),
  })
  const rows = preview.data ?? []
  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>{t('acct_.reclassTitle')}</DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>{t('acct_.reclassHint')}</Typography>
        {preview.error ? <Alert severity="error">{getErrorMessage(preview.error, t('acct_.failed'))}</Alert> : null}
        {run.error ? <Alert severity="error">{getErrorMessage(run.error, t('acct_.failed'))}</Alert> : null}
        {run.data ? <Alert severity="success">{t('acct_.reclassDone', { count: run.data.journalCount, amount: formatMoney(run.data.totalAmount) })}</Alert> : null}
        {!preview.isLoading && rows.length === 0 && !run.data ? <Alert severity="info">{t('acct_.reclassEmpty')}</Alert> : null}
        {rows.length > 0 && !run.data ? (
          <Table size="small">
            <TableHead><TableRow>
              <TableCell>{t('acct_.partner')}</TableCell><TableCell align="right">{t('acct_.advance')}</TableCell>
              <TableCell align="right">{t('acct_.open')}</TableCell><TableCell align="right">{t('acct_.moved')}</TableCell><TableCell align="right">{t('acct_.refs')}</TableCell>
            </TableRow></TableHead>
            <TableBody>
              {rows.map((r) => (
                <TableRow key={r.partnerAccountId}>
                  <TableCell>{r.accountNumber} · {r.partnerName}</TableCell>
                  <TableCell align="right">{formatMoney(r.advance)}</TableCell>
                  <TableCell align="right">{formatMoney(r.openAmount)}</TableCell>
                  <TableCell align="right">{formatMoney(r.amount)}</TableCell>
                  <TableCell align="right">{r.referenceCount}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : null}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('acct_.cancel')}</Button>
        {rows.length > 0 && !run.data ? (
          <Button variant="contained" disabled={run.isPending} onClick={() => run.mutate()}>
            {t('acct_.confirm')} ({formatMoney(rows.reduce((s, r) => s + r.amount, 0))})
          </Button>
        ) : null}
      </DialogActions>
    </Dialog>
  )
}

function BalancesDialog({ companyId, onClose }: { companyId: number; onClose: () => void }) {
  const { t } = useTranslation()
  const [asOf, setAsOf] = useState(new Date().toISOString().slice(0, 10))
  const query = useQuery({ queryKey: ['account-balances', companyId, asOf], queryFn: () => accountingApi.balances(companyId, asOf), enabled: asOf.length === 10 })
  const rows = query.data ?? []
  const sum = (k: 'debit' | 'credit' | 'balance') => rows.reduce((s, r) => s + r[k], 0)
  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>{t('acct_.balancesTitle')}</DialogTitle>
      <DialogContent>
        <TextField size="small" type="date" label={t('acct_.asOf')} value={asOf} onChange={(e) => setAsOf(e.target.value)} sx={{ my: 1 }} slotProps={{ inputLabel: { shrink: true } }} />
        {query.error ? <Alert severity="error">{getErrorMessage(query.error, t('acct_.failed'))}</Alert> : null}
        <Table size="small">
          <TableHead><TableRow>
            <TableCell>{t('acct_.col.account')}</TableCell><TableCell>{t('acct_.name')}</TableCell>
            <TableCell align="right">{t('acct_.col.debit')}</TableCell><TableCell align="right">{t('acct_.col.credit')}</TableCell><TableCell align="right">{t('acct_.balance')}</TableCell>
          </TableRow></TableHead>
          <TableBody>
            {rows.map((r) => (
              <TableRow key={r.account}>
                <TableCell>{r.account}</TableCell><TableCell>{r.name}</TableCell>
                <TableCell align="right">{formatMoney(r.debit)}</TableCell><TableCell align="right">{formatMoney(r.credit)}</TableCell><TableCell align="right">{formatMoney(r.balance)}</TableCell>
              </TableRow>
            ))}
            <TableRow>
              <TableCell colSpan={2}><strong>Σ</strong></TableCell>
              <TableCell align="right">{formatMoney(sum('debit'))}</TableCell><TableCell align="right">{formatMoney(sum('credit'))}</TableCell><TableCell align="right">{formatMoney(sum('balance'))}</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => window.print()}>{t('acct_.print')}</Button>
        <Button onClick={onClose}>{t('acct_.cancel')}</Button>
      </DialogActions>
    </Dialog>
  )
}
