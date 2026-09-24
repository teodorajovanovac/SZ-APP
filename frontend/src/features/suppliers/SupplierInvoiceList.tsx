import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import {
  Alert,
  Autocomplete,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { formatMoney } from '../../shared/format/money'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { usePartnerAccounts } from '../master-data/useMasterData'
import { useShortList } from '../master-data/useShortList'
import { SupplierInvoiceForm } from './SupplierInvoiceForm'
import { useSupplierInvoices, type SupplierInvoice } from './supplierApi'

const MARKER_FILTER_ALL = 'all'
const MARKER_FILTER_WITH = 'with'
const MARKER_FILTER_WITHOUT = 'without'
type MarkerFilterValue = typeof MARKER_FILTER_ALL | typeof MARKER_FILTER_WITH | typeof MARKER_FILTER_WITHOUT

function formatPeriodLabel(periodYYMM: number, locale: string) {
  const month = periodYYMM % 100
  const year = 2000 + Math.floor(periodYYMM / 100)
  return new Date(year, month - 1, 1).toLocaleDateString(locale, { month: 'long', year: 'numeric' })
}

export function SupplierInvoiceList({ companyId }: { companyId: number }) {
  const { t, i18n } = useTranslation()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [creating, setCreating] = useState(false)

  const [periodYYMM, setPeriodYYMM] = useState<number | undefined>(undefined)
  const [supplierPartnerAccountId, setSupplierPartnerAccountId] = useState<number | undefined>(undefined)
  const [markerFilter, setMarkerFilter] = useState<MarkerFilterValue>(MARKER_FILTER_ALL)
  const [documentTypeId, setDocumentTypeId] = useState<number | undefined>(undefined)
  const hasExtraordinaryMarker =
    markerFilter === MARKER_FILTER_ALL ? undefined : markerFilter === MARKER_FILTER_WITH

  const resetToFirstPage = () => setPagination((value) => ({ ...value, pageIndex: 0 }))

  const query = useSupplierInvoices(companyId, pagination.pageIndex + 1, pagination.pageSize, {
    periodYYMM,
    supplierPartnerAccountId,
    hasExtraordinaryMarker,
    documentTypeId,
  })
  // ponytail: distinct periods come from an unfiltered 200-row page rather than a
  // dedicated /supplier-invoices/periods endpoint — cheapest correct option while a
  // company's supplier invoice count stays well under 200. Add a real distinct-periods
  // endpoint if that stops holding.
  const periodsQuery = useSupplierInvoices(companyId, 1, 200)
  const partnerAccounts = usePartnerAccounts(companyId, { page: 1, pageSize: 200 })
  const documentTypes = useShortList(companyId, 'SupplierDocumentType')

  const periodOptions = useMemo(() => {
    const values = new Set((periodsQuery.data?.items ?? []).map((item) => item.periodYYMM))
    return Array.from(values).sort((a, b) => b - a)
  }, [periodsQuery.data])

  const partnerAccountById = useMemo(() => {
    const map = new Map(partnerAccounts.data?.items.map((account) => [account.id, account.account]))
    return map
  }, [partnerAccounts.data])

  const documentTypeById = useMemo(() => {
    const map = new Map(documentTypes.data?.map((item) => [item.id, item.caption]))
    return map
  }, [documentTypes.data])

  const columns = useMemo<ColumnDef<SupplierInvoice>[]>(
    () => [
      { accessorKey: 'invoiceNo', header: t('suppliers_.columns.number'), enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'periodYYMM', header: t('suppliers_.columns.period'), enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'caption', header: t('suppliers_.columns.caption'), enableSorting: false, meta: { ellipsis: true } },
      {
        id: 'supplierPartnerAccountId',
        header: t('suppliersFilters.colSupplier'),
        enableSorting: false,
        cell: ({ row }) => partnerAccountById.get(row.original.supplierPartnerAccountId) ?? row.original.supplierPartnerAccountId,
      },
      {
        id: 'documentTypeId',
        header: t('suppliersFilters.colDocumentType'),
        enableSorting: false,
        cell: ({ row }) => documentTypeById.get(row.original.documentTypeId) ?? row.original.documentTypeId,
      },
      {
        id: 'extraordinaryInvoiceMarker',
        header: t('suppliersFilters.colMarker'),
        enableSorting: false,
        cell: ({ row }) => row.original.extraordinaryInvoiceMarker || '—',
      },
      // amountRsd shown at 2 decimals like every other money value (no sub-cent precision requirement found).
      { accessorKey: 'amountRsd', header: t('suppliers_.columns.rsd'), enableSorting: false, meta: { numeric: true }, cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'postedAmount', header: t('suppliers_.columns.posted'), enableSorting: false, meta: { numeric: true }, cell: ({ getValue }) => formatMoney(getValue<number>()) },
    ],
    [t, partnerAccountById, documentTypeById],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('suppliers_.title')}</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreating(true)}>
          {t('suppliersUi.new')}
        </Button>
      </Stack>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} flexWrap="wrap" useFlexGap>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel id="supplier-invoice-period-label">{t('suppliersFilters.periodLabel')}</InputLabel>
          <Select
            labelId="supplier-invoice-period-label"
            label={t('suppliersFilters.periodLabel')}
            value={periodYYMM != null ? String(periodYYMM) : ''}
            onChange={(event: SelectChangeEvent) => {
              setPeriodYYMM(event.target.value ? Number(event.target.value) : undefined)
              resetToFirstPage()
            }}
          >
            <MenuItem value="">{t('suppliersFilters.periodAll')}</MenuItem>
            {periodOptions.map((value) => (
              <MenuItem key={value} value={String(value)}>{formatPeriodLabel(value, i18n.language)}</MenuItem>
            ))}
          </Select>
        </FormControl>

        <Autocomplete
          sx={{ minWidth: 220 }}
          options={partnerAccounts.data?.items ?? []}
          loading={partnerAccounts.isLoading}
          size="small"
          getOptionLabel={(option) => option.account}
          isOptionEqualToValue={(option, value) => option.id === value.id}
          value={partnerAccounts.data?.items.find((account) => account.id === supplierPartnerAccountId) ?? null}
          onChange={(_, option) => {
            setSupplierPartnerAccountId(option?.id)
            resetToFirstPage()
          }}
          renderInput={(params) => <TextField {...params} label={t('suppliersFilters.supplierLabel')} />}
        />

        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel id="supplier-invoice-marker-label">{t('suppliersFilters.markerLabel')}</InputLabel>
          <Select
            labelId="supplier-invoice-marker-label"
            label={t('suppliersFilters.markerLabel')}
            value={markerFilter}
            onChange={(event: SelectChangeEvent) => {
              setMarkerFilter(event.target.value as MarkerFilterValue)
              resetToFirstPage()
            }}
          >
            <MenuItem value={MARKER_FILTER_ALL}>{t('suppliersFilters.markerAll')}</MenuItem>
            <MenuItem value={MARKER_FILTER_WITH}>{t('suppliersFilters.markerWith')}</MenuItem>
            <MenuItem value={MARKER_FILTER_WITHOUT}>{t('suppliersFilters.markerWithout')}</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel id="supplier-invoice-document-type-label">{t('suppliersFilters.documentTypeLabel')}</InputLabel>
          <Select
            labelId="supplier-invoice-document-type-label"
            label={t('suppliersFilters.documentTypeLabel')}
            value={documentTypeId != null ? String(documentTypeId) : ''}
            onChange={(event: SelectChangeEvent) => {
              setDocumentTypeId(event.target.value ? Number(event.target.value) : undefined)
              resetToFirstPage()
            }}
          >
            <MenuItem value="">{t('suppliersFilters.documentTypeAll')}</MenuItem>
            {(documentTypes.data ?? []).map((item) => (
              <MenuItem key={item.id} value={String(item.id)}>{item.caption}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Stack>

      {query.error ? <Alert severity="error">{getErrorMessage(query.error, t('suppliers_.notLoaded'))}</Alert> : null}
      <ServerDataTable
        ariaLabel={t('suppliers_.title')}
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={query.isLoading}
        emptyMessage={t('suppliersUi.empty')}
        getRowId={(row) => String(row.id)}
        minWidth={960}
      />

      <FormDialog open={creating} title={t('suppliersUi.newTitle')} onClose={() => setCreating(false)} maxWidth="md">
        {creating ? <SupplierInvoiceForm companyId={companyId} onSaved={() => setCreating(false)} /> : null}
      </FormDialog>
    </Stack>
  )
}
