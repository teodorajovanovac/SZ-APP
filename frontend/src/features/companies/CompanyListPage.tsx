import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import { Alert, Button, Link, Stack, TextField, Typography } from '@mui/material'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { api, type CompanySummary } from '../../api/generated/client'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { useAuth } from '../auth/useAuth'
import { CompanyForm } from '../master-data/components/CompanyForm'
import { CompanyCreateForm } from './CompanyCreateForm'

const listKey = ['companies', 'list'] as const

// /api/v1/companies is already scoped server-side (Root = all, others = StaffAccess companies)
// and small (one row per SZ), so search/sort/paging happen client-side.
export function CompanyListPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const isRoot = user?.roles.includes('Root') ?? false
  const companies = useQuery({ queryKey: listKey, queryFn: api.companies.list })
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([{ id: 'name', desc: false }])
  const [editing, setEditing] = useState<CompanySummary | 'new'>()

  const filtered = useMemo(() => {
    const needle = search.trim().toLowerCase()
    const rows = (companies.data ?? []).filter(
      (row) => !needle || String(row.id) === needle || row.name.toLowerCase().includes(needle) || (row.locationName ?? '').toLowerCase().includes(needle),
    )
    const sort = sorting[0]
    if (sort) {
      const key = sort.id as keyof CompanySummary
      rows.sort((a, b) => {
        const result = typeof a[key] === 'number' ? Number(a[key]) - Number(b[key]) : String(a[key] ?? '').localeCompare(String(b[key] ?? ''), 'sr')
        return sort.desc ? -result : result
      })
    }
    return rows
  }, [companies.data, search, sorting])
  const page = filtered.slice(pagination.pageIndex * pagination.pageSize, (pagination.pageIndex + 1) * pagination.pageSize)

  const columns = useMemo<ColumnDef<CompanySummary>[]>(
    () => [
      { accessorKey: 'id', header: t('companiesAdmin_.id'), meta: { numeric: true } },
      {
        accessorKey: 'name',
        header: t('companiesAdmin_.name'),
        cell: ({ row }) => (
          <Link component="button" type="button" underline="hover" textAlign="left" onClick={() => setEditing(row.original)}>
            {row.original.name}
          </Link>
        ),
      },
      { accessorKey: 'locationName', header: t('companiesAdmin_.location'), cell: ({ getValue }) => (getValue() as string | null) ?? '—' },
      { accessorKey: 'registrationNumber', header: t('companiesAdmin_.registrationNumber'), enableSorting: false, cell: ({ getValue }) => (getValue() as string | null) ?? '—' },
    ],
    [t],
  )

  const close = () => {
    setEditing(undefined)
    void queryClient.invalidateQueries({ queryKey: listKey })
  }

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('companiesAdmin_.title')}</Typography>
        {isRoot && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
            {t('companiesAdmin_.new')}
          </Button>
        )}
      </Stack>
      <TextField
        label={t('companiesAdmin_.search')}
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
          setPagination((value) => ({ ...value, pageIndex: 0 }))
        }}
        size="small"
      />
      {companies.isError && <Alert severity="error">{t('companiesAdmin_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('companiesAdmin_.title')}
        rows={page}
        columns={columns}
        rowCount={filtered.length}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={companies.isLoading}
        getRowId={(row) => String(row.id)}
      />
      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('companiesAdmin_.new') : t('companiesAdmin_.editTitle')}
        onClose={close}
        maxWidth="md"
      >
        {editing === 'new' ? <CompanyCreateForm onSaved={close} /> : editing ? <CompanyForm companyId={editing.id} /> : null}
      </FormDialog>
    </Stack>
  )
}
