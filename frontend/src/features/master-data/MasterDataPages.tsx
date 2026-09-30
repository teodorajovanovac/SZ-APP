import { Alert, Link, Stack } from '@mui/material'
import type { ColumnDef } from '@tanstack/react-table'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'
import { useActiveCompany } from '../companies/useActiveCompany'
import { formatDate } from '../../shared/format/date'
import { PageHeader } from '../../shared/components/PageHeader'
import { SearchField } from '../../shared/components/SearchField'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { urlTableProps, useUrlState } from '../../shared/hooks/useUrlState'
import { AddressList, PartnerList } from './index'
import { BuildingEntranceList } from './components/BuildingEntranceList'
import { LocationCategoryList } from './components/LocationCategoryList'
import { useUnits } from './useMasterData'
import type { Unit } from './types'

export { CompanyListPage as CompanyPage } from '../companies/CompanyListPage'
export function BuildingEntrancesPage() { const { activeCompany } = useActiveCompany(); return <BuildingEntranceList companyId={activeCompany.id} /> }
export function LocationCategoriesPage() { const { activeCompany } = useActiveCompany(); return <LocationCategoryList companyId={activeCompany.id} /> }
export function PartnersPage() { const { activeCompany } = useActiveCompany(); return <PartnerList companyId={activeCompany.id} /> }
export function AddressesPage() { const { activeCompany } = useActiveCompany(); return <AddressList companyId={activeCompany.id} /> }

export function UnitsPage() {
  const { t } = useTranslation()
  const { activeCompany } = useActiveCompany()
  const companyId = activeCompany.id
  // The API orders by sorting number; no column sort.
  const [url, setUrl] = useUrlState({ q: '', page: 0, size: 50 })
  const table = urlTableProps(url, setUrl)
  const units = useUnits(companyId, { page: url.page + 1, pageSize: url.size, search: url.q })
  const columns = useMemo<ColumnDef<Unit>[]>(
    () => [
      {
        id: 'name',
        header: t('fields.unitLabel'),
        cell: ({ row }) => (
          <Link component={RouterLink} to={`/units/${companyId}/${row.original.id}`}>
            {row.original.name ?? `#${row.original.id}`}
          </Link>
        ),
      },
      {
        id: 'entrance',
        header: t('fields.entrance'),
        cell: ({ row }) =>
          row.original.buildingEntranceId ? (
            <Link component={RouterLink} to={`/building-entrances/${companyId}/${row.original.buildingEntranceId}`}>
              #{row.original.buildingEntranceId}
            </Link>
          ) : t('ui.noValue'),
      },
      { id: 'k1', header: t('fields.area'), meta: { numeric: true }, cell: ({ row }) => row.original.k1?.toLocaleString('sr-Latn-RS', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) ?? t('ui.noValue') },
      {
        id: 'contract',
        header: t('fields.activeContract'),
        cell: ({ row }) => {
          const unit = row.original
          if (!unit.activeContractDate) return t('ui.noValue')
          return `${formatDate(unit.activeContractDate)} – ${unit.activeContractEndDate ? formatDate(unit.activeContractEndDate) : t('ui.active')}`
        },
      },
    ],
    [companyId, t],
  )
  return (
    <Stack spacing={2}>
      <PageHeader
        title={t('units_.title')}
        subtitle={t('units_.subtitle')}
        filters={<SearchField label={t('units_.search')} value={url.q} onChange={(q) => setUrl({ q })} />}
      />
      {units.isError && <Alert severity="error">{t('units_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('units_.title')}
        rows={units.data?.items ?? []}
        columns={columns}
        rowCount={units.data?.totalCount ?? 0}
        pagination={table.pagination}
        onPaginationChange={table.onPaginationChange}
        isLoading={units.isLoading}
        emptyMessage={url.q ? undefined : t('units_.empty')}
        getRowId={(row) => String(row.id)}
        minWidth={640}
      />
    </Stack>
  )
}
