import { Box } from '@mui/material'
import { alpha } from '@mui/material/styles'

// Product mark: an indigo rounded square with the "SZ" monogram. Sizes are px.
export function BrandMark({ size = 36 }: { size?: number }) {
  return (
    <Box
      aria-hidden="true"
      sx={(theme) => ({
        width: size,
        height: size,
        flexShrink: 0,
        display: 'grid',
        placeItems: 'center',
        borderRadius: `${Math.round(size * 0.3)}px`,
        color: '#fff',
        fontWeight: 750,
        fontSize: size * 0.4,
        letterSpacing: '-0.02em',
        background: `linear-gradient(140deg, ${theme.palette.primary.light} 0%, ${theme.palette.primary.main} 55%, ${theme.palette.primary.dark} 100%)`,
        boxShadow: `inset 0 1px 0 ${alpha('#fff', 0.28)}, 0 4px 12px ${alpha(theme.palette.primary.main, 0.35)}`,
      })}
    >
      SZ
    </Box>
  )
}
