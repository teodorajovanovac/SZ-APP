import { useState } from 'react'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import {
  Accordion, AccordionDetails, AccordionSummary, Alert, Button, Checkbox, Chip, FormControlLabel,
  Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography,
} from '@mui/material'
import { useMutation } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'

interface ConsistencyRow { companyId: number; key: string; info: string; amount: number | null }
interface ConsistencyCheckResult { id: string; legacy: string; name: string; critical: boolean; count: number; rows: ConsistencyRow[] }
interface ConsistencyReport { companyId: number | null; checks: ConsistencyCheckResult[]; notTranslated: { id: string; reason: string }[] }

/** ERR-01/03: legacy ERROR_* checks, read-only, run on demand (Root/Upravnik). */
export function ConsistencyChecksPanel({ companyId, isRoot }: { companyId: number; isRoot: boolean }) {
  const { t } = useTranslation()
  const [allCompanies, setAllCompanies] = useState(false)
  const run = useMutation({
    mutationFn: () => apiRequest<ConsistencyReport>(
      `/api/v1/companies/${companyId}/consistency-checks?allCompanies=${isRoot && allCompanies}`),
  })

  return (
    <Stack spacing={2} component="section" aria-labelledby="consistency-title">
      <Typography id="consistency-title" variant="h6" component="h2">{t('consistency_.title')}</Typography>
      <Typography color="text.secondary" variant="body2">{t('consistency_.subtitle')}</Typography>
      <Stack direction="row" spacing={2} alignItems="center">
        <Button variant="contained" disabled={run.isPending} onClick={() => run.mutate()}>{t('consistency_.run')}</Button>
        {isRoot ? (
          <FormControlLabel
            control={<Checkbox checked={allCompanies} onChange={(e) => setAllCompanies(e.target.checked)} />}
            label={t('consistency_.allCompanies')}
          />
        ) : null}
      </Stack>
      {run.error ? <Alert severity="error">{getErrorMessage(run.error, t('consistency_.failed'))}</Alert> : null}
      {run.data?.checks.map((check) => (
        <Accordion key={check.id} disabled={check.count === 0}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Stack direction="row" spacing={1} alignItems="center">
              <Chip size="small" color={check.count === 0 ? 'success' : check.critical ? 'error' : 'warning'} label={check.count} />
              <Typography>{check.id} — {check.name}</Typography>
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Typography variant="caption" color="text.secondary">{check.legacy}</Typography>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>{t('consistency_.company')}</TableCell>
                  <TableCell>{t('consistency_.key')}</TableCell>
                  <TableCell>{t('consistency_.info')}</TableCell>
                  <TableCell align="right">{t('consistency_.amount')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {check.rows.map((row, index) => (
                  <TableRow key={index}>
                    <TableCell>{row.companyId}</TableCell>
                    <TableCell>{row.key}</TableCell>
                    <TableCell>{row.info}</TableCell>
                    <TableCell align="right">{row.amount === null ? '' : formatMoney(row.amount)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {check.count > check.rows.length ? (
              <Typography variant="caption">{t('consistency_.truncated', { shown: check.rows.length, total: check.count })}</Typography>
            ) : null}
          </AccordionDetails>
        </Accordion>
      ))}
      {run.data && run.data.notTranslated.length > 0 ? (
        <Alert severity="info">
          <Typography variant="subtitle2">{t('consistency_.notTranslated')}</Typography>
          {run.data.notTranslated.map((x) => <div key={x.id}><b>{x.id}</b>: {x.reason}</div>)}
        </Alert>
      ) : null}
    </Stack>
  )
}
