import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Grid, Stack, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { useCreateSupplierInvoice } from './supplierApi'

const schema = z.object({
  invoiceNo: z.number().int().positive(), codeName: z.string().min(1), caption: z.string().min(1),
  supplierPartnerAccountId: z.number().int().positive(), calculationTypeId: z.number().int().positive(),
  periodYYMM: z.number().int(), amountRsd: z.number().nonnegative(), documentTypeId: z.number().int().positive(),
  invoiceDate: z.string().min(1), transactionDate: z.string().min(1),
})
type Values = z.infer<typeof schema>

export function SupplierInvoiceForm({ companyId }: { companyId: number }) {
  const create = useCreateSupplierInvoice(companyId)
  const { control, handleSubmit } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { invoiceNo: 1, codeName: '', caption: '', supplierPartnerAccountId: 0, calculationTypeId: 0, periodYYMM: 2601, amountRsd: 0, documentTypeId: 0, invoiceDate: '', transactionDate: '' } })
  const submit = (value: Values) => create.mutate({
    ...value, invoiceTotalCalculationAmountEur: 0, invoiceTotalCalculationAmountRsd: value.amountRsd,
    calculationAmountByCoefficientEur: 0, calculationAmountByCoefficientRsd: value.amountRsd,
    paymentPriority: value.invoiceNo, subAccountId: null, extraordinaryInvoiceMarker: null, invoiceNameRule: null,
    paymentDate: null, invoiceDescription: null, paymentReference: null, unitTypeIds: [],
  })
  return (
    <Stack component="form" onSubmit={handleSubmit(submit)} spacing={2} noValidate>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, 'Dobavljački račun nije sačuvan.')}</Alert> : null}
      <Grid container spacing={2}>{([
        ['invoiceNo', 'Redni broj', 'number'], ['codeName', 'Šifra', 'text'], ['caption', 'Naziv', 'text'],
        ['supplierPartnerAccountId', 'Konto dobavljača', 'number'], ['calculationTypeId', 'Tip obračuna', 'number'],
        ['periodYYMM', 'Period YYMM', 'number'], ['amountRsd', 'Iznos RSD', 'number'], ['documentTypeId', 'Tip dokumenta', 'number'],
        ['invoiceDate', 'Datum računa', 'date'], ['transactionDate', 'Datum prometa', 'date'],
      ] as const).map(([name, label, type]) => <Grid key={name} size={{ xs: 12, sm: 6 }}><Controller name={name} control={control} render={({ field, fieldState }) => <TextField {...field} onChange={(event) => field.onChange(type === 'number' ? Number(event.target.value) : event.target.value)} fullWidth label={label} type={type} error={Boolean(fieldState.error)} helperText={fieldState.error?.message} slotProps={{ inputLabel: { shrink: type === 'date' } }} />} /></Grid>)}</Grid>
      <Button type="submit" variant="contained" disabled={create.isPending}>Sačuvaj dobavljački račun</Button>
    </Stack>
  )
}
