import { useState } from 'react'
import CloudUploadOutlinedIcon from '@mui/icons-material/CloudUploadOutlined'
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined'
import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
  MenuItem,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'
import { useActiveCompany } from '../companies/useActiveCompany'

interface EtlRun {
  id: string
  sourceFile: string
  sourceTable: string
  stage: string
  status: string
  sourceRows: number
  importedRows: number
  quarantinedRows: number
  error?: string | null
}

// Import order matters: parents before children.
const tables = [
  'ShortList', 'Languages', 'Staff', 'Address', 'Partner', 'Company', 'BuildingEntrance', 'Unit',
  'Contract', 'PartnerAccount', 'BankAccount', 'InvoiceBatch', 'Invoice', 'SupplierInvoice',
  'BankStatement', 'JournalEntry', 'LedgerEntry', 'Notice', 'InterestStatement', 'SentEmail', 'Documents',
]

export function EtlRunsPage() {
  const { t } = useTranslation()
  const { activeCompany } = useActiveCompany()
  const client = useQueryClient()
  const [file, setFile] = useState<File>()
  const [sourceTable, setSourceTable] = useState('ShortList')
  const key = ['etl-runs', activeCompany.id]

  const runs = useQuery({
    queryKey: key,
    queryFn: () => apiRequest<EtlRun[]>(`/api/v1/companies/${activeCompany.id}/etl-runs`),
  })
  const upload = useMutation({
    mutationFn: () => {
      if (!file) throw new Error(t('imports_.noFile'))
      const form = new FormData()
      form.append('file', file)
      form.append('sourceTable', sourceTable)
      form.append('sourceSystem', 'Access')
      form.append('encoding', 'windows-1250')
      form.append('delimiter', ';')
      return apiRequest<EtlRun>(`/api/v1/companies/${activeCompany.id}/etl-runs/import`, { method: 'POST', body: form })
    },
    onSuccess: () => client.invalidateQueries({ queryKey: key }),
  })
  const execute = useMutation({
    mutationFn: (id: string) =>
      apiRequest<EtlRun>(`/api/v1/companies/${activeCompany.id}/etl-runs/${id}/execute`, { method: 'POST' }),
    onSuccess: () => client.invalidateQueries({ queryKey: key }),
  })

  const items = runs.data ?? []

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('imports_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('imports_.subtitle')}</Typography>
      </Box>

      {upload.isError || execute.isError ? <Alert severity="error">{t('imports_.importFailed')}</Alert> : null}
      {runs.isError ? <Alert severity="error">{t('imports_.loadFailed')}</Alert> : null}

      <Paper variant="outlined" component="section" sx={{ p: { xs: 2, md: 3 } }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ md: 'center' }}>
          <TextField
            select
            label={t('imports_.table')}
            value={sourceTable}
            onChange={(event) => setSourceTable(event.target.value)}
            sx={{ minWidth: 220 }}
          >
            {tables.map((name) => <MenuItem key={name} value={name}>{name}</MenuItem>)}
          </TextField>
          <Button component="label" variant="outlined" startIcon={<CloudUploadOutlinedIcon />}>
            {file?.name ?? t('imports_.chooseCsv')}
            <input
              hidden
              type="file"
              aria-label={t('imports_.chooseCsv')}
              accept=".csv,.txt"
              onChange={(event) => setFile(event.target.files?.[0])}
            />
          </Button>
          <Button variant="contained" disabled={!file || upload.isPending} onClick={() => upload.mutate()}>
            {upload.isPending ? t('imports_.validating') : t('imports_.validate')}
          </Button>
        </Stack>
      </Paper>

      <Paper variant="outlined" component="section">
        <Typography component="h2" variant="h6" sx={{ p: 2 }}>{t('imports_.runsTitle')}</Typography>
        <Divider />
        {runs.isLoading ? (
          <Box sx={{ p: 2 }}>{[0, 1, 2].map((row) => <Skeleton key={row} height={40} />)}</Box>
        ) : items.length === 0 ? (
          <Stack spacing={1} alignItems="center" sx={{ px: 3, py: 6, textAlign: 'center' }}>
            <InboxOutlinedIcon fontSize="large" color="disabled" aria-hidden="true" />
            <Typography component="p" variant="subtitle1">{t('imports_.emptyTitle')}</Typography>
            <Typography color="text.secondary" sx={{ maxWidth: 440 }}>{t('imports_.emptyBody')}</Typography>
          </Stack>
        ) : (
          <TableContainer>
            <Table size="small" aria-label={t('imports_.tableLabel')}>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 600 }}>{t('imports_.columns.file')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>{t('imports_.columns.table')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>{t('imports_.columns.stage')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }} align="right">{t('imports_.columns.rows')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }} align="right">{t('imports_.columns.quarantined')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>{t('imports_.columns.action')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {items.map((run) => (
                  <TableRow key={run.id} hover>
                    <TableCell>
                      {run.sourceFile}
                      {run.error ? (
                        <Typography variant="caption" color="error" display="block">
                          {t('imports_.errorRow', { message: run.error })}
                        </Typography>
                      ) : null}
                    </TableCell>
                    <TableCell>{run.sourceTable}</TableCell>
                    <TableCell><Chip size="small" label={run.stage} /></TableCell>
                    <TableCell align="right">{run.importedRows}/{run.sourceRows}</TableCell>
                    <TableCell align="right">
                      {run.quarantinedRows > 0 ? (
                        <Chip size="small" color="warning" label={run.quarantinedRows} />
                      ) : (
                        run.quarantinedRows
                      )}
                    </TableCell>
                    <TableCell>
                      {run.stage === 'Staged' ? (
                        <Button
                          size="small"
                          variant="outlined"
                          disabled={execute.isPending}
                          onClick={() => execute.mutate(run.id)}
                        >
                          {t('imports_.execute')}
                        </Button>
                      ) : null}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>
    </Stack>
  )
}
