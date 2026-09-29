import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Box, Button, Checkbox, FormControlLabel, Grid, Stack, Typography } from '@mui/material'
import type { ColumnDef, PaginationState } from '@tanstack/react-table'
import { useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
import { FormDialog } from '../../shared/components/FormDialog'
import { MoneyField } from '../../shared/components/MoneyField'
import { ServerDataTable } from '../../shared/components/ServerDataTable'
import { formatDate } from '../../shared/format/date'
import { formatMoney } from '../../shared/format/money'
import { useCompanyRole } from '../companies/useCompanyRole'
import { useDeleteNoticeCost, useNoticeCosts, useSaveNoticeCost, type NoticeCost } from './noticeApi'

const money = z.number({ error: 'Iznos mora biti broj.' }).min(0, 'Iznos ne može biti negativan.')
const schema = z
  .object({
    dateStart: z.string().min(1),
    dateEnd: z.string(),
    isGlobal: z.boolean(),
    aditionalCostsLowerAmount: money,
    aditionalCostsLowerLimit: money,
    aditionalCostsUpperAmount: money,
  })
  .refine((v) => !v.dateEnd || v.dateEnd >= v.dateStart, { path: ['dateEnd'], message: 'Kraj mora biti posle početka.' })

type FormValues = z.infer<typeof schema>

/** P11 "Troškovi opomena": threshold rows over time; company rows for Upravnik, global rows for Root only. */
export function NoticeCostsPanel({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const { role } = useCompanyRole()
  const isRoot = role === 'Root'
  const canEditCompany = isRoot || role === 'Upravnik'
  const query = useNoticeCosts(companyId)
  const remove = useDeleteNoticeCost(companyId)
  const [editing, setEditing] = useState<NoticeCost | 'new'>()
  const [deleting, setDeleting] = useState<NoticeCost | null>(null)
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 25 })
  const rows = query.data ?? []

  const columns = useMemo<ColumnDef<NoticeCost>[]>(
    () => [
      { id: 'scope', header: t('noticeCosts_.scope'), cell: ({ row }) => (row.original.companyId === null ? t('noticeCosts_.global') : t('noticeCosts_.company')) },
      { accessorKey: 'dateStart', header: t('noticeCosts_.dateStart'), cell: ({ getValue }) => formatDate(getValue<string>()) },
      { accessorKey: 'dateEnd', header: t('noticeCosts_.dateEnd'), cell: ({ getValue }) => formatDate(getValue<string | null>()) || t('noticeCosts_.active') },
      { accessorKey: 'aditionalCostsLowerLimit', header: t('noticeCosts_.lowerLimit'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'aditionalCostsLowerAmount', header: t('noticeCosts_.lowerAmount'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      { accessorKey: 'aditionalCostsUpperAmount', header: t('noticeCosts_.upperAmount'), cell: ({ getValue }) => formatMoney(getValue<number>()) },
      {
        id: 'actions',
        header: t('noticeCosts_.actions'),
        cell: ({ row }) =>
          (row.original.companyId === null ? isRoot : canEditCompany) ? (
            <Stack direction="row" spacing={1}>
              <Button size="small" onClick={() => setEditing(row.original)}>{t('noticeCosts_.edit')}</Button>
              <Button size="small" color="error" onClick={() => setDeleting(row.original)}>{t('noticeCosts_.delete')}</Button>
            </Stack>
          ) : null,
      },
    ],
    [t, isRoot, canEditCompany],
  )

  return (
    <Stack spacing={2} component="section" aria-labelledby="notice-costs-title">
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Box>
          <Typography id="notice-costs-title" variant="h6" component="h2">{t('noticeCosts_.title')}</Typography>
          <Typography color="text.secondary" variant="body2">{t('noticeCosts_.rule')}</Typography>
        </Box>
        {canEditCompany ? <Button variant="contained" onClick={() => setEditing('new')}>{t('noticeCosts_.add')}</Button> : null}
      </Stack>
      {query.error || remove.error ? <Alert severity="error">{getErrorMessage(query.error ?? remove.error, t('noticeCosts_.loadFailed'))}</Alert> : null}
      <ServerDataTable
        ariaLabel={t('noticeCosts_.title')}
        rows={rows.slice(pagination.pageIndex * pagination.pageSize, (pagination.pageIndex + 1) * pagination.pageSize)}
        columns={columns}
        rowCount={rows.length}
        pagination={pagination}
        sorting={[]}
        onPaginationChange={setPagination}
        onSortingChange={() => undefined}
        isLoading={query.isLoading}
        emptyMessage={t('noticeCosts_.empty')}
        getRowId={(row) => String(row.id)}
      />
      <FormDialog open={editing !== undefined} title={t(editing === 'new' ? 'noticeCosts_.add' : 'noticeCosts_.edit')} onClose={() => setEditing(undefined)}>
        {editing !== undefined ? (
          <NoticeCostForm companyId={companyId} row={editing === 'new' ? undefined : editing} isRoot={isRoot} onDone={() => setEditing(undefined)} />
        ) : null}
      </FormDialog>
      <ConfirmDialog
        open={deleting !== null}
        title={t('noticeCosts_.deleteTitle')}
        description={t('noticeCosts_.deleteBody')}
        confirmLabel={t('noticeCosts_.delete')}
        destructive
        pending={remove.isPending}
        onClose={() => setDeleting(null)}
        onConfirm={() => {
          if (deleting) remove.mutate(deleting.id)
          setDeleting(null)
        }}
      />
    </Stack>
  )
}

function NoticeCostForm({ companyId, row, isRoot, onDone }: { companyId: number; row?: NoticeCost; isRoot: boolean; onDone: () => void }) {
  const { t } = useTranslation()
  const save = useSaveNoticeCost(companyId)
  const { control, handleSubmit } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: row
      ? {
          dateStart: row.dateStart,
          dateEnd: row.dateEnd ?? '',
          isGlobal: row.companyId === null,
          aditionalCostsLowerAmount: row.aditionalCostsLowerAmount,
          aditionalCostsLowerLimit: row.aditionalCostsLowerLimit,
          aditionalCostsUpperAmount: row.aditionalCostsUpperAmount,
        }
      : { dateStart: '', dateEnd: '', isGlobal: false, aditionalCostsLowerAmount: 5000, aditionalCostsLowerLimit: 50000, aditionalCostsUpperAmount: 7500 },
  })
  const submit = async (value: FormValues) => {
    await save.mutateAsync({ id: row?.id, request: { ...value, dateEnd: value.dateEnd || null, rowVersion: row?.rowVersion ?? null } })
    onDone()
  }

  return (
    <Stack component="form" spacing={2} noValidate onSubmit={handleSubmit(submit)}>
      {save.error ? <Alert severity="error">{getErrorMessage(save.error, t('noticeCosts_.saveFailed'))}</Alert> : null}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="dateStart" label={t('noticeCosts_.dateStart')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="dateEnd" label={t('noticeCosts_.dateEnd')} type="date" helperText={t('noticeCosts_.dateEndHelp')} />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <MoneyField control={control} name="aditionalCostsLowerLimit" label={t('noticeCosts_.lowerLimit')} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <MoneyField control={control} name="aditionalCostsLowerAmount" label={t('noticeCosts_.lowerAmount')} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <MoneyField control={control} name="aditionalCostsUpperAmount" label={t('noticeCosts_.upperAmount')} required />
        </Grid>
      </Grid>
      {isRoot && !row ? (
        <Controller
          name="isGlobal"
          control={control}
          render={({ field }) => (
            <FormControlLabel control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />} label={t('noticeCosts_.isGlobal')} />
          )}
        />
      ) : null}
      <Stack direction="row" justifyContent="flex-end">
        <Button type="submit" variant="contained" disabled={save.isPending}>{t('noticeCosts_.save')}</Button>
      </Stack>
    </Stack>
  )
}
