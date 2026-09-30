import { useMemo, useState } from 'react'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, IconButton, Link, Stack, Tooltip } from '@mui/material'
import type { ColumnDef } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'
import { FormDialog } from '../../../shared/components/FormDialog'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SearchField } from '../../../shared/components/SearchField'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useNotify } from '../../../shared/feedback/NotifyProvider'
import { urlTableProps, useUrlState } from '../../../shared/hooks/useUrlState'
import { usePartners } from '../useMasterData'
import { PartnerForm } from './PartnerForm'
import type { Partner } from '../types'

interface PartnerListProps {
  companyId: number
}

export function PartnerList({ companyId }: PartnerListProps) {
  const { t } = useTranslation()
  const notify = useNotify()
  const [url, setUrl] = useUrlState({ q: '', page: 0, size: 25, sort: 'shortName', desc: false })
  const table = urlTableProps(url, setUrl)
  const [editing, setEditing] = useState<Partner | 'new'>()
  const [formKey, setFormKey] = useState(0)
  const [savedId, setSavedId] = useState<string>()
  const partners = usePartners(companyId, {
    page: url.page + 1,
    pageSize: url.size,
    search: url.q,
    sortBy: url.sort,
    descending: url.desc,
  })

  const onSaved = (partner: Partner) => {
    setSavedId(String(partner.id))
    notify({ message: t('partners_.saved', { name: partner.shortName }), link: { to: `/partners/${companyId}/${partner.id}`, label: t('partners_.open') } })
  }

  const columns = useMemo<ColumnDef<Partner>[]>(
    () => [
      {
        accessorKey: 'shortName',
        header: t('fields.shortName'),
        enableSorting: true,
        // UX-22: a real link, so Ctrl+click opens the partner in a new tab.
        cell: ({ row, getValue }) => (
          <Link component={RouterLink} to={`/partners/${companyId}/${row.original.id}`}>
            {String(getValue())}
          </Link>
        ),
      },
      { accessorKey: 'name', header: t('fields.fullName'), enableSorting: true, meta: { ellipsis: true } },
      { accessorKey: 'taxNumber', header: t('fields.taxNumber'), enableSorting: true, meta: { align: 'left', numeric: true } },
      { accessorKey: 'registrationNumber', header: t('fields.registrationNumber'), meta: { align: 'left', numeric: true } },
      { accessorKey: 'language', header: t('fields.language') },
      {
        id: 'actions',
        header: t('ui.actions'),
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
    [companyId, t],
  )

  return (
    <Stack spacing={2}>
      <PageHeader
        title={t('partners_.title')}
        subtitle={t('partners_.subtitle')}
        onNew={() => setEditing('new')}
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
        highlightRowId={savedId}
      />

      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('partners_.newTitle') : t('partners_.editTitle')}
        onClose={() => setEditing(undefined)}
        maxWidth="md"
      >
        {editing ? (
          <PartnerForm
            key={formKey}
            companyId={companyId}
            partner={editing === 'new' ? undefined : editing}
            onSaved={(partner) => { onSaved(partner); setEditing(undefined) }}
            onSavedNew={editing === 'new' ? (partner) => { onSaved(partner); setFormKey((k) => k + 1) } : undefined}
            onCancel={() => setEditing(undefined)}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
