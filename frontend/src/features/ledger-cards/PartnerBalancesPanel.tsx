import { Alert, FormControlLabel, Link, Stack, Switch, TextField } from '@mui/material'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import type { ColumnDef, SortingState } from '@tanstack/react-table'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { formatDate } from '../../shared/format/date'
import { formatAmount } from '../../shared/format/money'
import { AccountSelect } from './cardFilters'
import { ledgerCardsApi, type PartnerBalance } from './ledgerCardsApi'
import { useUrlFilters } from './useUrlFilters'

const SORTABLE = new Set(['balance', 'name', 'accountNumber'])

/** GAP-08 "Stanja partnera": per partner-account balance on one account as of a date. */
export function PartnerBalancesPanel() {
  const { t } = useTranslation()
  const { companyId, get, set } = useUrlFilters()
  const account = get('account') || '2040'
  const asOf = get('asOf')
  const onlyDebtors = get('onlyDebtors') !== '0'
  const minDebt = get('minDebt')
  const sort = SORTABLE.has(get('sort')) ? get('sort') : 'balance'
  const desc = get('desc') !== '0'
  const page = Math.max(Number(get('page')) || 1, 1)
  const pageSize = Number(get('pageSize')) || 25

  const balances = useQuery({
    queryKey: ['partner-balances', companyId, account, asOf, onlyDebtors, minDebt, sort, desc, page, pageSize],
    queryFn: () => ledgerCardsApi.balances(companyId, { account, asOf, onlyDebtors, minDebt, sort, desc, page, pageSize }),
    placeholderData: keepPreviousData,
  })

  const openCard = (row: PartnerBalance) =>
    set({ tab: null, account, partnerAccountId: row.partnerAccountId, partner: `${row.partnerName} (${row.account} / ${row.accountNumber})`, partnerId: null, to: asOf || null, groupBy: null, sort: null, desc: null, pageSize: null })

  const columns = useMemo<ColumnDef<PartnerBalance>[]>(
    () => [
      { id: 'accountNumber', header: t('balances_.col.accountNumber'), accessorKey: 'accountNumber', meta: { numeric: true } },
      {
        id: 'name',
        accessorKey: 'partnerName',
        header: t('balances_.col.partner'),
        cell: ({ row }) => (
          <Link component="button" type="button" onClick={() => openCard(row.original)} sx={{ textAlign: 'left' }}>
            {row.original.partnerName}
          </Link>
        ),
      },
      { id: 'debit', header: t('cards_.col.debit'), enableSorting: false, meta: { numeric: true }, cell: ({ row }) => formatAmount(row.original.debit) },
      { id: 'credit', header: t('cards_.col.credit'), enableSorting: false, meta: { numeric: true }, cell: ({ row }) => formatAmount(row.original.credit) },
      { id: 'balance', accessorKey: 'balance', header: t('cards_.col.balance'), meta: { numeric: true }, cell: ({ row }) => formatAmount(row.original.balance) },
      { id: 'overdue', header: t('balances_.col.overdue'), enableSorting: false, meta: { numeric: true }, cell: ({ row }) => formatAmount(row.original.overdue) },
      { id: 'lastPayment', header: t('balances_.col.lastPayment'), enableSorting: false, cell: ({ row }) => formatDate(row.original.lastPaymentDate) },
    ],
    // openCard only closes over URL state; re-creating columns per render is not needed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [t, account, asOf],
  )

  const sorting: SortingState = [{ id: sort, desc }]
  const onSortingChange = (next: SortingState) => {
    const first = next[0]
    set({ sort: first?.id ?? null, desc: first ? (first.desc ? null : '0') : null })
  }

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap alignItems="center">
        <AccountSelect companyId={companyId} value={account} onChange={(next) => set({ account: next || null })} />
        <TextField size="small" type="date" label={t('balances_.asOf')} value={asOf} onChange={(e) => set({ asOf: e.target.value })} slotProps={{ inputLabel: { shrink: true } }} />
        <FormControlLabel
          control={<Switch checked={onlyDebtors} onChange={(e) => set({ onlyDebtors: e.target.checked ? null : '0' })} />}
          label={t('balances_.onlyDebtors')}
        />
        <TextField size="small" type="number" label={t('balances_.minDebt')} value={minDebt} onChange={(e) => set({ minDebt: e.target.value })} sx={{ width: 150 }} />
      </Stack>
      <Alert severity="info">{t('balances_.overdueHint')}</Alert>
      {balances.isError ? <Alert severity="error">{t('cards_.loadFailed')}</Alert> : null}
      <ServerDataTable
        ariaLabel={t('balances_.title')}
        rows={balances.data?.items ?? []}
        columns={columns}
        rowCount={balances.data?.totalCount ?? 0}
        pagination={{ pageIndex: page - 1, pageSize }}
        onPaginationChange={(next) => set({ page: next.pageIndex + 1, pageSize: next.pageSize })}
        sorting={sorting}
        onSortingChange={onSortingChange}
        getRowId={(row) => String(row.partnerAccountId)}
        isLoading={balances.isLoading}
        emptyMessage={t('balances_.empty')}
      />
    </Stack>
  )
}
