import PrintIcon from '@mui/icons-material/Print'
import {
  Alert,
  Button,
  GlobalStyles,
  MenuItem,
  Paper,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableFooter,
  TableHead,
  TablePagination,
  TableRow,
  Tabs,
  TextField,
  Typography,
} from '@mui/material'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { formatDate } from '../../shared/format/date'
import { formatAmount } from '../../shared/format/money'
import { AccountSelect, PartnerPicker } from './cardFilters'
import { JournalDialog } from './JournalDialog'
import { ledgerCardsApi, type CardGrouping, type LedgerCardRow } from './ledgerCardsApi'
import { PartnerBalancesPanel } from './PartnerBalancesPanel'
import { useUrlFilters } from './useUrlFilters'

const GROUPINGS: CardGrouping[] = ['none', 'paymentReference', 'document', 'openItems']
const num = { fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const
// Print: only the card itself (AppShell header/nav and the filter bar are hidden).
const printStyles = (
  <GlobalStyles styles={{ '@media print': { 'header, nav, aside, .no-print': { display: 'none !important' }, main: { margin: 0, padding: 0 } } }} />
)

export function LedgerCardsPage() {
  const { t } = useTranslation()
  const { get, set } = useUrlFilters()
  const tab = get('tab') === 'balances' ? 'balances' : 'card'

  return (
    <Stack spacing={2}>
      {printStyles}
      <Typography variant="h4">{t('cards_.title')}</Typography>
      <Tabs className="no-print" value={tab} onChange={(_, value) => set({ tab: value === 'card' ? null : value })}>
        <Tab value="card" label={t('cards_.tabCard')} />
        <Tab value="balances" label={t('balances_.title')} />
      </Tabs>
      {tab === 'card' ? <CardPanel /> : <PartnerBalancesPanel />}
    </Stack>
  )
}

function CardPanel() {
  const { t } = useTranslation()
  const { companyId, get, set } = useUrlFilters()
  const [journalId, setJournalId] = useState<number | null>(null)
  const page = Math.max(Number(get('page')) || 1, 1)
  const pageSize = Number(get('pageSize')) || 100
  const groupBy = (GROUPINGS.includes(get('groupBy') as CardGrouping) ? get('groupBy') : 'none') as CardGrouping
  const filters = {
    account: get('account'),
    partnerAccountId: get('partnerAccountId'),
    partnerId: get('partnerId'),
    subAccountId: get('subAccountId'),
    from: get('from'),
    to: get('to'),
    paymentReference: get('paymentReference'),
    document: get('document'),
  }
  const hasFilter = Boolean(filters.account || filters.partnerAccountId || filters.partnerId || filters.paymentReference || filters.document)
  const card = useQuery({
    queryKey: ['ledger-card', companyId, filters, groupBy, page, pageSize],
    queryFn: () => ledgerCardsApi.card(companyId, { ...filters, groupBy, page, pageSize }),
    enabled: hasFilter,
    placeholderData: keepPreviousData,
  })
  const partner = filters.partnerAccountId ? { partnerAccountId: Number(filters.partnerAccountId), label: get('partner') || `#${filters.partnerAccountId}` } : null
  const grouped = groupBy === 'paymentReference' || groupBy === 'document'
  const rows = card.data?.items ?? []
  // Carry-over into this page: the first row's running balance minus its own movement.
  const carry = rows.length > 0 ? rows[0].balance - rows[0].debit + rows[0].credit : card.data?.openingBalance ?? 0

  const openRow = (row: LedgerCardRow) => {
    if (row.journalEntryId != null) setJournalId(row.journalEntryId)
    else if (groupBy === 'paymentReference') set({ groupBy: null, paymentReference: row.groupKey })
    else if (groupBy === 'document') set({ groupBy: null, document: row.groupKey })
  }

  return (
    <Stack spacing={2}>
      <Stack className="no-print" direction="row" spacing={1.5} flexWrap="wrap" useFlexGap alignItems="center">
        <AccountSelect companyId={companyId} value={filters.account} onChange={(account) => set({ account })} />
        <PartnerPicker
          companyId={companyId}
          value={partner}
          onChange={(next) => set({ partnerAccountId: next?.partnerAccountId, partner: next?.label, partnerId: null })}
        />
        <TextField size="small" type="date" label={t('cards_.from')} value={filters.from} onChange={(e) => set({ from: e.target.value })} slotProps={{ inputLabel: { shrink: true } }} />
        <TextField size="small" type="date" label={t('cards_.to')} value={filters.to} onChange={(e) => set({ to: e.target.value })} slotProps={{ inputLabel: { shrink: true } }} />
        <TextField size="small" label={t('cards_.paymentReference')} value={filters.paymentReference} onChange={(e) => set({ paymentReference: e.target.value })} />
        <TextField size="small" label={t('cards_.document')} value={filters.document} onChange={(e) => set({ document: e.target.value })} sx={{ width: 130 }} />
        <TextField select size="small" label={t('cards_.groupBy')} value={groupBy} onChange={(e) => set({ groupBy: e.target.value === 'none' ? null : e.target.value })} sx={{ minWidth: 200 }}>
          {GROUPINGS.map((g) => <MenuItem key={g} value={g}>{t(`cards_.grouping.${g}`)}</MenuItem>)}
        </TextField>
        <Button startIcon={<PrintIcon />} onClick={() => window.print()} disabled={!card.data}>{t('cards_.print')}</Button>
      </Stack>

      {!hasFilter ? <Alert severity="info">{t('cards_.pickFilter')}</Alert> : null}
      {card.isError ? <Alert severity="error">{t('cards_.loadFailed')}</Alert> : null}

      {card.data ? (
        <Paper variant="outlined">
          <TableContainer sx={{ overflowX: 'auto' }}>
            <Table size="small" aria-label={t('cards_.title')} sx={{ minWidth: 960 }}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('cards_.col.date')}</TableCell>
                  <TableCell>{grouped ? t('cards_.col.count') : t('cards_.col.journal')}</TableCell>
                  <TableCell>{t('cards_.col.document')}</TableCell>
                  <TableCell>{t('cards_.col.description')}</TableCell>
                  <TableCell>{t('cards_.col.lineType')}</TableCell>
                  <TableCell>{t('cards_.col.dueDate')}</TableCell>
                  <TableCell>{t('cards_.col.paymentReference')}</TableCell>
                  <TableCell align="right">{t('cards_.col.debit')}</TableCell>
                  <TableCell align="right">{t('cards_.col.credit')}</TableCell>
                  <TableCell align="right">{t('cards_.col.balance')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                <TableRow sx={{ bgcolor: 'action.hover' }}>
                  <TableCell colSpan={9} sx={{ fontWeight: 600 }}>{page === 1 ? t('cards_.opening') : t('cards_.carry')}</TableCell>
                  <TableCell align="right" sx={{ ...num, fontWeight: 600 }}>{formatAmount(page === 1 ? card.data.openingBalance : carry)}</TableCell>
                </TableRow>
                {rows.map((row, index) => (
                  <TableRow
                    key={row.ledgerEntryId ?? `${row.groupKey}-${index}`}
                    hover
                    onDoubleClick={() => openRow(row)}
                    sx={{ cursor: 'pointer' }}
                    title={t('cards_.dblClickHint')}
                  >
                    <TableCell sx={num}>{formatDate(row.postingDate)}</TableCell>
                    <TableCell sx={num}>{grouped ? row.lineCount : row.journalEntryId}</TableCell>
                    <TableCell>{row.documentRef}</TableCell>
                    <TableCell sx={{ maxWidth: 280, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{row.description}</TableCell>
                    <TableCell>{row.lineType != null ? t(`cards_.lineTypes.${row.lineType}`, { defaultValue: String(row.lineType) }) : ''}</TableCell>
                    <TableCell sx={num}>{formatDate(row.dueDate)}</TableCell>
                    <TableCell sx={num}>{row.paymentReference}</TableCell>
                    <TableCell align="right" sx={num}>{formatAmount(row.debit)}</TableCell>
                    <TableCell align="right" sx={num}>{formatAmount(row.credit)}</TableCell>
                    <TableCell align="right" sx={{ ...num, color: row.balance < 0 ? 'error.main' : undefined }}>{formatAmount(row.balance)}</TableCell>
                  </TableRow>
                ))}
                {rows.length === 0 ? (
                  <TableRow><TableCell colSpan={10} align="center" sx={{ py: 4 }}>{t('cards_.empty')}</TableCell></TableRow>
                ) : null}
              </TableBody>
              <TableFooter>
                <TableRow>
                  <TableCell colSpan={7} sx={{ fontWeight: 700 }}>{t('cards_.totals')}</TableCell>
                  <TableCell align="right" sx={{ ...num, fontWeight: 700 }}>{formatAmount(card.data.totalDebit)}</TableCell>
                  <TableCell align="right" sx={{ ...num, fontWeight: 700 }}>{formatAmount(card.data.totalCredit)}</TableCell>
                  <TableCell align="right" sx={{ ...num, fontWeight: 700 }}>{formatAmount(card.data.closingBalance)}</TableCell>
                </TableRow>
              </TableFooter>
            </Table>
          </TableContainer>
          <TablePagination
            className="no-print"
            component="div"
            count={card.data.totalCount}
            page={page - 1}
            rowsPerPage={pageSize}
            rowsPerPageOptions={[50, 100, 250, 500]}
            onPageChange={(_, next) => set({ page: next + 1 })}
            onRowsPerPageChange={(e) => set({ pageSize: Number(e.target.value) })}
            labelRowsPerPage={t('table.rowsPerPage')}
          />
        </Paper>
      ) : null}
      <JournalDialog companyId={companyId} journalId={journalId} onClose={() => setJournalId(null)} />
    </Stack>
  )
}
