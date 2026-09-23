import { useMemo, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router-dom'
import { ApiProblemError } from '../../api/generated/client'
import { useActiveCompany } from '../companies/useActiveCompany'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { ledgerBankingApi } from './ledgerBankingApi'
import { canPostJournal, canPostStatement, formatMoney } from './ledgerBankingFormat'
import type { BankStatementSummary, JournalEntrySummary } from './types'

const statementStatusColor: Record<BankStatementSummary['status'], 'default' | 'warning' | 'info' | 'success'> = {
  Imported: 'default',
  PartiallyMatched: 'warning',
  Ready: 'info',
  Posted: 'success',
}

export function LedgerBankingPage({ canPost }: { canPost: boolean }) {
  const { t } = useTranslation()
  const { pathname } = useLocation()
  // Both /ledger and /banking render this page (same element, so no remount on
  // navigation): default to the tab the sidebar link points at, until the user picks one.
  const [picked, setPicked] = useState<{ path: string; value: number } | null>(null)
  const tab = picked?.path === pathname ? picked.value : pathname.startsWith('/banking') ? 1 : 0
  const setTab = (value: number) => setPicked({ path: pathname, value })
  const { activeCompany } = useActiveCompany()

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('ledgerBanking.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('ledgerBanking.subtitle')}</Typography>
      </Box>
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tab} onChange={(_, value: number) => setTab(value)} aria-label={t('ledgerBanking.title')}>
          <Tab label={t('ledgerBanking.tabJournals')} id="ledger-tab-0" aria-controls="ledger-panel-0" />
          <Tab label={t('ledgerBanking.tabStatements')} id="ledger-tab-1" aria-controls="ledger-panel-1" />
        </Tabs>
      </Box>
      {tab === 0 ? (
        <Box role="tabpanel" id="ledger-panel-0" aria-labelledby="ledger-tab-0">
          <JournalPanel companyId={activeCompany.id} canPost={canPost} />
        </Box>
      ) : (
        <Box role="tabpanel" id="ledger-panel-1" aria-labelledby="ledger-tab-1">
          <StatementPanel companyId={activeCompany.id} canPost={canPost} />
        </Box>
      )}
    </Stack>
  )
}

