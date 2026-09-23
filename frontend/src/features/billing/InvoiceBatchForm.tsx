import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Divider, Grid, Stack, Typography } from '@mui/material'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
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

function SectionHeading({ children }: { children: string }) {
  return (
    <Grid size={12}>
      <Typography variant="overline" color="text.secondary" component="h3">{children}</Typography>
      <Divider />
    </Grid>
  )
}

export function InvoiceBatchForm({ companyId, onCreated }: { companyId: number; onCreated?: () => void }) {
  const { t } = useTranslation()
  const create = useCreateInvoiceBatch(companyId)
  const { control, handleSubmit, reset, formState } = useForm<FormValues>({
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
      <Typography variant="h6" component="h2">{t('billingUi.formTitle')}</Typography>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('billing_.form.notCreated'))}</Alert> : null}
      {create.isSuccess && formState.isSubmitSuccessful ? <Alert severity="success">{t('billingUi.created')}</Alert> : null}
      <Grid container spacing={2} columnSpacing={3}>
        <SectionHeading>{t('billingUi.sectionIdentification')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 4 }}>
          <ControlledTextField control={control} name="periodYYMM" label={t('billing_.form.period')} type="number" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <ControlledTextField control={control} name="caption" label={t('billing_.form.caption')} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <ControlledTextField control={control} name="place" label={t('billing_.form.place')} required />
        </Grid>

        <SectionHeading>{t('billingUi.sectionDates')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <ControlledTextField control={control} name="issueDate" label={t('billing_.form.issueDate')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <ControlledTextField control={control} name="serviceDateFrom" label={t('billing_.form.serviceFrom')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <ControlledTextField control={control} name="serviceDateTo" label={t('billing_.form.serviceTo')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <ControlledTextField control={control} name="transactionDate" label={t('billing_.form.transactionDate')} type="date" required />
        </Grid>

        <SectionHeading>{t('billingUi.sectionAmounts')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="dueDate" label={t('billing_.form.dueDate')} type="date" required />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <ControlledTextField control={control} name="exchangeRateNbs" label={t('billing_.form.exchangeRate')} type="number" required />
        </Grid>
      </Grid>
      <Stack direction="row" justifyContent="flex-end">
        <Button type="submit" variant="contained" disabled={create.isPending}>{t('billing_.form.submit')}</Button>
      </Stack>
    </Stack>
  )
}
