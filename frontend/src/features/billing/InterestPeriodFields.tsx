import { Alert, FormControlLabel, FormLabel, Grid, Radio, RadioGroup, Switch, Typography } from '@mui/material'
import { Controller, useWatch, type Control, type UseFormSetValue } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { ControlledTextField } from '../../shared/components/ControlledTextField'
import { formatDate } from '../../shared/format/date'
import { resolveInterestPeriod, useInterestPeriodPresets, type InterestPresetKey } from './billingApi'
import type { InvoiceBatchFormValues } from './InvoiceBatchForm'

const validPeriod = (value: number) => value >= 1001 && value <= 9912 && value % 100 >= 1 && value % 100 <= 12

// P10: interest period is a per-run parameter; presets are suggestions computed by the server.
export function InterestPeriodFields({
  companyId,
  control: c,
  setValue: set,
}: {
  companyId: number
  control: Control<InvoiceBatchFormValues>
  setValue: UseFormSetValue<InvoiceBatchFormValues>
}) {
  const { t } = useTranslation()
  const [periodYYMM, dueDate, enabled, previousValueDate, balanceAsOfDate, interestPreset, interestStart, interestEnd] = useWatch({
    control: c,
    name: ['periodYYMM', 'dueDate', 'interestEnabled', 'previousValueDate', 'balanceAsOfDate', 'interestPreset', 'interestStart', 'interestEnd'],
  })
  const presets = useInterestPeriodPresets(companyId, { periodYYMM, previousValueDate, balanceAsOfDate, dueDate }, enabled && validPeriod(periodYYMM))
  const resolved = resolveInterestPeriod({ interestPreset, interestStart, interestEnd }, presets.data?.presets, presets.data?.defaultPreset)

  return (
    <>
      <Grid size={12}>
        <Controller
          name="interestEnabled"
          control={c}
          render={({ field }) => (
            <FormControlLabel control={<Switch checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />} label={t('interest_.enabled')} />
          )}
        />
      </Grid>
      {enabled ? (
        <>
          <Grid size={{ xs: 12, sm: 6 }}>
            <ControlledTextField control={c} name="previousValueDate" label={t('interest_.previousValueDate')} type="date" />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <ControlledTextField control={c} name="balanceAsOfDate" label={t('interest_.balanceAsOfDate')} type="date" />
          </Grid>
          <Grid size={12}>
            <FormLabel id="interest-preset-label">{t('interest_.preset')}</FormLabel>
            <RadioGroup
              aria-labelledby="interest-preset-label"
              value={resolved.key}
              onChange={(e) => {
                const key = e.target.value as InterestPresetKey
                if (key === 'custom') {
                  set('interestStart', resolved.start)
                  set('interestEnd', resolved.end)
                }
                set('interestPreset', key)
              }}
            >
              {(presets.data?.presets ?? []).map((p) => (
                <FormControlLabel
                  key={p.key}
                  value={p.key}
                  control={<Radio />}
                  label={p.key === 'custom' ? t('interest_.presets.custom') : `${t(`interest_.presets.${p.key}`)}: ${formatDate(p.start)} – ${formatDate(p.end)}`}
                />
              ))}
            </RadioGroup>
          </Grid>
          {resolved.key === 'custom' ? (
            <>
              <Grid size={{ xs: 12, sm: 6 }}>
                <ControlledTextField control={c} name="interestStart" label={t('interest_.from')} type="date" required />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <ControlledTextField control={c} name="interestEnd" label={t('interest_.to')} type="date" required />
              </Grid>
            </>
          ) : null}
          <Grid size={12}>
            <Typography variant="body2" color="text.secondary">
              {t('interest_.resolved', { from: formatDate(resolved.start), to: formatDate(resolved.end) })}
              {presets.data?.lastRunEnd ? ` ${t('interest_.lastRunEnd', { date: formatDate(presets.data.lastRunEnd) })}` : ''}
            </Typography>
          </Grid>
          {presets.error ? (
            <Grid size={12}>
              <Alert severity="warning">{getErrorMessage(presets.error, t('interest_.presetsFailed'))}</Alert>
            </Grid>
          ) : null}
        </>
      ) : null}
    </>
  )
}
