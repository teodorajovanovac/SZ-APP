import { Alert, Button, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from '@mui/material'
import { useInfiniteQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { apiRequest } from '../../api/generated/client'
import { getErrorMessage } from '../../api/problemDetails'
import { useCompanyRole } from '../companies/useCompanyRole'

type AuditRow = { id: number; timestamp: string; staffName: string | null; action: string; entityType: string; itemId: string | null; detailsJson: string | null }

/** GAP-24: company change history (EF audit interceptor), newest first, keyset paging by id. */
export function AuditLogPage() {
  const { companyId } = useCompanyRole()
  const [filter, setFilter] = useState({ entity: '', itemId: '', from: '', to: '' })
  const params = new URLSearchParams(Object.entries(filter).filter(([, v]) => v))
  const rows = useInfiniteQuery({
    queryKey: ['audit', companyId, params.toString()],
    enabled: companyId > 0,
    initialPageParam: 0,
    queryFn: ({ pageParam }) => {
      const q = new URLSearchParams(params)
      if (pageParam) q.set('before', String(pageParam))
      return apiRequest<AuditRow[]>(`/api/v1/companies/${companyId}/audit?${q}`)
    },
    getNextPageParam: (last) => (last.length === 100 ? last.at(-1)?.id : undefined),
  })
  const set = (key: keyof typeof filter) => (e: React.ChangeEvent<HTMLInputElement>) => setFilter({ ...filter, [key]: e.target.value })

  return (
    <Stack spacing={2}>
      <Typography variant="h5" component="h1">Istorija izmena</Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <TextField size="small" label="Entitet" value={filter.entity} onChange={set('entity')} />
        <TextField size="small" label="ID" value={filter.itemId} onChange={set('itemId')} />
        <TextField size="small" type="date" label="Od" value={filter.from} onChange={set('from')} slotProps={{ inputLabel: { shrink: true } }} />
        <TextField size="small" type="date" label="Do" value={filter.to} onChange={set('to')} slotProps={{ inputLabel: { shrink: true } }} />
      </Stack>
      {rows.error ? <Alert severity="error">{getErrorMessage(rows.error, 'Greška pri učitavanju.')}</Alert> : null}
      <Table size="small">
        <TableHead><TableRow>
          <TableCell>Vreme</TableCell><TableCell>Korisnik</TableCell><TableCell>Akcija</TableCell>
          <TableCell>Entitet</TableCell><TableCell>ID</TableCell><TableCell>Detalji</TableCell>
        </TableRow></TableHead>
        <TableBody>
          {rows.data?.pages.flat().map((r) => (
            <TableRow key={r.id}>
              <TableCell sx={{ whiteSpace: 'nowrap' }}>{new Date(r.timestamp).toLocaleString('sr-Latn-RS')}</TableCell>
              <TableCell>{r.staffName ?? '—'}</TableCell>
              <TableCell>{r.action}</TableCell>
              <TableCell>{r.entityType}</TableCell>
              <TableCell>{r.itemId}</TableCell>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: 12, wordBreak: 'break-all' }}>{r.detailsJson}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {rows.hasNextPage ? <Button onClick={() => rows.fetchNextPage()} disabled={rows.isFetchingNextPage}>Učitaj još</Button> : null}
    </Stack>
  )
}
