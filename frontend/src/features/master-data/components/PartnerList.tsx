import { useMemo, useState } from 'react'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, IconButton, Link, Stack, Tooltip } from '@mui/material'
import type { ColumnDef } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { FormDialog } from '../../../shared/components/FormDialog'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SearchField } from '../../../shared/components/SearchField'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { urlTableProps, useUrlState } from '../../../shared/hooks/useUrlState'
import { usePartners } from '../useMasterData'
import { PartnerDetail } from './PartnerDetail'
import type { Partner } from '../types'

interface PartnerListProps {
  companyId: number
  onSelect?: (partner: Partner) => void
}

export function PartnerList({ companyId, onSelect }: PartnerListProps) {
  const { t } = useTranslation()
  const [url, setUrl] = useUrlState({ q: '', page: 0, size: 25, sort: 'shortName', desc: false })
  const table = urlTableProps(url, setUrl)
  const navigate = useNavigate()
  const [viewing, setViewing] = useState<Partner>()
  const partners = usePartners(companyId, {
    page: url.page + 1,
    pageSize: url.size,
    search: url.q,
    sortBy: url.sort,
    descending: url.desc,
  })

  const columns = useMemo<ColumnDef<Partner>[]>(
    () => [
      {
        accessorKey: 'shortName',
        header: t('fields.shortName'),
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
      { accessorKey: 'name', header: t('fields.fullName'), meta: { ellipsis: true } },
      { accessorKey: 'taxNumber', header: t('fields.taxNumber'), meta: { align: 'left', numeric: true } },
      { accessorKey: 'registrationNumber', header: t('fields.registrationNumber'), enableSorting: false, meta: { align: 'left', numeric: true } },
      { accessorKey: 'language', header: t('fields.language'), enableSorting: false },
      {
        id: 'actions',
        header: t('ui.actions'),
        enableSorting: false,
        meta: { align: 'right' },
        cell: ({ row }) => (
          <Tooltip title={t('ui.edit')}>
            <IconButton size="small" aria-label={t('ui.edit')} onClick={() => navigate(`/partners/${row.original.companyId}/${row.original.id}/edit`)}>
              <EditIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        ),
      },
    ],
    [navigate, onSelect, t],
  )

  return (
    <Stack spacing={2}>
      <PageHeader
        title={t('partners_.title')}
        subtitle={t('partners_.subtitle')}
        onNew={() => navigate('/partners/new')}
        newLabel={t('partners_.new')}
        filters={<SearchField label={t('partners_.search')} value={url.q} onChange={(q) => setUrl({ q })} />}
      />
      {partners.isError && <Alert severity="error">{t('partners_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('partners_.title')}
        rows={partners.data?.items ?? []}
        columns={columns}
        rowCount={partners.data?.totalCount ?? 0}
        {...table}
        isLoading={partners.isLoading}
        emptyMessage={url.q ? undefined : t('partners_.empty')}
        emptyHint={url.q ? t('partners_.emptyHint') : undefined}
        getRowId={(row) => String(row.id)}
      />

      <FormDialog open={Boolean(viewing)} title={t('partners_.detailTitle')} onClose={() => setViewing(undefined)}>
        {viewing ? (
          <PartnerDetail
            partner={viewing}
            onEdit={() => {
              navigate(`/partners/${viewing.companyId}/${viewing.id}/edit`)
            }}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
