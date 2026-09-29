import { useEffect, useMemo, useRef, useState } from 'react'
import {
  Alert, Box, Button, Chip, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, List, ListItemButton,
  ListItemText, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import WarningAmberIcon from '@mui/icons-material/WarningAmber'
import DeleteIcon from '@mui/icons-material/Delete'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { ApiProblemError } from '../../api/generated/client'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { formatMoney } from './ledgerBankingFormat'
import { ledgerBankingApi } from './ledgerBankingApi'
import type { BankStatementAllocationInput, BankStatementLine } from './types'

const num = { fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const
const amountOf = (line: BankStatementLine) => line.credit > 0 ? line.credit : line.debit
const isTyping = (target: EventTarget | null) =>
  target instanceof HTMLElement && ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName)

function ErrorAlert({ error }: { error: Error | null }) {
  if (!error) return null
  const message = error instanceof ApiProblemError ? error.problem.detail ?? error.problem.title : error.message
  return <Alert severity="error">{message}</Alert>
}

/**
 * Flow B (5.9): statement lines ⟷ proposals. ↑/↓ moves, Enter accepts, Esc reopens, "/" focuses the
 * partner search, Ctrl+Enter accepts every confident proposal. Posting stays blocked until every
 * line is accepted (FIN-15: nothing is ignored).
 */
export function BankStatementMatchingDialog({
  companyId, statementId, canPost, onClose,
}: { companyId: number; statementId: number | null; canPost: boolean; onClose: () => void }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const key = ['bank-statement-detail', companyId, statementId]
  const detail = useQuery({
    queryKey: key,
    queryFn: () => ledgerBankingApi.statements.get(companyId, statementId!),
    enabled: statementId != null,
  })
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [search, setSearch] = useState('')
  const [split, setSplit] = useState<BankStatementAllocationInput[] | null>(null)
  const [templateName, setTemplateName] = useState<string | null>(null)
  const [templateSubAccount, setTemplateSubAccount] = useState('')
  const [confirmUnpost, setConfirmUnpost] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)
  const searchRef = useRef<HTMLInputElement>(null)

  const statement = detail.data
  const lines = useMemo(() => statement?.lines ?? [], [statement])
  const selected = lines.find((x) => x.id === selectedId) ?? lines[0]
  const posted = statement?.header.status === 'Posted'
  const accepted = lines.filter((x) => x.status === 'Matched' || x.status === 'Posted').length
  const partnerOfSelected = selected?.allocations.find((x) => x.partnerAccountId != null)?.partnerAccountId ?? null

  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: key })
    void queryClient.invalidateQueries({ queryKey: ['bank-statements', companyId] })
  }
  const done = { onSuccess: refresh }

  const partners = useQuery({
    queryKey: ['bank-statement-partners', companyId, search],
    queryFn: () => ledgerBankingApi.partners.search(companyId, search),
    enabled: search.trim().length > 1,
  })

  const accept = useMutation({
    mutationFn: ({ line, allocations }: { line: BankStatementLine; allocations?: BankStatementAllocationInput[] }) =>
      ledgerBankingApi.lines.accept(companyId, line.id, line.rowVersion, allocations),
    onSuccess: () => { setSplit(null); refresh() },
  })
  const reopen = useMutation({
    mutationFn: (line: BankStatementLine) => ledgerBankingApi.lines.reopen(companyId, line.id, line.rowVersion),
    ...done,
  })
  const assign = useMutation({
    mutationFn: ({ line, partnerAccountId }: { line: BankStatementLine; partnerAccountId: number }) =>
      ledgerBankingApi.lines.assignPartner(companyId, line.id, partnerAccountId, line.rowVersion),
    onSuccess: () => { setSearch(''); refresh() },
  })
  const savePayer = useMutation({
    mutationFn: ({ line, partnerAccountId }: { line: BankStatementLine; partnerAccountId: number }) =>
      ledgerBankingApi.lines.savePayerAccount(companyId, line.id, partnerAccountId),
    onSuccess: () => setNotice(t('bankMatch_.savePayerAccountDone')),
  })
  const createTemplate = useMutation({
    mutationFn: ({ line, partnerAccountId }: { line: BankStatementLine; partnerAccountId: number }) =>
      ledgerBankingApi.lines.createTemplate(companyId, line.id, partnerAccountId, templateName ?? undefined, templateSubAccount || undefined),
    onSuccess: () => { setTemplateName(null); setNotice(t('bankMatch_.createTemplateDone')) },
  })
  const acceptConfident = useMutation({ mutationFn: () => ledgerBankingApi.statements.acceptConfident(companyId, statementId!), ...done })
  const rematch = useMutation({ mutationFn: () => ledgerBankingApi.statements.rematch(companyId, statementId!), ...done })
  const post = useMutation({ mutationFn: () => ledgerBankingApi.statements.post(companyId, statement!.header), ...done })
  const unpost = useMutation({ mutationFn: () => ledgerBankingApi.statements.unpost(companyId, statement!.header), ...done })
  const error = [accept, reopen, assign, savePayer, createTemplate, acceptConfident, rematch, post, unpost]
    .map((m) => m.error).find((e) => e) ?? null

  useEffect(() => {
    if (statementId == null || posted) return
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Enter' && event.ctrlKey) {
        event.preventDefault()
        acceptConfident.mutate()
        return
      }
      if (isTyping(event.target) || !selected) return
      const index = lines.findIndex((x) => x.id === selected.id)
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault()
        const next = lines[Math.min(Math.max(index + (event.key === 'ArrowDown' ? 1 : -1), 0), lines.length - 1)]
        if (next) { setSelectedId(next.id); setSplit(null) }
      } else if (event.key === 'Enter' && selected.status === 'Pending' && selected.allocations.length > 0 && !split) {
        event.preventDefault()
        accept.mutate({ line: selected })
      } else if (event.key === 'Escape' && selected.status === 'Matched') {
        event.preventDefault()
        reopen.mutate(selected)
      } else if (event.key === '/') {
        event.preventDefault()
        searchRef.current?.focus()
      }
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [statementId, posted, selected, lines, split, accept, reopen, acceptConfident])

  const splitTotal = split?.reduce((sum, x) => sum + (Number(x.amount) || 0), 0) ?? 0
  const splitOk = selected != null && Math.round(splitTotal * 100) === Math.round(amountOf(selected) * 100)

  return (
    <Dialog open={statementId != null} onClose={onClose} maxWidth="xl" fullWidth>
      <DialogTitle>
        {statement
          ? t('bankMatch_.title', { number: statement.header.statementNumber, year: statement.header.date.slice(0, 4) })
          : '…'}
        {statement ? (
          <Typography component="span" variant="body2" color="text.secondary" sx={{ ml: 2 }}>
            {t('bankMatch_.acceptedCount', { accepted, total: lines.length })}
          </Typography>
        ) : null}
      </DialogTitle>
      <DialogContent dividers>
        <Stack spacing={1}>
          <ErrorAlert error={error} />
          {notice ? <Alert severity="success" onClose={() => setNotice(null)}>{notice}</Alert> : null}
        </Stack>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '2fr 3fr' }, gap: 2, mt: 1 }}>
          <Box>
            <Typography variant="subtitle2">{t('bankMatch_.linesPanel')}</Typography>
            <List dense aria-label={t('bankMatch_.linesPanel')} sx={{ maxHeight: '60vh', overflow: 'auto' }}>
              {lines.map((line) => (
                <ListItemButton
                  key={line.id}
                  selected={line.id === selected?.id}
                  onClick={() => { setSelectedId(line.id); setSplit(null) }}
                >
                  <ListItemText
                    primary={`${line.lineNumber}. ${line.payerRecipientName}`}
                    secondary={line.paymentReference ?? line.info ?? ''}
                  />
                  {line.matchNote && line.allocations.length > 0 ? <WarningAmberIcon color="warning" fontSize="small" sx={{ mr: 1 }} /> : null}
                  <Typography sx={{ ...num, mr: 1 }} color={line.credit > 0 ? 'success.main' : 'error.main'}>
                    {line.credit > 0 ? '+' : '−'}{formatMoney(amountOf(line))}
                  </Typography>
                  <Chip
                    size="small"
                    variant={line.status === 'Pending' ? 'outlined' : 'filled'}
                    color={line.status === 'Pending' ? (line.isConfidentMatch ? 'info' : 'default') : 'success'}
                    label={t(`bankMatch_.status.${line.status}`)}
                  />
                </ListItemButton>
              ))}
            </List>
          </Box>

          <Box>
            <Typography variant="subtitle2">{t('bankMatch_.proposalPanel')}</Typography>
            {selected ? (
              <Stack spacing={1.5} sx={{ mt: 1 }}>
                <Typography variant="body2">
                  {selected.payerRecipientName} · {selected.bankAccountNumber ?? '—'} · {selected.info ?? ''}
                </Typography>
                {selected.matchNote ? (
                  <Alert severity={selected.allocations.length > 0 ? 'warning' : 'info'}>{selected.matchNote}</Alert>
                ) : null}

                {selected.allocations.length > 0 && !split ? (
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>{t('bankMatch_.manualSplitAccount')}</TableCell>
                        <TableCell>{t('bankMatch_.manualSplitPartner')}</TableCell>
                        <TableCell>{t('bankMatch_.manualSplitReference')}</TableCell>
                        <TableCell />
                        <TableCell align="right">{t('bankMatch_.manualSplitAmount')}</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {selected.allocations.map((a) => (
                        <TableRow key={a.id}>
                          <TableCell>{a.account}{a.subAccountId ? ` / ${a.subAccountId}` : ''}</TableCell>
                          <TableCell>{a.partnerName ?? a.partnerAccountId ?? ''}</TableCell>
                          <TableCell>{a.parameters ?? ''}{a.documentRef ? ` · ${a.documentRef}` : ''}</TableCell>
                          <TableCell>{t(`bankMatch_.kind.${a.kind}`)}</TableCell>
                          <TableCell align="right" sx={num}>{formatMoney(a.amount)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                ) : null}

                {split ? (
                  <Stack spacing={1}>
                    {split.map((row, index) => (
                      <Stack key={index} direction="row" spacing={1} alignItems="center">
                        <TextField size="small" label={t('bankMatch_.manualSplitAccount')} value={row.account}
                          onChange={(e) => setSplit(split.map((r, i) => i === index ? { ...r, account: e.target.value } : r))} />
                        <TextField size="small" type="number" label={t('bankMatch_.manualSplitPartner')} value={row.partnerAccountId ?? ''}
                          onChange={(e) => setSplit(split.map((r, i) => i === index ? { ...r, partnerAccountId: e.target.value ? Number(e.target.value) : null } : r))} />
                        <TextField size="small" label={t('bankMatch_.manualSplitReference')} value={row.parameters ?? ''}
                          onChange={(e) => setSplit(split.map((r, i) => i === index ? { ...r, parameters: e.target.value || null } : r))} />
                        <TextField size="small" type="number" label={t('bankMatch_.manualSplitAmount')} value={row.amount}
                          onChange={(e) => setSplit(split.map((r, i) => i === index ? { ...r, amount: Number(e.target.value) } : r))} />
                        <IconButton aria-label={t('bankMatch_.manualSplitRemove')} onClick={() => setSplit(split.filter((_, i) => i !== index))}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Stack>
                    ))}
                    <Typography variant="body2" color={splitOk ? 'text.secondary' : 'error'}>
                      {t('bankMatch_.manualSplitTotal', { total: formatMoney(splitTotal), amount: formatMoney(amountOf(selected)) })}
                    </Typography>
                    <Stack direction="row" spacing={1}>
                      <Button size="small" onClick={() => setSplit([...split, { account: '2040', amount: 0 }])}>{t('bankMatch_.manualSplitAdd')}</Button>
                      <Button size="small" variant="contained" disabled={!splitOk || accept.isPending}
                        onClick={() => accept.mutate({ line: selected, allocations: split })}>
                        {t('bankMatch_.accept')}
                      </Button>
                      <Button size="small" onClick={() => setSplit(null)}>{t('common.cancel')}</Button>
                    </Stack>
                  </Stack>
                ) : null}

                {!posted && !split ? (
                  <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                    {selected.status === 'Pending' ? (
                      <Button variant="contained" size="small" disabled={selected.allocations.length === 0 || accept.isPending}
                        onClick={() => accept.mutate({ line: selected })}>
                        {t('bankMatch_.accept')}
                      </Button>
                    ) : null}
                    {selected.status === 'Matched' ? (
                      <Button size="small" onClick={() => reopen.mutate(selected)}>{t('bankMatch_.reopen')}</Button>
                    ) : null}
                    <Button size="small" onClick={() => setSplit(selected.allocations.length > 0
                      ? selected.allocations.map(({ account, partnerAccountId, amount, subAccountId, parameters, documentRef, invoiceId, supplierInvoiceId, collectionPriority, closesDocumentType }) =>
                        ({ account, partnerAccountId, amount, subAccountId, parameters, documentRef, invoiceId, supplierInvoiceId, collectionPriority, closesDocumentType }))
                      : [{ account: '2040', amount: amountOf(selected) }])}>
                      {t('bankMatch_.manualSplit')}
                    </Button>
                    {partnerOfSelected != null && selected.bankAccountNumber ? (
                      <Button size="small" disabled={savePayer.isPending}
                        onClick={() => savePayer.mutate({ line: selected, partnerAccountId: partnerOfSelected })}>
                        {t('bankMatch_.savePayerAccount')}
                      </Button>
                    ) : null}
                    {partnerOfSelected != null ? (
                      <Button size="small" onClick={() => setTemplateName(selected.payerRecipientName)}>{t('bankMatch_.createTemplate')}</Button>
                    ) : null}
                  </Stack>
                ) : null}

                {!posted && selected.status !== 'Posted' ? (
                  <Box>
                    <TextField
                      inputRef={searchRef}
                      size="small"
                      fullWidth
                      label={t('bankMatch_.partnerSearchLabel')}
                      placeholder={t('bankMatch_.partnerSearchPlaceholder')}
                      value={search}
                      onChange={(e) => setSearch(e.target.value)}
                      onKeyDown={(e) => {
                        const first = partners.data?.[0]
                        if (e.key === 'Enter' && first) { e.preventDefault(); assign.mutate({ line: selected, partnerAccountId: first.partnerAccountId }) }
                        if (e.key === 'Escape') { e.stopPropagation(); setSearch(''); (e.target as HTMLElement).blur() }
                      }}
                    />
                    {partners.data && search.trim().length > 1 ? (
                      <List dense>
                        {partners.data.map((p) => (
                          <ListItemButton key={p.partnerAccountId} onClick={() => assign.mutate({ line: selected, partnerAccountId: p.partnerAccountId })}>
                            <ListItemText primary={p.partnerName} secondary={`${p.account} · ${p.accountNumber}`} />
                          </ListItemButton>
                        ))}
                      </List>
                    ) : null}
                  </Box>
                ) : null}
              </Stack>
            ) : null}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        {!posted ? (
          <>
            <Button onClick={() => rematch.mutate()} disabled={rematch.isPending}>{t('bankMatch_.rematch')}</Button>
            <Button onClick={() => acceptConfident.mutate()} disabled={acceptConfident.isPending}>{t('bankMatch_.acceptAllConfident')}</Button>
          </>
        ) : null}
        <Box sx={{ flex: 1 }} />
        {!posted && accepted < lines.length ? (
          <Typography variant="body2" color="text.secondary">{t('bankMatch_.blockedUnmatched')}</Typography>
        ) : null}
        {canPost && !posted ? (
          <Button variant="contained" disabled={accepted < lines.length || lines.length === 0 || post.isPending} onClick={() => post.mutate()}>
            {t('ledgerBanking.post')}
          </Button>
        ) : null}
        {canPost && posted ? (
          <Button color="warning" onClick={() => setConfirmUnpost(true)} disabled={unpost.isPending}>{t('bankMatch_.unpost')}</Button>
        ) : null}
        <Button onClick={onClose}>{t('bankImport_.close')}</Button>
      </DialogActions>

      <Dialog open={templateName != null} onClose={() => setTemplateName(null)} maxWidth="xs" fullWidth>
        <DialogTitle>{t('bankMatch_.createTemplateTitle')}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label={t('bankMatch_.createTemplateName')} value={templateName ?? ''} onChange={(e) => setTemplateName(e.target.value)} />
            <TextField label={t('bankMatch_.createTemplateSubAccount')} value={templateSubAccount} onChange={(e) => setTemplateSubAccount(e.target.value)} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setTemplateName(null)}>{t('common.cancel')}</Button>
          <Button variant="contained" disabled={!selected || partnerOfSelected == null || createTemplate.isPending}
            onClick={() => selected && partnerOfSelected != null && createTemplate.mutate({ line: selected, partnerAccountId: partnerOfSelected })}>
            {t('common.save')}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={confirmUnpost}
        title={t('bankMatch_.unpostConfirmTitle')}
        description={t('bankMatch_.unpostConfirmBody', { number: statement?.header.statementNumber })}
        confirmLabel={t('bankMatch_.unpost')}
        destructive
        pending={unpost.isPending}
        onClose={() => setConfirmUnpost(false)}
        onConfirm={() => { unpost.mutate(); setConfirmUnpost(false) }}
      />
    </Dialog>
  )
}
