import { TextField, type TextFieldProps } from '@mui/material'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'

type FieldKind = 'text' | 'number' | 'date'

interface ControlledTextFieldProps<TValues extends FieldValues> {
  control: Control<TValues>
  name: FieldPath<TValues>
  label: string
  type?: FieldKind
  required?: boolean
  helperText?: string
  multiline?: boolean
  minRows?: number
  inputProps?: TextFieldProps['slotProps']
}

/**
 * One place where a react-hook-form field becomes an MUI TextField.
 *
 * It exists mainly to kill a real visual bug: passing `InputLabelProps.shrink={false}`
 * on a field that already holds a value paints the label on top of the value. Only
 * `date`/`time`-style inputs need a forced shrink (the browser always paints their
 * placeholder), everything else must let MUI decide.
 */
export function ControlledTextField<TValues extends FieldValues>({
  control,
  name,
  label,
  type = 'text',
  required,
  helperText,
  multiline,
  minRows,
  inputProps,
}: ControlledTextFieldProps<TValues>) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <TextField
          {...field}
          value={
            type === 'number' && (field.value === null || Number.isNaN(field.value))
              ? ''
              : (field.value ?? '')
          }
          onChange={(event) =>
            field.onChange(
              type === 'number'
                ? event.target.value === ''
                  ? Number.NaN
                  : Number(event.target.value)
                : event.target.value,
            )
          }
          fullWidth
          size="small"
          type={type}
          label={label}
          required={required}
          multiline={multiline}
          minRows={minRows}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message ?? helperText}
          slotProps={{
            ...inputProps,
            // Never force shrink off — that is what made value and placeholder overlap.
            inputLabel: type === 'date' ? { shrink: true } : undefined,
          }}
        />
      )}
    />
  )
}
