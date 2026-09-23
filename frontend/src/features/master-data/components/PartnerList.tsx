import { useMemo, useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, Link, Stack, TextField, Tooltip, Typography } from '@mui/material'
import type { ColumnDef, PaginationState, SortingState } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { FormDialog } from '../../../shared/components/FormDialog'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { usePartners } from '../useMasterData'
import { PartnerDetail } from './PartnerDetail'
import { PartnerForm } from './PartnerForm'
import type { Partner } from '../types'

interface PartnerListProps {
  companyId: number
  onSelect?: (partner: Partner) => void
}

export function PartnerList({ companyId, onSelect }: PartnerListProps) {
  const { t } = useTranslation()
  const [search, setSearch] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const [sorting, setSorting] = useState<SortingState>([{ id: 'shortName', desc: false }])
  const [editing, setEditing] = useState<Partner | 'new'>()
  const [viewing, setViewing] = useState<Partner>()
  const sort = sorting[0]
  const partners = usePartners(companyId, {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    search,
    sortBy: sort?.id,
    descending: sort?.desc,
  })

  const columns = useMemo<ColumnDef<Partner>[]>(
    () => [
      {
        accessorKey: 'shortName',
        header: 'Kratak naziv',
        cell: ({ row, getValue }) => (
          <Link
            component="button"
            type="button"
            underline="hover"
            textAlign="left"
            onClick={() => {
              setViewing(row.original)
              onSelect?.(row.original)
            }}
          >
            {String(getValue())}
          </Link>
        ),
      },
      { accessorKey: 'name', header: 'Pun naziv', meta: { ellipsis: true } },
      { accessorKey: 'taxNumber', header: 'PIB', meta: { align: 'left', numeric: true } },
      { accessorKey: 'registrationNumber', header: 'Matični broj', enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'language', header: 'Jezik', enableSorting: false },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) => (
          <Tooltip title={t('ui.edit')}>
            <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(row.original)}>
              <EditIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [onSelect, t],
  )

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Typography component="h1" variant="h1">Partneri</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing('new')}>
          {t('partners_.new')}
        </Button>
      </Stack>
      <TextField
        label="Pretraga po nazivu ili PIB-u"
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
          setPagination((value) => ({ ...value, pageIndex: 0 }))
        }}
        size="small"
      />
      {partners.isError && <Alert severity="error">Partneri nisu mogli da se učitaju.</Alert>}
      <ServerDataTable
        ariaLabel="Partneri"
        rows={partners.data?.items ?? []}
        columns={columns}
        rowCount={partners.data?.totalCount ?? 0}
        pagination={pagination}
        sorting={sorting}
        onPaginationChange={setPagination}
        onSortingChange={setSorting}
        isLoading={partners.isLoading}
        emptyMessage={search ? undefined : t('partners_.empty')}
        getRowId={(row) => String(row.id)}
      />

      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('partners_.newTitle') : t('partners_.editTitle')}
        onClose={() => setEditing(undefined)}
        maxWidth="md"
      >
        {editing ? (
          <PartnerForm
            companyId={companyId}
            partner={editing === 'new' ? undefined : editing}
            onSaved={() => setEditing(undefined)}
            onCancel={() => setEditing(undefined)}
          />
        ) : null}
      </FormDialog>

      <FormDialog open={Boolean(viewing)} title={t('partners_.detailTitle')} onClose={() => setViewing(undefined)}>
        {viewing ? (
          <PartnerDetail
            partner={viewing}
            onEdit={() => {
              setEditing(viewing)
              setViewing(undefined)
            }}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
