import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Autocomplete, Button, Divider, Grid, Stack, TextField, Typography } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
import { usePartnerAccounts } from '../master-data/useMasterData'
import { useCreateSupplierInvoice } from './supplierApi'

const schema = z.object({
  invoiceNo: z.number().int().positive(), codeName: z.string().min(1), caption: z.string().min(1),
  supplierPartnerAccountId: z.number().int().positive(), calculationTypeId: z.number().int().positive(),
  periodYYMM: z.number().int().min(1001).max(9912).refine((value) => { const month = value % 100; return month >= 1 && month <= 12 }, {
    message: 'Mesec (poslednje dve cifre) mora biti između 01 i 12.',
  }),
  amountRsd: z.number().nonnegative(), documentTypeId: z.number().int().positive(),
  invoiceDate: z.string().min(1), transactionDate: z.string().min(1),
})
type Values = z.infer<typeof schema>

function SectionHeading({ children }: { children: string }) {
  return (
    <Grid size={12}>
      <Typography variant="overline" color="text.secondary" component="h3">{children}</Typography>
      <Divider />
    </Grid>
  )
}

export function SupplierInvoiceForm({ companyId, onSaved }: { companyId: number; onSaved?: () => void }) {
  const { t } = useTranslation()
  const create = useCreateSupplierInvoice(companyId)
  // ponytail: calculationTypeId/documentTypeId stay raw numeric inputs — no lookup-list
  // endpoint is exposed for those yet. Wire up an Autocomplete once one exists.
  const partnerAccounts = usePartnerAccounts(companyId, { page: 1, pageSize: 200 })
  const { control, handleSubmit } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { invoiceNo: 1, codeName: '', caption: '', supplierPartnerAccountId: 0, calculationTypeId: 0, periodYYMM: 2601, amountRsd: 0, documentTypeId: 0, invoiceDate: '', transactionDate: '' } })
  const submit = (value: Values) => create.mutate({
    ...value, invoiceTotalCalculationAmountEur: 0, invoiceTotalCalculationAmountRsd: value.amountRsd,
    calculationAmountByCoefficientEur: 0, calculationAmountByCoefficientRsd: value.amountRsd,
    paymentPriority: value.invoiceNo, subAccountId: null, extraordinaryInvoiceMarker: null, invoiceNameRule: null,
    paymentDate: null, invoiceDescription: null, paymentReference: null, unitTypeIds: [],
  }, { onSuccess: onSaved })

  return (
    <Stack component="form" onSubmit={handleSubmit(submit)} spacing={2} noValidate>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('suppliers_.form.notSaved'))}</Alert> : null}
      <Grid container spacing={2} columnSpacing={3}>
        <SectionHeading>{t('suppliersUi.sectionInvoice')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="supplierPartnerAccountId"
            control={control}
            render={({ field, fieldState }) => (
              <Autocomplete
                options={partnerAccounts.data?.items ?? []}
                loading={partnerAccounts.isLoading}
                size="small"
                getOptionLabel={(option) => option.account}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                value={partnerAccounts.data?.items.find((account) => account.id === field.value) ?? null}
                onChange={(_, option) => field.onChange(option?.id ?? 0)}
                renderInput={(params) => (
                  <TextField {...params} label={t('suppliers_.form.partnerAccount')} required error={Boolean(fieldState.error)} helperText={fieldState.error?.message} />
                )}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="invoiceNo" label={t('suppliers_.form.invoiceNo')} type="number" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="codeName" label={t('suppliers_.form.codeName')} required />
        </Grid>
        <Grid size={12}>
          <ControlledTextField control={control} name="caption" label={t('suppliers_.form.caption')} required />
        </Grid>

        <SectionHeading>{t('suppliersUi.sectionAmounts')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="periodYYMM" label={t('suppliers_.form.period')} type="number" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="amountRsd" label={t('suppliers_.form.amount')} type="number" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="calculationTypeId" label={t('suppliers_.form.calculationType')} type="number" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <ControlledTextField control={control} name="documentTypeId" label={t('suppliers_.form.documentType')} type="number" required />
        </Grid>

        <SectionHeading>{t('suppliersUi.sectionDates')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="invoiceDate" label={t('suppliers_.form.invoiceDate')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="transactionDate" label={t('suppliers_.form.transactionDate')} type="date" required />
        </Grid>
      </Grid>
      <Stack direction="row" justifyContent="flex-end">
        <Button type="submit" variant="contained" disabled={create.isPending}>{t('suppliers_.form.submit')}</Button>
      </Stack>
    </Stack>
  )
}
