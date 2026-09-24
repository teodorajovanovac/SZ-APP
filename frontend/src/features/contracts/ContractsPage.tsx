import { useMemo, useState, type ReactNode } from 'react'
import {
  Alert,
  FormControl,
  InputLabel,
  Link,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { companyScopeToQueryParams } from '../companies/companyScope'
import { useCompanyScope } from '../companies/useCompanyScope'
import { useContracts } from './useContracts'
import type { ContractOverviewRow, ContractsMode } from './types'

export function ContractsPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { scope, setScope } = useCompanyScope()
  const scopeParams = companyScopeToQueryParams(scope)

  const [idInput, setIdInput] = useState('')
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting] = useState<SortingState>([])

  // One combined field: the backend's idSearch param already ORs Company.Id and
  // PartnerAccount.AccountNumber, so a single input is all the UI needs.
  const idSearch = idInput.trim() || undefined

  const contracts = useContracts({
    mode: (scopeParams.mode as ContractsMode | undefined) ?? 'single',
    companyId: scopeParams.companyId ? Number(scopeParams.companyId) : undefined,
    locationCategoryId: scopeParams.locationCategoryId ? Number(scopeParams.locationCategoryId) : undefined,
    idSearch,
    search: search.trim() || undefined,
    isActive,
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
  })

  const columns = useMemo<ColumnDef<ContractOverviewRow>[]>(
    () => [
      {
        id: 'companyId',
        header: t('contracts_.colCompanyId'),
        accessorKey: 'companyId',
        cell: ({ row }) => (
          <ClickableCell
            onClick={() =>
              navigate(
                `/kartice?companyId=${row.original.companyId}&accountNumber=${row.original.partnerAccountNumber ?? ''}`,
              )
            }
          >
            {row.original.companyId}
          </ClickableCell>
        ),
      },
      {
        id: 'partnerAccountNumber',
        header: t('contracts_.colPartnerAccountNumber'),
        accessorKey: 'partnerAccountNumber',
        cell: ({ row }) =>
          row.original.partnerAccountId != null ? (
            <ClickableCell onClick={() => navigate(`/kartice?partnerAccountId=${row.original.partnerAccountId}`)}>
              {row.original.partnerAccountNumber}
            </ClickableCell>
          ) : (
            '—'
          ),
      },
      {
        id: 'companyShortName',
        header: t('contracts_.colCompanyShortName'),
        accessorKey: 'companyShortName',
        cell: ({ row }) => (
          <ClickableCell
            onClick={() => {
              setScope({ mode: 'single', companyId: row.original.companyId })
              navigate('/companies')
            }}
          >
            {row.original.companyShortName}
          </ClickableCell>
        ),
      },
      {
        id: 'buildingEntranceName',
        header: t('contracts_.colBuildingEntrance'),
        accessorKey: 'buildingEntranceName',
        cell: ({ row }) =>
          row.original.buildingEntranceId != null ? (
            <ClickableCell
              onClick={() => navigate(`/building-entrances/${row.original.companyId}/${row.original.buildingEntranceId}`)}
            >
              {row.original.buildingEntranceName ?? `#${row.original.buildingEntranceId}`}
            </ClickableCell>
          ) : (
            row.original.buildingEntranceName ?? '—'
          ),
      },
      {
        id: 'partnerName',
        header: t('contracts_.colPartner'),
        accessorKey: 'partnerName',
        cell: ({ row }) =>
          row.original.partnerId != null ? (
            <ClickableCell onClick={() => navigate(`/partners/${row.original.companyId}/${row.original.partnerId}`)}>
              {row.original.partnerName ?? `#${row.original.partnerId}`}
            </ClickableCell>
          ) : (
            row.original.partnerName ?? '—'
          ),
      },
      {
        id: 'unitName',
        header: t('contracts_.colUnit'),
        accessorKey: 'unitName',
        cell: ({ row }) => (
          <ClickableCell onClick={() => navigate(`/units/${row.original.companyId}/${row.original.unitId}`)}>
            {row.original.unitName ?? `#${row.original.unitId}`}
          </ClickableCell>
        ),
      },
      {
        id: 'unitTypeName',
        header: t('contracts_.colUnitType'),
        accessorKey: 'unitTypeName',
        enableSorting: false,
        cell: ({ row }) => row.original.unitTypeName ?? '—',
      },
    ],
    [navigate, setScope, t],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">
        {t('contracts_.title')}
      </Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField
          label={t('masterDataDetail_.idSearchLabel')}
          value={idInput}
          onChange={(event) => {
            setIdInput(event.target.value)
            setPagination((value) => ({ ...value, pageIndex: 0 }))
          }}
          size="small"
        />
        <TextField
          label={t('contracts_.searchLabel')}
          value={search}
          onChange={(event) => {
            setSearch(event.target.value)
            setPagination((value) => ({ ...value, pageIndex: 0 }))
          }}
          size="small"
          fullWidth
        />
        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel id="contracts-status-label">{t('contracts_.statusLabel')}</InputLabel>
          <Select
            labelId="contracts-status-label"
            label={t('contracts_.statusLabel')}
            value={isActive ? 'active' : 'inactive'}
            onChange={(event: SelectChangeEvent) => {
              setIsActive(event.target.value === 'active')
              setPagination((value) => ({ ...value, pageIndex: 0 }))
            }}
          >
            <MenuItem value="active">{t('contracts_.statusActive')}</MenuItem>
            <MenuItem value="inactive">{t('contracts_.statusInactive')}</MenuItem>
          </Select>
        </FormControl>
      </Stack>
      {contracts.isError && <Alert severity="error">{t('contracts_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('contracts_.title')}
        rows={contracts.data?.items ?? []}
        columns={columns}
        rowCount={contracts.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={() => {}}
        isLoading={contracts.isLoading}
        emptyMessage={t('contracts_.empty')}
        getRowId={(row) => String(row.contractId)}
        minWidth={960}
      />
    </Stack>
  )
}

function ClickableCell({ onClick, children }: { onClick: () => void; children: ReactNode }) {
  return (
    <Link component="button" type="button" underline="hover" textAlign="left" onClick={onClick}>
      {children}
    </Link>
  )
}
