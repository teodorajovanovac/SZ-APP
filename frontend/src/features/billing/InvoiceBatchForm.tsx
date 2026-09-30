import { zodResolver } from '@hookform/resolvers/zod'
import { Alert, Button, Divider, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { getErrorMessage } from '../../api/problemDetails'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
import { MoneyField } from '../../shared/components/MoneyField'
import { currentPeriodYYMM, MonthYearField } from '../../shared/components/MonthYearField'
import { useQueryClient } from '@tanstack/react-query'
import { formatMoney } from '../../shared/format/money'
import { interestPresetsQuery, resolveInterestPeriod, useCreateInvoiceBatch, useRunInterest } from './billingApi'
import { InterestPeriodFields } from './InterestPeriodFields'
import { InvoiceBatchPreview } from './InvoiceBatchPreview'
import type { GenerateInvoicesRequest } from './types'
import { useState } from 'react'

/** Item 8: the wizard defaults to next month. */
function nextPeriodYYMM() {
  const p = currentPeriodYYMM()
  return p % 100 === 12 ? (Math.floor(p / 100) + 1) * 100 + 1 : p + 1
}

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
  exchangeRateNbs: z.number({ error: 'Kurs mora biti broj.' }).positive('Kurs mora biti pozitivan broj.'),
  // P10 / FIN-27: interest is on by default; the period is an explicit per-run parameter.
  interestEnabled: z.boolean(),
  previousValueDate: z.string(),
  balanceAsOfDate: z.string(),
  interestPreset: z.enum(['fromPreviousDueDate', 'wholeMonth', 'dueToDue', 'custom', '']),
  interestStart: z.string(),
  interestEnd: z.string(),
}).refine((v) => v.interestPreset !== 'custom' || (v.interestStart !== '' && v.interestEnd !== '' && v.interestStart <= v.interestEnd), {
  path: ['interestEnd'],
  message: 'Početak perioda kamate mora biti pre kraja.',
})

export type InvoiceBatchFormValues = z.infer<typeof schema>
type FormValues = InvoiceBatchFormValues

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
  const runInterest = useRunInterest(companyId)
  const queryClient = useQueryClient()
  const [generation, setGeneration] = useState<GenerateInvoicesRequest | null>(null)
  const [scope, setScope] = useState<'single' | 'location' | 'all'>('single')
  const [locationCategoryId, setLocationCategoryId] = useState('')
  const { control, handleSubmit, reset, formState, setValue } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      periodYYMM: nextPeriodYYMM(), caption: '', place: '', issueDate: '', serviceDateFrom: '', serviceDateTo: '', transactionDate: '', dueDate: '', exchangeRateNbs: 1,
      interestEnabled: true, previousValueDate: '', balanceAsOfDate: '', interestPreset: '', interestStart: '', interestEnd: '',
    },
  })
  const submit = async (value: FormValues) => {
    const month = value.periodYYMM % 100
    const year = 2000 + Math.floor(value.periodYYMM / 100)
    const { interestEnabled, previousValueDate, balanceAsOfDate, interestPreset, interestStart, interestEnd, ...batch } = value
    let period = { start: '', end: '' }
    if (interestEnabled) {
      // Same inputs as the preset query the fields showed, so this is served from cache.
      const presets = await queryClient.fetchQuery(interestPresetsQuery(companyId, { periodYYMM: value.periodYYMM, previousValueDate, balanceAsOfDate, dueDate: value.dueDate }))
      period = resolveInterestPeriod({ interestPreset, interestStart, interestEnd }, presets.presets, presets.defaultPreset)
    }
    // Multi-company scope: the server creates one batch per company and runs interest after generation.
    const multi = scope !== 'single'
    if (!multi) {
      const created = await create.mutateAsync({
        ...batch, month, year, extraordinaryInvoiceMarker: null,
        balanceAsOfDate: balanceAsOfDate || null, previousValueDate: previousValueDate || null,
        isInterestCalculated: interestEnabled, paymentPurpose: null,
      })
      if (interestEnabled) {
        await runInterest.mutateAsync({ invoiceBatchId: created.id, periodStart: period.start, periodEnd: period.end })
      }
    }
    setGeneration({
      periodYYMM: value.periodYYMM, extraordinaryMarker: null, place: value.place, issueDate: value.issueDate, dueDate: value.dueDate,
      serviceDateFrom: value.serviceDateFrom, serviceDateTo: value.serviceDateTo, transactionDate: value.transactionDate, exchangeRateNbs: value.exchangeRateNbs,
      scope, locationCategoryId: scope === 'location' && locationCategoryId ? Number(locationCategoryId) : null,
      interestPeriodStart: multi && interestEnabled ? period.start : null,
      interestPeriodEnd: multi && interestEnabled ? period.end : null,
    })
    reset()
    onCreated?.()
  }

  return (
    <Stack component="form" onSubmit={handleSubmit(submit)} spacing={2} noValidate>
      <Typography variant="h6" component="h2">{t('billingUi.formTitle')}</Typography>
      {create.error ? <Alert severity="error">{getErrorMessage(create.error, t('billing_.form.notCreated'))}</Alert> : null}
      {runInterest.error ? <Alert severity="error">{getErrorMessage(runInterest.error, t('interest_.runFailed'))}</Alert> : null}
      {runInterest.data && formState.isSubmitSuccessful ? (
        <Alert severity="info">{t('interest_.runDone', { total: formatMoney(runInterest.data.totalInterest), count: runInterest.data.totals.length })}</Alert>
      ) : null}
      {create.isSuccess && formState.isSubmitSuccessful ? <Alert severity="success">{t('billing2_.created')}</Alert> : null}
      <Grid container spacing={2} columnSpacing={3}>
        <SectionHeading>{t('billingUi.sectionIdentification')}</SectionHeading>
        <Grid size={{ xs: 12, sm: 4 }}>
          <MonthYearField control={control} name="periodYYMM" label={t('billing_.form.period')} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <ControlledTextField control={control} name="caption" label={t('billing_.form.caption')} required />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <ControlledTextField control={control} name="place" label={t('billing_.form.place')} required />
        </Grid>

        <Grid size={{ xs: 12, sm: 4 }}>
          <TextField select fullWidth size="small" label={t('invx_.scope')} value={scope} onChange={(e) => setScope(e.target.value as typeof scope)}>
            <MenuItem value="single">{t('invx_.scopeSingle')}</MenuItem>
            <MenuItem value="location">{t('invx_.scopeLocation')}</MenuItem>
            <MenuItem value="all">{t('invx_.scopeAll')}</MenuItem>
          </TextField>
        </Grid>
        {scope === 'location' ? (
          <Grid size={{ xs: 12, sm: 4 }}>
            <TextField fullWidth size="small" type="number" required label={t('invx_.locationId')} value={locationCategoryId} onChange={(e) => setLocationCategoryId(e.target.value)} />
          </Grid>
        ) : null}

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
          <MoneyField control={control} name="exchangeRateNbs" label={t('billing_.form.exchangeRate')} required />
        </Grid>

        <SectionHeading>{t('interest_.section')}</SectionHeading>
        <InterestPeriodFields companyId={companyId} control={control} setValue={setValue} />
      </Grid>
      <Stack direction="row" justifyContent="flex-end">
        <Button type="submit" variant="contained" disabled={create.isPending}>{t('billing_.form.submit')}</Button>
      </Stack>
      {generation ? <InvoiceBatchPreview companyId={companyId} request={generation} /> : null}
    </Stack>
  )
}
