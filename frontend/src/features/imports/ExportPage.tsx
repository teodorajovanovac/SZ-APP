import { useState } from 'react'
import FileDownloadOutlinedIcon from '@mui/icons-material/FileDownloadOutlined'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  FormGroup,
  MenuItem,
  Paper,
  Skeleton,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { ApiProblemError, apiRequest, type ProblemDetails } from '../../api/generated/client'
import { useAuth } from '../auth/useAuth'
import { useActiveCompany } from '../companies/useActiveCompany'
import { useCompanyRole } from '../companies/useCompanyRole'
import { saveBlob } from '../reports/reportsApi'

interface ExportTable {
  name: string
  group: 'CodeLists' | 'MasterData' | 'Transactions'
  reimportable: boolean
  importTable?: string | null
  hasPersonalData: boolean
}

const groups = ['CodeLists', 'MasterData', 'Transactions'] as const
const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

// apiRequest parses JSON; the ZIP needs the raw Response, so fetch it directly (GET, no CSRF).
async function downloadExport(path: string): Promise<{ blob: Blob; fileName: string }> {
  const response = await fetch(`${API_BASE_URL}${path}`, { credentials: 'include', headers: { Accept: 'application/zip' } })
  if (!response.ok) {
    let problem: ProblemDetails = { status: response.status, title: response.statusText }
    try { problem = { ...problem, ...await response.json() as ProblemDetails } } catch { /* status problem is sufficient */ }
    throw new ApiProblemError(problem)
  }
  const disposition = response.headers.get('content-disposition') ?? ''
  const fileName = /filename="?([^";]+)"?/.exec(disposition)?.[1] ?? 'szapp-export.zip'
  return { blob: await response.blob(), fileName }
}

interface LegacyExport { name: string; title: string; needsPeriod: boolean }

function LegacyExportSection({ companyId, encoding, all }: { companyId: number; encoding: string; all: boolean }) {
  const { t } = useTranslation()
  const [yymm, setYymm] = useState('')
  const list = useQuery({
    queryKey: ['export-legacy', companyId],
    queryFn: () => apiRequest<LegacyExport[]>(`/api/v1/companies/${companyId}/export/legacy`),
  })
  const run = useMutation({
    mutationFn: async (name: string) => {
      const query = new URLSearchParams({ encoding, ...(yymm ? { yymm } : {}) })
      const path = all ? `/api/v1/export/legacy/${name}?${query}` : `/api/v1/companies/${companyId}/export/legacy/${name}?${query}`
      const { blob, fileName } = await downloadExport(path)
      saveBlob(blob, fileName)
    },
  })
  return (
    <Paper variant="outlined" component="section" sx={{ p: { xs: 2, md: 3 } }}>
      <Stack spacing={2}>
        <Box>
          <Typography component="h2" variant="h6">{t('legacyExport_.title')}</Typography>
          <Typography color="text.secondary" variant="body2">{t('legacyExport_.subtitle')}</Typography>
        </Box>
        {run.isError ? <Alert severity="error">{t('legacyExport_.failed')}</Alert> : null}
        <TextField label={t('legacyExport_.period')} value={yymm} onChange={(e) => setYymm(e.target.value.replace(/\D/g, '').slice(0, 4))} sx={{ maxWidth: 200 }} inputProps={{ inputMode: 'numeric' }} />
        <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
          {(list.data ?? []).map((x) => (
            <Button
              key={x.name}
              variant="outlined"
              size="small"
              startIcon={<FileDownloadOutlinedIcon />}
              disabled={run.isPending || (x.needsPeriod && yymm.length !== 4)}
              title={x.needsPeriod && yymm.length !== 4 ? t('legacyExport_.needsPeriod') : x.title}
              onClick={() => run.mutate(x.name)}
            >
              {x.name}
            </Button>
          ))}
        </Stack>
      </Stack>
    </Paper>
  )
}

