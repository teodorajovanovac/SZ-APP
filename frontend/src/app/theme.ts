import { srRS } from '@mui/material/locale'
import { alpha, createTheme } from '@mui/material/styles'

// srRS covers MUI's own built-in component text (pagination aria-labels, breadcrumbs, …).
// App-level strings still go through i18next/t() — see ServerDataTable for the pagination
// row-count labels, which follow the selected app language instead of this fixed default.

// ── Design tokens ────────────────────────────────────────────────────────────────
// One navy brand ramp + one terracotta accent + reserved status colours. Everything
// visual (surfaces, borders, focus rings, hovers) derives from these, so components
// never need ad-hoc hex values in `sx`.
const navy = {
  50: '#f2f5f8',
  100: '#dde5ec',
  200: '#b6c6d5',
  300: '#89a1b8',
  400: '#557b9c',
  500: '#315b7d',
  600: '#234764',
  700: '#17324d',
  800: '#102538',
  900: '#0a1724',
}

const ink = '#111d29'
const inkMuted = '#5b6b7b'
const borderColor = '#dfe5ec'

// Data-heavy accounting screens: a tight, consistent radius reads calmer than
// pill-shaped everything. 10px on cards, 8px on controls.
const radius = 10

export const theme = createTheme(
  {
    palette: {
      mode: 'light',
      primary: {
        main: navy[700],
        light: navy[500],
        dark: navy[900],
        contrastText: '#ffffff',
      },
      secondary: { main: '#a94f24', light: '#c9713f', dark: '#7f3915', contrastText: '#ffffff' },
      // Status colours are reserved: they mean state (posted / draft / overdue / failed)
      // and are never reused as decoration.
      success: { main: '#1f7a4d', light: '#e6f4ed', dark: '#155c39' },
      warning: { main: '#9a6207', light: '#fdf3e2', dark: '#7a4d05' },
      error: { main: '#b3261e', light: '#fdecea', dark: '#8c1d17' },
      info: { main: navy[500], light: navy[50], dark: navy[700] },
      text: { primary: ink, secondary: inkMuted, disabled: alpha(ink, 0.38) },
      divider: borderColor,
      background: { default: '#f4f6f9', paper: '#ffffff' },
      action: {
        hover: alpha(navy[700], 0.04),
        selected: alpha(navy[700], 0.08),
        focus: alpha(navy[700], 0.12),
      },
    },
    shape: { borderRadius: radius },
    typography: {
      fontFamily: 'Inter, "Segoe UI", Roboto, Arial, sans-serif',
      // Page title (one <h1> per page), section heading, card/tile heading, then body.
      h1: { fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.2, letterSpacing: '-0.02em' },
      h2: { fontSize: '1.3125rem', fontWeight: 700, lineHeight: 1.3, letterSpacing: '-0.01em' },
      h3: { fontSize: '1.0625rem', fontWeight: 700, lineHeight: 1.4 },
      h4: { fontSize: '0.9375rem', fontWeight: 700, lineHeight: 1.45 },
      h5: { fontSize: '0.875rem', fontWeight: 700, lineHeight: 1.45 },
      h6: { fontSize: '1rem', fontWeight: 700, lineHeight: 1.45 },
      subtitle1: { fontSize: '0.9375rem', fontWeight: 600, lineHeight: 1.5 },
      subtitle2: { fontSize: '0.8125rem', fontWeight: 600, lineHeight: 1.5 },
      body1: { fontSize: '0.9375rem', lineHeight: 1.6 },
      body2: { fontSize: '0.8125rem', lineHeight: 1.55 },
      button: { fontSize: '0.875rem', fontWeight: 600, textTransform: 'none', letterSpacing: 0 },
      caption: { fontSize: '0.75rem', lineHeight: 1.45 },
      overline: {
        fontSize: '0.6875rem',
        fontWeight: 700,
        letterSpacing: '0.08em',
        lineHeight: 1.6,
        textTransform: 'uppercase',
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          // A single visible focus treatment across the whole app, keyboard only.
          '*:focus-visible': {
            outline: `2px solid ${navy[500]}`,
            outlineOffset: 2,
          },
          body: { WebkitFontSmoothing: 'antialiased' },
        },
      },
      MuiAppBar: {
        defaultProps: { elevation: 0, color: 'primary' },
        styleOverrides: {
          root: { borderBottom: `1px solid ${navy[900]}` },
        },
      },
      MuiDrawer: {
        styleOverrides: {
          paper: { borderRight: `1px solid ${borderColor}`, backgroundImage: 'none' },
        },
      },
      MuiPaper: {
        styleOverrides: {
          // Surfaces are separated by hairlines and one whisper-soft shadow, not by
          // Material's default stack of drop shadows — the app reads as one dense
          // product rather than a pile of floating cards.
          root: { backgroundImage: 'none' },
          outlined: { borderColor },
          elevation1: {
            border: `1px solid ${borderColor}`,
            boxShadow: `0 1px 2px ${alpha(navy[900], 0.04)}`,
          },
        },
      },
      MuiCard: {
        defaultProps: { variant: 'outlined' },
        styleOverrides: { root: { borderRadius: radius } },
      },
      MuiCardContent: {
        styleOverrides: {
          root: { padding: 20, '&:last-child': { paddingBottom: 20 } },
        },
      },
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: { borderRadius: 8, paddingInline: 16 },
          sizeLarge: { paddingBlock: 10 },
        },
      },
      MuiIconButton: { styleOverrides: { root: { borderRadius: 8 } } },
      MuiTextField: { defaultProps: { size: 'small' } },
      MuiOutlinedInput: { styleOverrides: { root: { borderRadius: 8 } } },
      MuiChip: {
        styleOverrides: {
          root: { borderRadius: 6, fontWeight: 600, fontSize: '0.75rem' },
          sizeSmall: { height: 22 },
        },
      },
      MuiListItemButton: {
        styleOverrides: {
          root: {
            borderRadius: 8,
            '&.Mui-selected': { backgroundColor: alpha(navy[700], 0.09) },
          },
        },
      },
      MuiListItemIcon: { styleOverrides: { root: { minWidth: 38, color: inkMuted } } },
      MuiListSubheader: {
        styleOverrides: {
          root: {
            backgroundColor: 'transparent',
            color: inkMuted,
            fontSize: '0.6875rem',
            fontWeight: 700,
            letterSpacing: '0.08em',
            textTransform: 'uppercase',
            lineHeight: 2.4,
          },
        },
      },
      MuiDivider: { styleOverrides: { root: { borderColor } } },
      MuiTableCell: {
        styleOverrides: {
          root: { borderColor, paddingBlock: 10 },
          head: { fontWeight: 700, color: ink, backgroundColor: navy[50] },
        },
      },
      MuiTableRow: {
        styleOverrides: {
          root: { '&:last-child td': { borderBottom: 0 } },
        },
      },
      MuiAlert: {
        styleOverrides: { root: { borderRadius: 8, alignItems: 'center' } },
      },
      MuiSkeleton: { defaultProps: { animation: 'wave' } },
      MuiTooltip: {
        defaultProps: { arrow: true },
      },
      MuiLink: { defaultProps: { underline: 'hover' } },
    },
  },
  srRS,
)
