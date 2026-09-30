import { useMemo, useState } from 'react'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, IconButton, Link, Stack, Tooltip } from '@mui/material'
import type { ColumnDef } from '@tanstack/react-table'
import { useTranslation } from 'react-i18next'
import { FormDialog } from '../../../shared/components/FormDialog'
import { PageHeader } from '../../../shared/components/PageHeader'
import { SearchField } from '../../../shared/components/SearchField'
import { ServerDataTable } from '../../../shared/components/ServerDataTable'
import { useNotify } from '../../../shared/feedback/NotifyProvider'
import { urlTableProps, useUrlState } from '../../../shared/hooks/useUrlState'
import { useAddresses } from '../useMasterData'
import { AddressForm } from './AddressForm'
import type { Address } from '../types'

interface AddressListProps {
  companyId: number
  onSelect?: (address: Address) => void
}

export function AddressList({ companyId, onSelect }: AddressListProps) {
  const { t } = useTranslation()
  const notify = useNotify()
  // The API orders by city (then street); only the direction is selectable.
  const [url, setUrl] = useUrlState({ q: '', page: 0, size: 25, sort: 'city', desc: false })
  const table = urlTableProps(url, setUrl)
  const [editing, setEditing] = useState<Address | 'new'>()
  const [formKey, setFormKey] = useState(0)
  const [savedId, setSavedId] = useState<string>()
  const result = useAddresses(companyId, { page: url.page + 1, pageSize: url.size, search: url.q, descending: url.desc })
  const onSaved = (address: Address) => {
    setSavedId(String(address.id))
    notify({ message: t('addresses_.saved') })
  }
  const columns = useMemo<ColumnDef<Address>[]>(
    () => [
      {
        accessorKey: 'streetAddress',
        header: t('fields.address'),
        cell: ({ row, getValue }) => (
          <Link component="button" type="button" textAlign="left" onClick={() => (onSelect ? onSelect(row.original) : setEditing(row.original))}>
            {String(getValue())}
          </Link>
        ),
      },
      { accessorKey: 'postalCode', header: t('fields.postalCode'), meta: { align: 'left', numeric: true } },
      { accessorKey: 'city', header: t('fields.city'), enableSorting: true },
      { accessorKey: 'countryCode', header: t('fields.country') },
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
    [onSelect, t],
  )

  return (
    <Stack spacing={2}>
      <PageHeader
        title={t('addresses_.title')}
        onNew={() => setEditing('new')}
        newLabel={t('addresses_.new')}
        filters={<SearchField label={t('addresses_.search')} value={url.q} onChange={(q) => setUrl({ q })} />}
      />
      <Alert severity="info">{t('addresses_.rootOnly')}</Alert>
      {result.isError && <Alert severity="error">{t('addresses_.loadError')}</Alert>}
      <ServerDataTable
        ariaLabel={t('addresses_.title')}
        rows={result.data?.items ?? []}
        columns={columns}
        rowCount={result.data?.totalCount ?? 0}
        {...table}
        isLoading={result.isLoading}
        emptyMessage={url.q ? undefined : t('addresses_.empty')}
        getRowId={(row) => String(row.id)}
        highlightRowId={savedId}
      />

      <FormDialog
        open={Boolean(editing)}
        title={editing === 'new' ? t('addresses_.newTitle') : t('addresses_.editTitle')}
        onClose={() => setEditing(undefined)}
      >
        {editing ? (
          <AddressForm
            key={formKey}
            companyId={companyId}
            address={editing === 'new' ? undefined : editing}
            onSaved={(address) => { onSaved(address); setEditing(undefined) }}
            onSavedNew={editing === 'new' ? (address) => { onSaved(address); setFormKey((k) => k + 1) } : undefined}
            onCancel={() => setEditing(undefined)}
          />
        ) : null}
      </FormDialog>
    </Stack>
  )
}
