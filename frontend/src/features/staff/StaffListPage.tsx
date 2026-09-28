import AddIcon from '@mui/icons-material/Add'
import { Alert, Button, Chip, Link, Stack, TextField, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import { FormDialog } from '../../shared/components/FormDialog'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { formatDate } from '../../shared/format/date'
import { StaffCreateForm } from './StaffCreateForm'
import { useStaffList, type StaffListItem } from './staffApi'

export function StaffListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([])
  const [creating, setCreating] = useState(false)
  const result = useStaffList(pagination.pageIndex + 1, pagination.pageSize, search, sorting[0]?.desc)

  const columns = useMemo<ColumnDef<StaffListItem>[]>(
    () => [
      {
        accessorKey: 'email',
        header: t('staffAdmin_.email'),
        cell: ({ row }) => (
          <Link component={RouterLink} to={`/staff/${row.original.id}`} underline="hover">{row.original.email}</Link>
        ),
      },
      {
        accessorKey: 'isActive',
        header: t('staffAdmin_.status'),
        enableSorting: false,
        cell: ({ row }) => (
          <Stack direction="row" spacing={0.5}>
            <Chip size="small" color={row.original.isActive ? 'success' : 'default'} label={row.original.isActive ? t('staffAdmin_.active') : t('staffAdmin_.inactive')} />
            {row.original.isRoot && <Chip size="small" color="primary" label="Root" />}
          </Stack>
        ),
      },
      { accessorKey: 'preferredLanguage', header: t('staffAdmin_.language'), enableSorting: false },
      { accessorKey: 'companyCount', header: t('staffAdmin_.companies'), enableSorting: false, meta: { numeric: true } },
      {
        accessorKey: 'lastLoginAt',
        header: t('staffAdmin_.lastLogin'),
        enableSorting: false,
        cell: ({ row }) => formatDate(row.original.lastLoginAt),
      },
    ],
    [t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">{t('staffAdmin_.title')}</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreating(true)}>{t('staffAdmin_.new')}</Button>
      </Stack>
      <TextField
        size="small"
        label={t('staffAdmin_.search')}
        value={search}
        onChange={(e) => {
          setSearch(e.target.value)
          setPagination((p) => ({ ...p, pageIndex: 0 }))
        }}
      />
      {result.isError && <Alert severity="error">{t('staffAdmin_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('staffAdmin_.title')}
        rows={result.data?.items ?? []}
        columns={columns}
        rowCount={result.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={result.isLoading}
        emptyMessage={search ? undefined : t('staffAdmin_.empty')}
        getRowId={(row) => String(row.id)}
        minWidth={640}
      />
      <FormDialog open={creating} title={t('staffAdmin_.newTitle')} onClose={() => setCreating(false)} maxWidth="xs">
        {creating && (
          <StaffCreateForm
            onCancel={() => setCreating(false)}
            onSaved={(staff) => {
              setCreating(false)
              navigate(`/staff/${staff.id}`)
            }}
          />
        )}
      </FormDialog>
    </Stack>
  )
}
