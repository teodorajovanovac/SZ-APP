import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Grid, Stack, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { useCreateInvoiceBatch } from './billingApi'

const schema = z.object({
  periodYYMM: z
    .number()
    .int()
    .min(1001)
    .max(9912)
    .refine((value) => { const month = value % 100; return month >= 1 && month <= 12 }, {
      message: 'Mesec (poslednje dve cifre) mora biti između 01 i 12.',
    }),
  caption: z.string().trim().min(1).max(50),
  place: z.string().trim().min(1).max(50),
  issueDate: z.string().min(1),
  serviceDateFrom: z.string().min(1),
  serviceDateTo: z.string().min(1),
  transactionDate: z.string().min(1),
  dueDate: z.string().min(1),
  exchangeRateNbs: z.number().positive(),
})

type FormValues = z.infer<typeof schema>

export function InvoiceBatchForm({ companyId, onCreated }: { companyId: number; onCreated?: () => void }) {
  const { t } = useTranslation()
  const create = useCreateInvoiceBatch(companyId)
  const { control, handleSubmit, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { periodYYMM: 2601, caption: '', place: '', issueDate: '', serviceDateFrom: '', serviceDateTo: '', transactionDate: '', dueDate: '', exchangeRateNbs: 1 },
  })
  const submit = async (value: FormValues) => {
    const month = value.periodYYMM % 100
    const year = 2000 + Math.floor(value.periodYYMM / 100)
    await create.mutateAsync({ ...value, month, year, extraordinaryInvoiceMarker: null, balanceAsOfDate: null, previousValueDate: null, isInterestCalculated: false, paymentPurpose: null })
    reset()
    onCreated?.()
  }

  return (
    <Stack component="form" onSubmit={handleSubmit(submit)} spacing={2} noValidate>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('billing_.form.notCreated'))}</Alert> : null}
      <Grid container spacing={2}>
        {([
          ['periodYYMM', t('billing_.form.period'), 'number'], ['caption', t('billing_.form.caption'), 'text'], ['place', t('billing_.form.place'), 'text'],
          ['issueDate', t('billing_.form.issueDate'), 'date'], ['serviceDateFrom', t('billing_.form.serviceFrom'), 'date'], ['serviceDateTo', t('billing_.form.serviceTo'), 'date'],
          ['transactionDate', t('billing_.form.transactionDate'), 'date'], ['dueDate', t('billing_.form.dueDate'), 'date'], ['exchangeRateNbs', t('billing_.form.exchangeRate'), 'number'],
        ] as const).map(([name, label, type]) => (
          <Grid key={name} size={{ xs: 12, sm: 6 }}>
            <Controller name={name} control={control} render={({ field, fieldState }) => (
              <TextField {...field} onChange={(event) => field.onChange(type === 'number' ? Number(event.target.value) : event.target.value)} fullWidth type={type} label={label} error={Boolean(fieldState.error)} helperText={fieldState.error?.message} slotProps={{ inputLabel: { shrink: type === 'date' } }} />
            )} />
          </Grid>
        ))}
      </Grid>
      <Button type="submit" variant="contained" disabled={create.isPending}>{t('billing_.form.submit')}</Button>
    </Stack>
  )
}
