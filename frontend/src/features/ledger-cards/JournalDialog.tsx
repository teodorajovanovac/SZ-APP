import { Alert, Dialog, DialogContent, DialogTitle, Skeleton, Table, TableBody, TableCell, TableHead, TableRow } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { formatDate } from '../../shared/format/date'
import { formatAmount } from '../../shared/format/money'
import { ledgerCardsApi } from './ledgerCardsApi'

const num = { fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } as const

/** Read-only journal (nalog) detail opened by double-clicking a card row. */
export function JournalDialog({ companyId, journalId, onClose }: { companyId: number; journalId: number | null; onClose: () => void }) {
  const { t } = useTranslation()
  const journal = useQuery({
    queryKey: ['journal-detail', companyId, journalId],
    queryFn: () => ledgerCardsApi.journal(companyId, journalId!),
    enabled: journalId != null,
  })
  const header = journal.data?.header

  return (
    <Dialog open={journalId != null} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {t('cards_.journalTitle', { id: journalId })}
        {header ? ` · ${formatDate(header.postingDate)} · ${header.description}` : ''}
      </DialogTitle>
      <DialogContent>
        {journal.isError ? <Alert severity="error">{t('cards_.loadFailed')}</Alert> : null}
        {journal.isLoading ? <Skeleton height={120} /> : null}
        {journal.data ? (
          <Table size="small" aria-label={t('cards_.journalTitle', { id: journalId })}>
            <TableHead>
              <TableRow>
                <TableCell>{t('cards_.col.account')}</TableCell>
                <TableCell>{t('cards_.col.date')}</TableCell>
                <TableCell>{t('cards_.col.document')}</TableCell>
                <TableCell>{t('cards_.col.partnerAccount')}</TableCell>
                <TableCell align="right">{t('cards_.col.debit')}</TableCell>
                <TableCell align="right">{t('cards_.col.credit')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {journal.data.lines.map((line) => (
                <TableRow key={line.id}>
                  <TableCell>{line.account}{line.subAccountId ? ` / ${line.subAccountId}` : ''}</TableCell>
                  <TableCell>{formatDate(line.postingDate)}</TableCell>
                  <TableCell>{line.documentRef}</TableCell>
                  <TableCell>{line.partnerAccountId ?? ''}</TableCell>
                  <TableCell align="right" sx={num}>{formatAmount(line.debitAmount)}</TableCell>
                  <TableCell align="right" sx={num}>{formatAmount(line.creditAmount)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : null}
      </DialogContent>
    </Dialog>
  )
}