export function ExportPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const { activeCompany } = useActiveCompany()
  const isRoot = user?.roles.includes('Root') ?? false
  const { role } = useCompanyRole()
  const canPersonal = isRoot || role === 'Upravnik'
  const [selected, setSelected] = useState<Set<string> | null>(null)
  const [encoding, setEncoding] = useState<'cp1250' | 'utf8'>('cp1250')
  const [includePersonalData, setIncludePersonalData] = useState(false)
  const [scope, setScope] = useState<'company' | 'all'>('company')

  const tables = useQuery({
    queryKey: ['export-tables', activeCompany.id],
    queryFn: () => apiRequest<ExportTable[]>(`/api/v1/companies/${activeCompany.id}/export/tables`),
  })
  const items = tables.data ?? []
  const chosen = selected ?? new Set(items.map((x) => x.name))

  const toggle = (names: string[], on: boolean) => {
    const next = new Set(chosen)
    names.forEach((name) => (on ? next.add(name) : next.delete(name)))
    setSelected(next)
  }

  const run = useMutation({
    mutationFn: async () => {
      const query = new URLSearchParams({
        tables: items.filter((x) => chosen.has(x.name)).map((x) => x.name).join(','),
        encoding,
        includePersonalData: String(canPersonal && includePersonalData),
      })
      const path = isRoot && scope === 'all'
        ? `/api/v1/export?companyIds=all&${query}`
        : `/api/v1/companies/${activeCompany.id}/export?${query}`
      const { blob, fileName } = await downloadExport(path)
      saveBlob(blob, fileName)
    },
  })

  const allOn = items.length > 0 && items.every((x) => chosen.has(x.name))

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('export_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('export_.subtitle')}</Typography>
      </Box>

      {tables.isError ? <Alert severity="error">{t('export_.loadFailed')}</Alert> : null}
      {run.isError ? <Alert severity="error">{t('export_.failed')}</Alert> : null}
      {run.isSuccess ? <Alert severity="success">{t('export_.done')}</Alert> : null}

      <Paper variant="outlined" component="section" sx={{ p: { xs: 2, md: 3 } }}>
        {tables.isLoading ? [0, 1, 2].map((row) => <Skeleton key={row} height={40} />) : (
          <Stack spacing={2}>
            <FormControlLabel
              control={<Checkbox checked={allOn} indeterminate={!allOn && chosen.size > 0} onChange={(e) => toggle(items.map((x) => x.name), e.target.checked)} />}
              label={t('export_.selectAll')}
            />
            {groups.map((group) => {
              const inGroup = items.filter((x) => x.group === group)
              if (inGroup.length === 0) return null
              const groupOn = inGroup.every((x) => chosen.has(x.name))
              return (
                <Box key={group} component="fieldset" sx={{ border: 0, m: 0, p: 0 }}>
                  <FormControlLabel
                    control={<Checkbox checked={groupOn} indeterminate={!groupOn && inGroup.some((x) => chosen.has(x.name))} onChange={(e) => toggle(inGroup.map((x) => x.name), e.target.checked)} />}
                    label={<Typography component="legend" variant="subtitle1" sx={{ fontWeight: 600 }}>{t(`export_.groups.${group}`)}</Typography>}
                  />
                  <FormGroup row sx={{ pl: 4 }}>
                    {inGroup.map((table) => (
                      <FormControlLabel
                        key={table.name}
                        sx={{ minWidth: 240 }}
                        control={<Checkbox size="small" checked={chosen.has(table.name)} onChange={(e) => toggle([table.name], e.target.checked)} />}
                        label={
                          <Typography variant="body2">
                            {table.name}
                            <Typography component="span" variant="caption" color="text.secondary">
                              {' · '}{table.reimportable ? t('export_.reimportable') : t('export_.exportOnly')}
                            </Typography>
                          </Typography>
                        }
                      />
                    ))}
                  </FormGroup>
                </Box>
              )
            })}
          </Stack>
        )}
      </Paper>

      <Paper variant="outlined" component="section" sx={{ p: { xs: 2, md: 3 } }}>
        <Stack spacing={2}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
            <TextField select label={t('export_.encoding')} value={encoding} onChange={(e) => setEncoding(e.target.value as 'cp1250' | 'utf8')} sx={{ minWidth: 240 }}>
              <MenuItem value="cp1250">Windows-1250</MenuItem>
              <MenuItem value="utf8">UTF-8</MenuItem>
            </TextField>
            {isRoot ? (
              <TextField select label={t('export_.scope')} value={scope} onChange={(e) => setScope(e.target.value as 'company' | 'all')} sx={{ minWidth: 240 }}>
                <MenuItem value="company">{t('export_.scopeCompany', { name: activeCompany.name })}</MenuItem>
                <MenuItem value="all">{t('export_.scopeAll')}</MenuItem>
              </TextField>
            ) : null}
          </Stack>
          {canPersonal ? (
            <>
              <FormControlLabel
                control={<Switch checked={includePersonalData} onChange={(e) => setIncludePersonalData(e.target.checked)} />}
                label={t('export_.personalData')}
              />
              {includePersonalData ? <Alert severity="warning">{t('export_.personalWarning')}</Alert> : null}
            </>
          ) : null}
          <Box>
            <Button
              variant="contained"
              startIcon={<FileDownloadOutlinedIcon />}
              disabled={chosen.size === 0 || items.length === 0 || run.isPending}
              onClick={() => run.mutate()}
            >
              {run.isPending ? t('export_.running') : t('export_.submit')}
            </Button>
          </Box>
        </Stack>
      </Paper>

      <LegacyExportSection companyId={activeCompany.id} encoding={encoding} all={isRoot && scope === 'all'} />
    </Stack>
  )
}
