import { useState } from 'react'
import { Alert, Button, Card, CardActions, CardContent, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from '@mui/material'
import { useActiveCompany } from '../companies/useActiveCompany'
import { openPrintableHtml, saveBlob, useAnalysisDefinitions, useExportReport, useReportDefinitions, useRunAnalysis, useRunReport } from './reportsApi'
import { parseReportParameters } from './reportParameters'
import type { ReportTable } from './types'

export function ReportsPage() {
  const { activeCompany } = useActiveCompany()
  const [rawParameters, setRawParameters] = useState('{}')
  const [table, setTable] = useState<ReportTable>()
  const [error, setError] = useState<string>()
  const analyses = useAnalysisDefinitions(activeCompany.id)
  const reports = useReportDefinitions(activeCompany.id)
  const runAnalysis = useRunAnalysis(activeCompany.id)
  const runReport = useRunReport(activeCompany.id)
  const exportReport = useExportReport(activeCompany.id)
  const parameters = () => { try { setError(undefined); return parseReportParameters(rawParameters) } catch (e) { setError(e instanceof Error ? e.message : 'Neispravni parametri.'); throw e } }
  return <Stack spacing={3}><Typography variant="h4">Analize i izveštaji</Typography>
    {(analyses.isError || reports.isError || error) && <Alert severity="error">{error ?? 'Definicije izveštaja nisu dostupne.'}</Alert>}
    <TextField label="Parametri (JSON)" value={rawParameters} onChange={e => setRawParameters(e.target.value)} multiline minRows={2}/>
    <Typography variant="h5">Analize</Typography>
    <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} flexWrap="wrap">{analyses.data?.map(item => <Card key={item.id} variant="outlined" sx={{ minWidth: 280 }}><CardContent><Typography variant="h6">{item.name}</Typography><Typography color="text.secondary">{item.dataGroup}</Typography>{item.hasBlockedLegacyAction && <Alert severity="warning">Legacy action je blokirana.</Alert>}</CardContent><CardActions><Button onClick={() => runAnalysis.mutate({ id: item.id, parameters: parameters() }, { onSuccess: value => setTable(value.result) })}>Pokreni</Button></CardActions></Card>)}</Stack>
    <Typography variant="h5">Izveštaji</Typography>
    <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} flexWrap="wrap">{reports.data?.map(item => <Card key={item.id} variant="outlined" sx={{ minWidth: 280 }}><CardContent><Typography variant="h6">{item.title}</Typography></CardContent><CardActions><Button onClick={() => runReport.mutate({ id: item.id, parameters: parameters() }, { onSuccess: value => setTable(value.sections[0]) })}>Pokreni</Button><Button onClick={() => exportReport.mutate({ kind: 'reports', id: item.id, format: 'export.csv', parameters: parameters() }, { onSuccess: blob => saveBlob(blob, `${item.name}.csv`) })}>CSV</Button><Button onClick={() => exportReport.mutate({ kind: 'reports', id: item.id, format: 'print', parameters: parameters() }, { onSuccess: openPrintableHtml })}>Štampa</Button></CardActions></Card>)}</Stack>
    {table && <ReportResult table={table}/>}</Stack>
}

function ReportResult({ table }: { table: ReportTable }) {
  return <Paper sx={{ overflowX: 'auto', p: 1 }}><Typography variant="h6">{table.name} ({table.rowCount})</Typography>{table.isTruncated && <Alert severity="warning">Rezultat je skraćen na serverski limit.</Alert>}<Table size="small"><TableHead><TableRow>{table.columns.map(column => <TableCell key={column}>{column}</TableCell>)}</TableRow></TableHead><TableBody>{table.rows.map((row, i) => <TableRow key={i}>{row.map((cell, j) => <TableCell key={j}>{String(cell ?? '')}</TableCell>)}</TableRow>)}</TableBody></Table></Paper>
}
