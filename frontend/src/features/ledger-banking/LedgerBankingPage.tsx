import { useMemo, useState } from 'react'
import { Alert, Box, Button, Chip, Stack, Tab, Tabs, Typography } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { ApiProblemError } from '../../api/generated/client'
import { useActiveCompany } from '../companies/useActiveCompany'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { ledgerBankingApi } from './ledgerBankingApi'
import { canPostJournal, canPostStatement, formatMoney } from './ledgerBankingFormat'
import type { BankStatementSummary, JournalEntrySummary } from './types'

export function LedgerBankingPage({ canPost }: { canPost: boolean }) {
  const [tab, setTab] = useState(0)
  const { activeCompany } = useActiveCompany()

  return (
    <Stack spacing={2}>
      <Typography variant="h4">Glavna knjiga i izvodi</Typography>
      <Tabs value={tab} onChange={(_, value: number) => setTab(value)} aria-label="Glavna knjiga i izvodi">
        <Tab label="Nalozi" />
        <Tab label="Bankarski izvodi" />
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
      { accessorKey: 'id', header: 'Broj' },
      { accessorKey: 'postingDate', header: 'Datum' },
      { accessorKey: 'description', header: 'Opis' },
      { accessorKey: 'balance', header: 'Promet', cell: ({ row }) => formatMoney(row.original.balance, row.original.currency) },
      {
        accessorKey: 'isPosted',
        header: 'Status',
        cell: ({ row }) => <Chip size="small" label={row.original.isPosted ? 'Knjižen' : 'Nacrt'} color={row.original.isPosted ? 'success' : 'default'} />,
      },
      {
        id: 'actions',
        header: 'Akcije',
        cell: ({ row }) =>
          canPost ? (
            <Button
              size="small"
              disabled={mutation.isPending}
              onClick={() => mutation.mutate({ journal: row.original, reverse: row.original.isPosted })}
            >
              {canPostJournal(row.original) ? 'Knjiži' : 'Storniraj'}
            </Button>
          ) : null,
      },
    ],
    [mutation, canPost],
  )

  return (
    <Box>
      <MutationError error={mutation.error} />
      <ServerDataTable
        ariaLabel="Nalozi glavne knjige"
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
      { accessorKey: 'statementNumber', header: 'Broj' },
      { accessorKey: 'date', header: 'Datum' },
      { accessorKey: 'previousBalance', header: 'Prethodno stanje', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'debit', header: 'Duguje', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'credit', header: 'Potražuje', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'newBalance', header: 'Novo stanje', cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'status', header: 'Status' },
      {
        id: 'actions',
        header: 'Akcije',
        cell: ({ row }) =>
          canPost ? (
            <Button
              size="small"
              disabled={!canPostStatement(row.original) || post.isPending}
              onClick={() => post.mutate(row.original)}
            >
              Knjiži
            </Button>
          ) : null,
      },
    ],
    [post, canPost],
  )

  return (
    <Box>
      <MutationError error={post.error} />
      <ServerDataTable
        ariaLabel="Bankarski izvodi"
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
