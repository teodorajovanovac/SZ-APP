import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Autocomplete, Button, Grid, Stack, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
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

export function SupplierInvoiceForm({ companyId }: { companyId: number }) {
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
  })
  return (
    <Stack component="form" onSubmit={handleSubmit(submit)} spacing={2} noValidate>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('suppliers_.form.notSaved'))}</Alert> : null}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="supplierPartnerAccountId"
            control={control}
            render={({ field, fieldState }) => (
              <Autocomplete
                options={partnerAccounts.data?.items ?? []}
                loading={partnerAccounts.isLoading}
                getOptionLabel={(option) => option.account}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                value={partnerAccounts.data?.items.find((account) => account.id === field.value) ?? null}
                onChange={(_, option) => field.onChange(option?.id ?? 0)}
                renderInput={(params) => (
                  <TextField {...params} label={t('suppliers_.form.partnerAccount')} error={Boolean(fieldState.error)} helperText={fieldState.error?.message} />
                )}
              />
            )}
          />
        </Grid>
        {([
          ['invoiceNo', t('suppliers_.form.invoiceNo'), 'number'], ['codeName', t('suppliers_.form.codeName'), 'text'], ['caption', t('suppliers_.form.caption'), 'text'],
          ['calculationTypeId', t('suppliers_.form.calculationType'), 'number'],
          ['periodYYMM', t('suppliers_.form.period'), 'number'], ['amountRsd', t('suppliers_.form.amount'), 'number'], ['documentTypeId', t('suppliers_.form.documentType'), 'number'],
          ['invoiceDate', t('suppliers_.form.invoiceDate'), 'date'], ['transactionDate', t('suppliers_.form.transactionDate'), 'date'],
        ] as const).map(([name, label, type]) => <Grid key={name} size={{ xs: 12, sm: 6 }}><Controller name={name} control={control} render={({ field, fieldState }) => <TextField {...field} onChange={(event) => field.onChange(type === 'number' ? Number(event.target.value) : event.target.value)} fullWidth label={label} type={type} error={Boolean(fieldState.error)} helperText={fieldState.error?.message} slotProps={{ inputLabel: { shrink: type === 'date' } }} />} /></Grid>)}
      </Grid>
      <Button type="submit" variant="contained" disabled={create.isPending}>{t('suppliers_.form.submit')}</Button>
    </Stack>
  )
}
