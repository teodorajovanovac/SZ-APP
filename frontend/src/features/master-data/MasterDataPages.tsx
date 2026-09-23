import { Alert, Paper, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useActiveCompany } from '../companies/useActiveCompany'
import { AddressList, CompanyForm, PartnerList, StaffAccessList } from './index'
import { useContractHistory, useUnits } from './useMasterData'
import type { Unit } from './types'

export function CompanyPage() { const { activeCompany } = useActiveCompany(); return <CompanyForm companyId={activeCompany.id} /> }
export function PartnersPage() { const { activeCompany } = useActiveCompany(); return <PartnerList companyId={activeCompany.id} /> }
export function AddressesPage() { const { activeCompany } = useActiveCompany(); return <AddressList companyId={activeCompany.id} /> }
export function StaffPage() { const { activeCompany } = useActiveCompany(); return <StaffAccessList companyId={activeCompany.id} /> }

export function UnitsPage() {
  const { t } = useTranslation()
  const { activeCompany } = useActiveCompany()
  const units = useUnits(activeCompany.id, { page: 1, pageSize: 100 })
  const rows = units.data?.items ?? []
  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">Jedinice i ugovori</Typography>
      {units.isError && <Alert severity="error">Jedinice nisu dostupne.</Alert>}
      <Paper variant="outlined">
        <Typography variant="caption" color="text.secondary" sx={{ display: { xs: 'block', md: 'none' }, px: 2, pt: 1 }}>
          {t('ui.scrollHint')}
        </Typography>
        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small" sx={{ minWidth: 640 }}>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Oznaka</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Ulaz</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Površina</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Aktivni ugovor</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((unit) => <UnitRow key={unit.id} companyId={activeCompany.id} unit={unit} />)}
              {!units.isLoading && rows.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 6, border: 0, color: 'text.secondary' }}>
                    {t('table.empty')}
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Stack>
  )
}

function UnitRow({ companyId, unit }: { companyId: number; unit: Unit }) {
  const contracts = useContractHistory(companyId, unit.id)
  const active = contracts.data?.find(item => item.id === unit.contractId)
  return (
    <TableRow hover>
      <TableCell>{unit.name ?? `#${unit.id}`}</TableCell>
      <TableCell>{unit.buildingEntranceId ?? '—'}</TableCell>
      <TableCell align="right" sx={{ fontVariantNumeric: 'tabular-nums' }}>{unit.k1?.toFixed(2) ?? '—'}</TableCell>
      <TableCell>{active ? `${active.contractDate} – ${active.contractEndDate ?? 'aktivno'}` : '—'}</TableCell>
    </TableRow>
  )
}
