/* eslint-disable react-refresh/only-export-components */
import { TextField } from '@mui/material'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

interface MonthYearFieldProps<TValues extends FieldValues> {
  control: Control<TValues>
  name: FieldPath<TValues>
  label: string
  required?: boolean
}

/** periodYYMM (e.g. 2601 = January 2026) as YY*100+MM, matching the format already used
 * throughout billing/suppliers (see SupplierInvoiceList's formatPeriodLabel). */
function toMonthInputValue(periodYYMM: number | undefined): string {
  if (!periodYYMM) return ''
  const month = periodYYMM % 100
  const year = 2000 + Math.floor(periodYYMM / 100)
  return `${year}-${String(month).padStart(2, '0')}`
}

function fromMonthInputValue(value: string): number {
  const [year, month] = value.split('-').map(Number)
  return year && month ? (year % 100) * 100 + month : Number.NaN
}

export function currentPeriodYYMM(): number {
  const now = new Date()
  return (now.getFullYear() % 100) * 100 + (now.getMonth() + 1)
}

/**
 * Native month/year picker (<input type="month">) for billing/document periods, replacing
 * a raw 4-digit YYMM number field — no date-picker library is installed, and the browser's
 * built-in month input already gives calendar-style month selection for free.
 */
export function MonthYearField<TValues extends FieldValues>({ control, name, label, required }: MonthYearFieldProps<TValues>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <TextField
          fullWidth
          size="small"
          type="month"
          label={label}
          required={required}
          value={toMonthInputValue(field.value as number | undefined)}
          onChange={(event) => field.onChange(fromMonthInputValue(event.target.value))}
          onBlur={field.onBlur}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message}
          slotProps={{ inputLabel: { shrink: true } }}
        />
      )}
    />
  )
}
