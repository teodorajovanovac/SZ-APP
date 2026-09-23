import { Alert, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material'
import { useActiveCompany } from '../companies/useActiveCompany'
import { AddressList, CompanyForm, PartnerList, StaffAccessList } from './index'
import { useContractHistory, useUnits } from './useMasterData'
import type { Unit } from './types'

export function CompanyPage() { const { activeCompany } = useActiveCompany(); return <CompanyForm companyId={activeCompany.id} /> }
export function PartnersPage() { const { activeCompany } = useActiveCompany(); return <PartnerList companyId={activeCompany.id} onSelect={() => undefined} /> }
export function AddressesPage() { const { activeCompany } = useActiveCompany(); return <AddressList companyId={activeCompany.id} onSelect={() => undefined} /> }
export function StaffPage() { const { activeCompany } = useActiveCompany(); return <StaffAccessList companyId={activeCompany.id} onSelect={() => undefined} /> }

export function UnitsPage() {
  const { activeCompany } = useActiveCompany()
  const units = useUnits(activeCompany.id, { page: 1, pageSize: 100 })
  return <Stack spacing={2}><Typography component="h1" variant="h1">Jedinice i ugovori</Typography>
    {units.isError && <Alert severity="error">Jedinice nisu dostupne.</Alert>}
    <Paper sx={{ overflowX: 'auto' }}><Table size="small"><TableHead><TableRow><TableCell>Oznaka</TableCell><TableCell>Ulaz</TableCell><TableCell>Površina</TableCell><TableCell>Aktivni ugovor</TableCell></TableRow></TableHead>
      <TableBody>{units.data?.items.map(unit => <UnitRow key={unit.id} companyId={activeCompany.id} unit={unit}/>)}</TableBody></Table></Paper>
  </Stack>
}

function UnitRow({ companyId, unit }: { companyId: number; unit: Unit }) {
  const contracts = useContractHistory(companyId, unit.id)
  const active = contracts.data?.find(item => item.id === unit.contractId)
  return <TableRow><TableCell>{unit.name ?? `#${unit.id}`}</TableCell><TableCell>{unit.buildingEntranceId ?? '—'}</TableCell><TableCell>{unit.k1?.toFixed(2) ?? '—'}</TableCell><TableCell>{active ? `${active.contractDate} – ${active.contractEndDate ?? 'aktivno'}` : '—'}</TableCell></TableRow>
}
