import { srRS } from '@mui/material/locale'
import { createTheme } from '@mui/material/styles'

// srRS covers MUI's own built-in component text (pagination aria-labels, breadcrumbs, …).
// App-level strings still go through i18next/t() — see ServerDataTable for the pagination
// row-count labels, which follow the selected app language instead of this fixed default.
export const theme = createTheme(
  {
    palette: {
      mode: 'light',
      primary: { main: '#17324d' },
      secondary: { main: '#a94f24' },
      background: { default: '#f4f6f8' },
    },
    shape: { borderRadius: 10 },
    typography: {
      fontFamily: 'Inter, "Segoe UI", Roboto, Arial, sans-serif',
      h1: { fontSize: '2rem', fontWeight: 700 },
      h2: { fontSize: '1.5rem', fontWeight: 700 },
    },
    components: {
      MuiButton: { defaultProps: { disableElevation: true } },
      MuiTextField: { defaultProps: { size: 'small' } },
      MuiCard: { defaultProps: { variant: 'outlined' } },
    },
  },
  srRS,
)
