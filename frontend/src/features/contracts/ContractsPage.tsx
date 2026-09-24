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
import { companyScopeToQueryParams, useCompanyScope } from './companyScopeAdapter'
import { useContracts } from './useContracts'
import type { ContractOverviewRow, ContractsMode } from './types'

export function ContractsPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { scope } = useCompanyScope()
  const scopeParams = companyScopeToQueryParams(scope)

  const [companyIdInput, setCompanyIdInput] = useState('')
  const [partnerAccountInput, setPartnerAccountInput] = useState('')
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting] = useState<SortingState>([])

  // Boss's spec has two dedicated id boxes (Company Id / PartnerAccount broj) but
  // the backend has a single idSearch param that ORs across both columns - whichever
  // box has text wins; Company Id takes priority if both are filled.
  const idSearch = companyIdInput.trim() || partnerAccountInput.trim() || undefined

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
          <ClickableCell onClick={() => navigate('/companies')}>{row.original.companyShortName}</ClickableCell>
        ),
      },
      {
        id: 'buildingEntranceName',
        header: t('contracts_.colBuildingEntrance'),
        accessorKey: 'buildingEntranceName',
        // No detail page exists for a single building entrance today - plain text,
        // not a link (see report for the drill-through gap).
        cell: ({ row }) => row.original.buildingEntranceName ?? '—',
      },
      {
        id: 'partnerName',
        header: t('contracts_.colPartner'),
        accessorKey: 'partnerName',
        cell: ({ row }) =>
          row.original.partnerName ? (
            <ClickableCell onClick={() => navigate('/partners')}>{row.original.partnerName}</ClickableCell>
          ) : (
            '—'
          ),
      },
      {
        id: 'unitName',
        header: t('contracts_.colUnit'),
        accessorKey: 'unitName',
        cell: ({ row }) => (
          <ClickableCell onClick={() => navigate('/units')}>{row.original.unitName ?? `#${row.original.unitId}`}</ClickableCell>
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
    [navigate, t],
  )

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">
        {t('contracts_.title')}
      </Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
        <TextField
          label={t('contracts_.companyIdSearchLabel')}
          value={companyIdInput}
          onChange={(event) => {
            setCompanyIdInput(event.target.value)
            setPagination((value) => ({ ...value, pageIndex: 0 }))
          }}
          size="small"
        />
        <TextField
          label={t('contracts_.partnerAccountSearchLabel')}
          value={partnerAccountInput}
          onChange={(event) => {
            setPartnerAccountInput(event.target.value)
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
