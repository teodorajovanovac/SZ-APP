import SearchIcon from '@mui/icons-material/Search'
import { InputAdornment, TextField, type SxProps, type Theme } from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { useDebouncedValue } from '../hooks/useDebouncedValue'

interface SearchFieldProps {
  value: string
  onChange: (value: string) => void
  label: string
  sx?: SxProps<Theme>
  delayMs?: number
}

/**
 * PERF-06: search box that reports its value only once typing pauses. It is a
 * `type="search"` input, which is what the `/` shortcut focuses.
 */
export function SearchField({ value, onChange, label, sx, delayMs = 300 }: SearchFieldProps) {
  const [text, setText] = useState(value)
  const debounced = useDebouncedValue(text, delayMs)
  const latest = useRef({ value, onChange })
  useEffect(() => {
    latest.current = { value, onChange }
  })
  useEffect(() => {
    if (debounced !== latest.current.value) latest.current.onChange(debounced)
  }, [debounced])
  // Follow outside changes (Back navigation, "clear filters").
  useEffect(() => {
    setText(value)
  }, [value])

  return (
    <TextField
      type="search"
      label={label}
      value={text}
      onChange={(event) => setText(event.target.value)}
      size="small"
      sx={[{ minWidth: { sm: 320 }, flex: { xs: '1 1 100%', sm: '0 1 420px' } }, ...(Array.isArray(sx) ? sx : [sx])]}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon fontSize="small" />
            </InputAdornment>
          ),
        },
      }}
    />
  )
}
