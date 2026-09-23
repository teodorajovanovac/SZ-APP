import { useMemo, useState } from 'react'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Grid,
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
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { useActiveCompany } from '../companies/useActiveCompany'
import {
  openPrintableHtml,
  saveBlob,
  useAnalysisDefinitions,
  useExportReport,
  useReportDefinitions,
  useRunAnalysis,
  useRunReport,
} from './reportsApi'
import { parseReportParameters } from './reportParameters'
import type { ReportParameters, ReportTable } from './types'

/**
 * ponytail: the API exposes no per-report parameter metadata (AnalysisDefinitionResponse /
 * ReportDefinitionResponse carry only a free-text `filterCaption`), so a structured
 * parameter form cannot be generated. The JSON escape hatch stays, but demoted behind an
 * "advanced" disclosure with live validation. Replace it with generated fields the moment
 * the backend publishes a parameter schema.
 */
export function ReportsPage() {
  const { t } = useTranslation()
  const { activeCompany } = useActiveCompany()
  const [rawParameters, setRawParameters] = useState('')
  const [table, setTable] = useState<ReportTable>()
  const analyses = useAnalysisDefinitions(activeCompany.id)
  const reports = useReportDefinitions(activeCompany.id)
  const runAnalysis = useRunAnalysis(activeCompany.id)
  const runReport = useRunReport(activeCompany.id)
  const exportReport = useExportReport(activeCompany.id)

  // Parse once per keystroke so the field can show its own error instead of failing on Run.
  const parsed = useMemo<{ value?: ReportParameters; error?: string }>(() => {
    try {
      return { value: parseReportParameters(rawParameters) }
    } catch (e) {
      return { error: e instanceof Error ? e.message : 'Neispravni parametri.' }
    }
  }, [rawParameters])

  const parameters = parsed.value ?? {}
  const disabled = Boolean(parsed.error)
  const runError = runAnalysis.error ?? runReport.error ?? exportReport.error
  const definitionsError = analyses.isError || reports.isError

  return (
    <Stack spacing={3}>
      <Typography component="h1" variant="h1">Analize i izveštaji</Typography>
      <Typography color="text.secondary">{t('reports_.intro')}</Typography>

      {definitionsError ? <Alert severity="error">Definicije izveštaja nisu dostupne.</Alert> : null}
      {runError ? <Alert severity="error">{getErrorMessage(runError, 'Izveštaj nije mogao da se pokrene.')}</Alert> : null}

      <Accordion disableGutters variant="outlined" sx={{ '&::before': { display: 'none' } }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />} aria-controls="report-parameters" id="report-parameters-header">
          <Typography>{t('reports_.advanced')}</Typography>
        </AccordionSummary>
        <AccordionDetails id="report-parameters">
          <TextField
            fullWidth
            size="small"
            multiline
            minRows={2}
            label={t('reports_.parametersLabel')}
            value={rawParameters}
            onChange={(event) => setRawParameters(event.target.value)}
            error={disabled}
            helperText={parsed.error ?? `${t('reports_.parametersHelp')} ${t('reports_.parametersExample')}`}
            slotProps={{ htmlInput: { spellCheck: false, style: { fontFamily: 'monospace' } } }}
          />
        </AccordionDetails>
      </Accordion>

      <Section
        title={t('reports_.analysesTitle')}
        isLoading={analyses.isLoading}
        isEmpty={!analyses.isLoading && (analyses.data?.length ?? 0) === 0}
        emptyMessage={t('reports_.analysesEmpty')}
      >
        {analyses.data?.map((item) => (
          <Grid key={item.id} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card variant="outlined" sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
              <CardContent sx={{ flexGrow: 1 }}>
                <Typography variant="h6" component="h3">{item.name}</Typography>
                <Typography color="text.secondary" variant="body2">{item.dataGroup}</Typography>
                {item.description ? <Typography variant="body2" sx={{ mt: 1 }}>{item.description}</Typography> : null}
                {item.filterCaption ? (
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                    {t('reports_.filterHint', { caption: item.filterCaption })}
                  </Typography>
                ) : null}
                {item.hasBlockedLegacyAction ? <Alert severity="warning" sx={{ mt: 1 }}>{t('reports_.blockedLegacy')}</Alert> : null}
              </CardContent>
              <CardActions>
                <Button
                  startIcon={<PlayArrowIcon />}
                  disabled={disabled || runAnalysis.isPending}
                  onClick={() => runAnalysis.mutate({ id: item.id, parameters }, { onSuccess: (value) => setTable(value.result) })}
                >
                  {t('reports_.run')}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Section>

      <Section
        title={t('reports_.reportsTitle')}
        isLoading={reports.isLoading}
        isEmpty={!reports.isLoading && (reports.data?.length ?? 0) === 0}
        emptyMessage={t('reports_.reportsEmpty')}
      >
        {reports.data?.map((item) => (
          <Grid key={item.id} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card variant="outlined" sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
              <CardContent sx={{ flexGrow: 1 }}>
                <Typography variant="h6" component="h3">{item.title}</Typography>
                {item.filterCaption ? (
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                    {t('reports_.filterHint', { caption: item.filterCaption })}
                  </Typography>
                ) : null}
              </CardContent>
              <CardActions>
                <Button
                  startIcon={<PlayArrowIcon />}
                  disabled={disabled || runReport.isPending}
                  onClick={() => runReport.mutate({ id: item.id, parameters }, { onSuccess: (value) => setTable(value.sections[0]) })}
                >
                  {t('reports_.run')}
                </Button>
                <Button
                  disabled={disabled || exportReport.isPending}
                  onClick={() => exportReport.mutate({ kind: 'reports', id: item.id, format: 'export.csv', parameters }, { onSuccess: (blob) => saveBlob(blob, `${item.name}.csv`) })}
                >
                  {t('reports_.csv')}
                </Button>
                <Button
                  disabled={disabled || exportReport.isPending}
                  onClick={() => exportReport.mutate({ kind: 'reports', id: item.id, format: 'print', parameters }, { onSuccess: openPrintableHtml })}
                >
                  {t('reports_.print')}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Section>

      {table ? <ReportResult table={table} /> : null}
    </Stack>
  )
}

function Section({
  title,
  isLoading,
  isEmpty,
  emptyMessage,
  children,
}: {
  title: string
  isLoading: boolean
  isEmpty: boolean
  emptyMessage: string
  children: React.ReactNode
}) {
  const { t } = useTranslation()
  return (
    <Box component="section">
      <Typography variant="h5" component="h2" sx={{ mb: 1.5 }}>{title}</Typography>
      {isLoading ? (
        <Grid container spacing={2}>
          {[0, 1, 2].map((index) => (
            <Grid key={index} size={{ xs: 12, sm: 6, lg: 4 }}>
              <Skeleton variant="rounded" height={140} />
            </Grid>
          ))}
          <Grid size={12}><Typography role="status" variant="caption" color="text.secondary">{t('reports_.loading')}</Typography></Grid>
        </Grid>
      ) : isEmpty ? (
        <Paper variant="outlined" sx={{ p: 4, textAlign: 'center', color: 'text.secondary' }}>
          <Typography>{emptyMessage}</Typography>
        </Paper>
      ) : (
        <Grid container spacing={2}>{children}</Grid>
      )}
    </Box>
  )
}

function ReportResult({ table }: { table: ReportTable }) {
  const { t } = useTranslation()
  return (
    <Paper variant="outlined" component="section">
      <Stack spacing={1} sx={{ p: 2, pb: 1 }}>
        <Typography variant="h6" component="h2">{table.name} ({table.rowCount})</Typography>
        {table.isTruncated ? <Alert severity="warning">Rezultat je skraćen na serverski limit.</Alert> : null}
      </Stack>
      {table.rows.length === 0 ? (
        <Typography sx={{ p: 4, textAlign: 'center', color: 'text.secondary' }}>{t('reports_.resultEmpty')}</Typography>
      ) : (
        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                {table.columns.map((column) => (
                  <TableCell key={column} sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{column}</TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {table.rows.map((row, i) => (
                <TableRow key={i} hover>
                  {row.map((cell, j) => (
                    <TableCell
                      key={j}
                      align={typeof cell === 'number' ? 'right' : 'left'}
                      sx={typeof cell === 'number' ? { fontVariantNumeric: 'tabular-nums', whiteSpace: 'nowrap' } : undefined}
                    >
                      {String(cell ?? '')}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Paper>
  )
}