function JournalPanel({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [reverseTarget, setReverseTarget] = useState<JournalEntrySummary | null>(null)
  const queryKey = ['ledger-journals', companyId, pagination]
  const journals = useQuery({
    queryKey,
    queryFn: () => ledgerBankingApi.journals.list(companyId, pagination.pageIndex, pagination.pageSize),
  })
  const mutation = useMutation({
    mutationFn: ({ journal, reverse }: { journal: JournalEntrySummary; reverse: boolean }) =>
      reverse
        ? ledgerBankingApi.journals.reverse(companyId, journal)
        : ledgerBankingApi.journals.post(companyId, journal),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ledger-journals', companyId] }),
  })
  const columns = useMemo<ColumnDef<JournalEntrySummary>[]>(
    () => [
      { accessorKey: 'id', header: t('ledgerBanking.journalColumns.number') },
      { accessorKey: 'postingDate', header: t('ledgerBanking.journalColumns.date') },
      {
        accessorKey: 'description',
        header: t('ledgerBanking.journalColumns.description'),
        cell: ({ row }) => (
          <>
            {row.original.description}
            {row.original.reversalOfId ? (
              <Typography variant="caption" color="text.secondary" display="block">
                {t('ledgerBanking.reversedOf', { number: row.original.reversalOfId })}
              </Typography>
            ) : null}
          </>
        ),
      },
      {
        accessorKey: 'balance',
        header: t('ledgerBanking.journalColumns.turnover'),
        cell: ({ row }) => formatMoney(row.original.balance, row.original.currency),
      },
      {
        accessorKey: 'isPosted',
        header: t('ledgerBanking.journalColumns.status'),
        cell: ({ row }) => (
          <Chip
            size="small"
            label={row.original.isPosted ? t('ledgerBanking.statusPosted') : t('ledgerBanking.statusDraft')}
            color={row.original.isPosted ? 'success' : 'default'}
            variant={row.original.isPosted ? 'filled' : 'outlined'}
          />
        ),
      },
      {
        id: 'actions',
        header: t('ledgerBanking.statementColumns.action'),
        cell: ({ row }) => {
          if (!canPost) return null
          return canPostJournal(row.original) ? (
            <Button
              size="small"
              variant="contained"
              disabled={mutation.isPending}
              onClick={() => mutation.mutate({ journal: row.original, reverse: false })}
            >
              {t('ledgerBanking.post')}
            </Button>
          ) : (
            <Button
              size="small"
              variant="outlined"
              color="warning"
              disabled={mutation.isPending}
              onClick={() => setReverseTarget(row.original)}
            >
              {t('ledgerBanking.reverse')}
            </Button>
          )
        },
      },
    ],
    [mutation, canPost, t],
  )

  return (
    <Stack spacing={2}>
      <Typography variant="body2" color="text.secondary">{t('ledgerBanking.journalsHint')}</Typography>
      {journals.isError ? <Alert severity="error">{t('ledgerBanking.loadFailed')}</Alert> : null}
      <MutationError error={mutation.error} />
      <ServerDataTable
        ariaLabel={t('ledgerBanking.journalTableLabel')}
        rows={journals.data?.items ?? []}
        columns={columns}
        rowCount={journals.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={journals.isLoading}
        emptyMessage={`${t('ledgerBanking.journalsEmptyTitle')} — ${t('ledgerBanking.journalsEmptyBody')}`}
        getRowId={(row) => String(row.id)}
      />
      <Dialog open={reverseTarget !== null} onClose={() => setReverseTarget(null)}>
        <DialogTitle>{t('ledgerBanking.reverseConfirmTitle')}</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {t('ledgerBanking.reverseConfirmBody', { number: reverseTarget?.id })}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReverseTarget(null)}>{t('common.cancel')}</Button>
          <Button
            color="warning"
            variant="contained"
            disabled={mutation.isPending}
            onClick={() => {
              if (reverseTarget) mutation.mutate({ journal: reverseTarget, reverse: true })
              setReverseTarget(null)
            }}
          >
            {t('ledgerBanking.reverseConfirm')}
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  )
}

function StatementPanel({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const statements = useQuery({
    queryKey: ['bank-statements', companyId, pagination],
    queryFn: () => ledgerBankingApi.statements.list(companyId, pagination.pageIndex, pagination.pageSize),
  })
  const post = useMutation({
    mutationFn: (statement: BankStatementSummary) => ledgerBankingApi.statements.post(companyId, statement),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bank-statements', companyId] }),
  })
  const columns = useMemo<ColumnDef<BankStatementSummary>[]>(
    () => [
      { accessorKey: 'statementNumber', header: t('ledgerBanking.statementColumns.number') },
      { accessorKey: 'date', header: t('ledgerBanking.statementColumns.date') },
      { accessorKey: 'previousBalance', header: t('ledgerBanking.statementColumns.previousBalance'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'debit', header: t('ledgerBanking.statementColumns.debit'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'credit', header: t('ledgerBanking.statementColumns.credit'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'newBalance', header: t('ledgerBanking.statementColumns.newBalance'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      {
        accessorKey: 'status',
        header: t('ledgerBanking.statementColumns.status'),
        cell: ({ row }) => (
          <Chip
            size="small"
            color={statementStatusColor[row.original.status] ?? 'default'}
            variant={row.original.status === 'Posted' ? 'filled' : 'outlined'}
            label={t(`ledgerBanking.statementStatus.${row.original.status}`, { defaultValue: row.original.status })}
          />
        ),
      },
      {
        id: 'actions',
        header: t('ledgerBanking.statementColumns.action'),
        cell: ({ row }) =>
          canPost ? (
            <Button
              size="small"
              variant="contained"
              disabled={!canPostStatement(row.original) || post.isPending}
              onClick={() => post.mutate(row.original)}
            >
              {t('ledgerBanking.post')}
            </Button>
          ) : null,
      },
    ],
    [post, canPost, t],
  )

  return (
    <Stack spacing={2}>
      <Typography variant="body2" color="text.secondary">{t('ledgerBanking.statementsHint')}</Typography>
      {statements.isError ? <Alert severity="error">{t('ledgerBanking.loadFailed')}</Alert> : null}
      <MutationError error={post.error} />
      <ServerDataTable
        ariaLabel={t('ledgerBanking.statementTableLabel')}
        rows={statements.data?.items ?? []}
        columns={columns}
        rowCount={statements.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={statements.isLoading}
        emptyMessage={`${t('ledgerBanking.statementsEmptyTitle')} — ${t('ledgerBanking.statementsEmptyBody')}`}
        getRowId={(row) => String(row.id)}
      />
    </Stack>
  )
}

function MutationError({ error }: { error: Error | null }) {
  if (!error) return null
  const message = error instanceof ApiProblemError ? error.problem.detail ?? error.problem.title : error.message
  return <Alert severity="error">{message}</Alert>
}
