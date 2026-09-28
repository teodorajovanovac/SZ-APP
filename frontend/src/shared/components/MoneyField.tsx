import { useRef, useState } from 'react'
import { TextField } from '@mui/material'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

interface MoneyFieldProps<TValues extends FieldValues> {
  control: Control<TValues>
  name: FieldPath<TValues>
  label: string
  required?: boolean
}

function toDisplay(value: unknown): string {
  if (typeof value !== 'number' || Number.isNaN(value)) return ''
  return value.toLocaleString('sr-Latn-RS', { maximumFractionDigits: 2 })
}

function parseSerbian(text: string): number {
  const normalized = text.trim().replace(/\./g, '').replace(',', '.')
  return normalized === '' ? Number.NaN : Number(normalized)
}

// Digits, optional thousands dots, at most one decimal comma while typing.
const typingPattern = /^\d*(\.\d*)*(,\d*)?$/

function MoneyInput({
  value,
  onChange,
  onBlur,
  label,
  required,
  error,
  helperText,
}: {
  value: number | undefined
  onChange: (value: number) => void
  onBlur: () => void
  label: string
  required?: boolean
  error?: boolean
  helperText?: string
}) {
  const [text, setText] = useState(() => toDisplay(value))
  const lastNumeric = useRef(value)
  // Resync the display buffer only when the form value changed from outside this input
  // (e.g. form.reset()) — not on every keystroke, or a trailing "," while typing a decimal
  // would get reformatted away before the user can type the digits after it.
  if (value !== lastNumeric.current && parseSerbian(text) !== value) {
    lastNumeric.current = value
    setText(toDisplay(value))
  }
  return (
    <TextField
      fullWidth
      size="small"
      label={label}
      required={required}
      inputMode="decimal"
      value={text}
      onChange={(event) => {
        const raw = event.target.value
        if (raw !== '' && !typingPattern.test(raw)) return
        setText(raw)
        const parsed = parseSerbian(raw)
        lastNumeric.current = parsed
        onChange(parsed)
      }}
      onBlur={onBlur}
      error={error}
      helperText={helperText}
    />
  )
}

/**
 * Serbian-format money input (e.g. "1.234,56") backed by a numeric react-hook-form field.
 * Replaces `type="number"` for money fields: the scroll wheel silently changes a number
 * input's value, and typing a Serbian decimal comma in one produces NaN instead of parsing.
 */
export function MoneyField<TValues extends FieldValues>({ control, name, label, required }: MoneyFieldProps<TValues>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <MoneyInput
          value={field.value as number | undefined}
          onChange={field.onChange}
          onBlur={field.onBlur}
          label={label}
          required={required}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message}
        />
      )}
    />
  )
}
