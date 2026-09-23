import { useMemo, useState } from 'react'
import { Alert, Box, Button, Chip, Stack, Tab, Tabs, Typography } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { ApiProblemError } from '../../api/generated/client'
import { useActiveCompany } from '../companies/useActiveCompany'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { ledgerBankingApi } from './ledgerBankingApi'
import { canPostJournal, canPostStatement, formatMoney } from './ledgerBankingFormat'
import type { BankStatementSummary, JournalEntrySummary } from './types'

export function LedgerBankingPage({ canPost }: { canPost: boolean }) {
  const { t } = useTranslation()
  const [tab, setTab] = useState(0)
  const { activeCompany } = useActiveCompany()

  return (
    <Stack spacing={2}>
      <Typography component="h1" variant="h1">{t('ledgerBanking.title')}</Typography>
      <Tabs value={tab} onChange={(_, value: number) => setTab(value)} aria-label={t('ledgerBanking.title')}>
        <Tab label={t('ledgerBanking.tabJournals')} />
        <Tab label={t('ledgerBanking.tabStatements')} />
      </Tabs>
      {tab === 0 ? (
        <JournalPanel companyId={activeCompany.id} canPost={canPost} />
      ) : (
        <StatementPanel companyId={activeCompany.id} canPost={canPost} />
      )}
    </Stack>
  )
}

function JournalPanel({ companyId, canPost }: { companyId: number; canPost: boolean }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
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
      { accessorKey: 'description', header: t('ledgerBanking.journalColumns.description') },
      { accessorKey: 'balance', header: t('ledgerBanking.journalColumns.turnover'), cell: ({ row }) => formatMoney(row.original.balance, row.original.currency) },
      {
        accessorKey: 'isPosted',
        header: t('ledgerBanking.journalColumns.status'),
        cell: ({ row }) => (
          <Chip
            size="small"
            label={row.original.isPosted ? t('ledgerBanking.statusPosted') : t('ledgerBanking.statusDraft')}
            color={row.original.isPosted ? 'success' : 'default'}
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
              disabled={mutation.isPending}
              onClick={() => mutation.mutate({ journal: row.original, reverse: row.original.isPosted })}
            >
              {canPostJournal(row.original) ? t('ledgerBanking.post') : t('ledgerBanking.reverse')}
            </Button>
          ) : null,
      },
    ],
    [mutation, canPost, t],
  )

  return (
    <Box>
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
        getRowId={(row) => String(row.id)}
      />
    </Box>
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
      { accessorKey: 'status', header: t('ledgerBanking.statementColumns.status') },
      {
        id: 'actions',
        header: t('ledgerBanking.statementColumns.action'),
        cell: ({ row }) =>
          canPost ? (
            <Button
              size="small"
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
    <Box>
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
        getRowId={(row) => String(row.id)}
      />
    </Box>
  )
}

function MutationError({ error }: { error: Error | null }) {
  if (!error) return null
  const message = error instanceof ApiProblemError ? error.problem.detail ?? error.problem.title : error.message
  return <Alert severity="error" sx={{ mb: 2 }}>{message}</Alert>
}
