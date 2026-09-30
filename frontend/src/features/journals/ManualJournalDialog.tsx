import { useEffect, useMemo, useRef, useState, type KeyboardEvent } from 'react'
import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, MenuItem, Stack, Table, TableBody,
  TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'
import { useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { useShortList } from '../master-data/useShortList'
import { accountingApi, parseAmount, type JournalInput } from './accountingApi'

interface Row { account: string; partner: string; partnerId: number | null; sub: string; debit: string; credit: string; due: string; ref: string; desc: string }
const emptyRow = (): Row => ({ account: '', partner: '', partnerId: null, sub: '', debit: '', credit: '', due: '', ref: '', desc: '' })
const cols: (keyof Row)[] = ['account', 'partner', 'sub', 'debit', 'credit', 'due', 'ref', 'desc']
const colKey: Record<string, string> = { account: 'account', partner: 'partner', sub: 'sub', debit: 'debit', credit: 'credit', due: 'due', ref: 'ref', desc: 'desc' }
const today = () => new Date().toISOString().slice(0, 10)

/** GAP-13 manual journal / GAP-19 opening balance (line type 99): keyboard grid, draft save, post. */
export function ManualJournalDialog({ companyId, open, onClose, opening = false, draftId, canPost }: {
  companyId: number; open: boolean; onClose: () => void; opening?: boolean; draftId?: number; canPost: boolean
}) {
  const { t } = useTranslation()
  const client = useQueryClient()
  const types = useShortList(companyId, 'LedgerLineType')
  const [rows, setRows] = useState<Row[]>([emptyRow(), emptyRow()])
  const [date, setDate] = useState(today())
  const [description, setDescription] = useState('')
  const [typeId, setTypeId] = useState<number | ''>('')
  const [saved, setSaved] = useState<{ id: number; rowVersion: string } | null>(null)
  const [message, setMessage] = useState<{ ok: boolean; text: string } | null>(null)
  const [busy, setBusy] = useState(false)
  const [paste, setPaste] = useState('')
  const grid = useRef<HTMLTableSectionElement>(null)

  useEffect(() => {
    if (!open) return
    setMessage(null); setPaste('')
    if (draftId) {
      void accountingApi.journal(companyId, draftId).then((d) => {
        setSaved({ id: d.header.id, rowVersion: d.header.rowVersion })
        setDate(d.header.postingDate); setDescription(d.header.description); setTypeId(d.header.journalEntryTypeId ?? '')
        setRows(d.lines.map((l) => ({
          account: l.account, partner: l.partnerAccountId ? `#${l.partnerAccountId}` : '', partnerId: l.partnerAccountId,
          sub: l.subAccountId ?? '', debit: l.debitAmount ? String(l.debitAmount).replace('.', ',') : '',
          credit: l.creditAmount ? String(l.creditAmount).replace('.', ',') : '', due: l.dueDate ?? '', ref: l.paymentReference ?? '', desc: l.description ?? '',
        })))
      })
    } else {
      setSaved(null); setDate(today()); setRows([emptyRow(), emptyRow()]); setTypeId('')
      setDescription(opening ? t('acct_.openingBalance').toUpperCase() : '')
    }
  }, [open, draftId, companyId, opening, t])

  // Opening balance = legacy TIP_STAVKE 99.
  useEffect(() => {
    if (opening && !draftId && typeId === '') {
      const id = types.data?.find((x) => x.indexValue === 99)?.id
      if (id) setTypeId(id)
    }
  }, [opening, draftId, typeId, types.data])

  const totals = useMemo(() => {
    const d = rows.reduce((s, r) => s + parseAmount(r.debit), 0)
    const c = rows.reduce((s, r) => s + parseAmount(r.credit), 0)
    return { d, c, diff: Math.round((d - c) * 100) / 100 }
  }, [rows])

  const set = (i: number, key: keyof Row, value: string) =>
    setRows((all) => all.map((r, j) => (j === i ? { ...r, [key]: value, ...(key === 'partner' ? { partnerId: null } : {}) } : r)))

  const onKey = (e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key !== 'Enter') return
    e.preventDefault()
    if (e.ctrlKey) {
      setRows((all) => [...all, emptyRow()])
      setTimeout(() => {
        const inputs = grid.current?.querySelectorAll('input')
        inputs?.[inputs.length - cols.length]?.focus()
      })
      return
    }
    const inputs = Array.from(grid.current?.querySelectorAll('input') ?? [])
    inputs[inputs.indexOf(e.target as HTMLInputElement) + 1]?.focus()
  }

  const applyPaste = () => {
    const parsed = paste.split(/\r?\n/).filter((l) => l.trim()).map((l) => {
      const [account = '', partner = '', debit = '', credit = '', ref = '', desc = ''] = l.split(/[;\t]/).map((x) => x.trim())
      return { ...emptyRow(), account, partner, debit, credit, ref, desc }
    })
    setRows((all) => [...all.filter((r) => r.account), ...parsed])
    setPaste('')
  }

  async function build(): Promise<JournalInput> {
    const used = rows.filter((r) => r.account.trim() || parseAmount(r.debit) || parseAmount(r.credit))
    const codes = [...new Set(used.filter((r) => !r.partnerId && r.partner.trim()).map((r) => Number(r.partner.trim())))]
    const bad = codes.filter((n) => !Number.isInteger(n))
    const found = codes.length && !bad.length ? await accountingApi.lookup(companyId, codes) : []
    const resolve = (r: Row) => {
      if (r.partnerId || !r.partner.trim()) return r.partnerId
      const hits = found.filter((x) => x.accountNumber === Number(r.partner.trim()))
      return (hits.find((x) => x.account === r.account.trim()) ?? hits[0])?.id ?? null
    }
    const missing = used.filter((r) => r.partner.trim() && !resolve(r)).map((r) => r.partner)
    if (missing.length) throw new Error(t('acct_.partnerUnknown', { codes: [...new Set(missing)].join(', ') }))
    return {
      postingDate: date, dueDate: null, description: description.trim(), currency: 'RSD', journalEntryTypeId: typeId === '' ? null : typeId,
      lines: used.map((r) => ({
        account: r.account.trim(), debitAmount: parseAmount(r.debit), creditAmount: parseAmount(r.credit), dueDate: r.due || null,
        documentRef: null, subAccountId: r.sub.trim() || null, partnerAccountId: resolve(r), note: null,
        paymentReference: r.ref.trim() || null, description: r.desc.trim() || null,
      })),
    }
  }

  async function run(action: 'save' | 'post' | 'delete') {
    setBusy(true); setMessage(null)
    try {
      if (action === 'delete' && saved) {
        await accountingApi.deleteDraft(companyId, saved.id, saved.rowVersion)
        onClose()
      } else {
        const body = await build()
        const result = saved
          ? await accountingApi.updateDraft(companyId, saved.id, saved.rowVersion, body)
          : await accountingApi.createDraft(companyId, body)
        setSaved({ id: result.header.id, rowVersion: result.header.rowVersion })
        if (action === 'post') {
          await accountingApi.post(companyId, result.header.id, result.header.rowVersion)
          onClose()
        } else setMessage({ ok: true, text: t('acct_.saved', { id: result.header.id }) })
      }
      await client.invalidateQueries({ queryKey: ['ledger-journals', companyId] })
    } catch (error) {
      setMessage({ ok: false, text: getErrorMessage(error, t('acct_.failed')) })
    } finally {
      setBusy(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>{opening ? t('acct_.openingTitle') : t('acct_.journalTitle')}{saved ? ` #${saved.id}` : ''}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField size="small" type="date" label={t('acct_.postingDate')} value={date} onChange={(e) => setDate(e.target.value)} slotProps={{ inputLabel: { shrink: true } }} />
            <TextField size="small" label={t('acct_.description')} value={description} onChange={(e) => setDescription(e.target.value)} sx={{ flex: 1 }} />
            <TextField size="small" select label={t('acct_.type')} value={typeId} onChange={(e) => setTypeId(e.target.value === '' ? '' : Number(e.target.value))} sx={{ minWidth: 200 }}>
              <MenuItem value="">—</MenuItem>
              {(types.data ?? []).map((x) => <MenuItem key={x.id} value={x.id}>{x.indexValue} · {x.caption}</MenuItem>)}
            </TextField>
          </Stack>
          <Typography variant="caption" color="text.secondary">{t('acct_.keysHint')}</Typography>
          <Table size="small" aria-label={t('acct_.journalTitle')}>
            <TableHead>
              <TableRow>
                {cols.map((c) => <TableCell key={c}>{t(`acct_.col.${colKey[c]}`)}</TableCell>)}
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody ref={grid}>
              {rows.map((r, i) => (
                <TableRow key={i}>
                  {cols.map((c) => (
                    <TableCell key={c} sx={{ p: 0.5 }}>
                      <TextField size="small" variant="standard" value={r[c] ?? ''} onKeyDown={onKey}
                        type={c === 'due' ? 'date' : 'text'} onChange={(e) => set(i, c, e.target.value)}
                        slotProps={{ htmlInput: { 'aria-label': `${t(`acct_.col.${colKey[c]}`)} ${i + 1}`, inputMode: c === 'debit' || c === 'credit' ? 'decimal' : undefined } }}
                        sx={{ minWidth: c === 'desc' ? 160 : c === 'due' ? 130 : 80 }} />
                    </TableCell>
                  ))}
                  <TableCell sx={{ p: 0 }}>
                    <IconButton size="small" aria-label={t('acct_.removeLine')} onClick={() => setRows((all) => all.filter((_, j) => j !== i))}><DeleteIcon fontSize="small" /></IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <Stack direction="row" spacing={2} alignItems="center">
            <Button size="small" onClick={() => setRows((all) => [...all, emptyRow()])}>{t('acct_.addLine')}</Button>
            <Typography color={totals.diff === 0 ? 'text.secondary' : 'error'} variant="body2">
              {t('acct_.totals', { d: formatMoney(totals.d), c: formatMoney(totals.c), diff: formatMoney(totals.diff) })}
            </Typography>
          </Stack>
          {opening ? (
            <Stack direction="row" spacing={1} alignItems="flex-start">
              <TextField size="small" multiline minRows={2} label={t('acct_.paste')} value={paste} onChange={(e) => setPaste(e.target.value)} sx={{ flex: 1 }} />
              <Button onClick={applyPaste} disabled={!paste.trim()}>{t('acct_.pasteApply')}</Button>
            </Stack>
          ) : null}
          {message ? <Alert severity={message.ok ? 'success' : 'error'}>{message.text}</Alert> : null}
        </Stack>
      </DialogContent>
      <DialogActions>
        {saved ? <Button color="warning" disabled={busy} onClick={() => void run('delete')}>{t('acct_.deleteDraft')}</Button> : null}
        <Button onClick={onClose}>{t('acct_.cancel')}</Button>
        <Button variant="outlined" disabled={busy} onClick={() => void run('save')}>{t('acct_.saveDraft')}</Button>
        {canPost ? <Button variant="contained" disabled={busy || totals.diff !== 0} onClick={() => void run('post')}>{t('acct_.post')}</Button> : null}
      </DialogActions>
    </Dialog>
  )
}
