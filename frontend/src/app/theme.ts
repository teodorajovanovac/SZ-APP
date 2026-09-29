import { srRS } from '@mui/material/locale'
import { alpha, createTheme } from '@mui/material/styles'

// srRS covers MUI's own built-in component text (pagination aria-labels, breadcrumbs, …).
// App-level strings still go through i18next/t() — see ServerDataTable for the pagination
// row-count labels, which follow the selected app language instead of this fixed default.

// ── Design tokens ────────────────────────────────────────────────────────────────
// One indigo brand ramp on cool slate neutrals, plus reserved status colours. Surfaces
// are white on a barely-tinted canvas, separated by hairlines rather than shadows.
// Everything visual (borders, focus rings, hovers) derives from these tokens, so
// components never need ad-hoc hex values in `sx`.
export const brand = {
  50: '#f1f3ff',
  100: '#e3e7ff',
  200: '#c7cffd',
  300: '#a3b0fa',
  400: '#7a8cf3',
  500: '#5468e8',
  600: '#3f51d4',
  700: '#3241b0',
  800: '#28358a',
  900: '#1c2565',
}

const ink = '#0f172a'
const inkMuted = '#64748b'
const borderColor = '#e5e8ee'
const borderStrong = '#d3d9e3'
const canvas = '#f6f7fb'

// Data-heavy accounting screens: 14px on cards and dialogs, 10px on controls.
const radius = 10
const cardRadius = 14

const focusRing = (color: string) => `0 0 0 3px ${alpha(color, 0.18)}`

export const theme = createTheme(
  {
    palette: {
      mode: 'light',
      primary: {
        main: brand[600],
        light: brand[400],
        dark: brand[800],
        contrastText: '#ffffff',
      },
      secondary: { main: '#475569', light: '#94a3b8', dark: '#1e293b', contrastText: '#ffffff' },
      // Status colours are reserved: they mean state (posted / draft / overdue / failed)
      // and are never reused as decoration.
      success: { main: '#15803d', light: '#e8f6ee', dark: '#116632' },
      warning: { main: '#b45309', light: '#fef3e2', dark: '#92400e' },
      error: { main: '#c0362c', light: '#fdecea', dark: '#9a2a22' },
      info: { main: brand[600], light: brand[50], dark: brand[800] },
      text: { primary: ink, secondary: inkMuted, disabled: alpha(ink, 0.38) },
      divider: borderColor,
      background: { default: canvas, paper: '#ffffff' },
      action: {
        hover: alpha(ink, 0.04),
        selected: alpha(brand[600], 0.09),
        focus: alpha(brand[600], 0.12),
      },
    },
    shape: { borderRadius: radius },
    typography: {
      fontFamily: '"Inter Variable", Inter, ui-sans-serif, system-ui, "Segoe UI", Roboto, Arial, sans-serif',
      // Page title (one <h1> per page), section heading, card/tile heading, then body.
      h1: { fontSize: '1.625rem', fontWeight: 650, lineHeight: 1.2, letterSpacing: '-0.025em' },
      h2: { fontSize: '1.25rem', fontWeight: 650, lineHeight: 1.3, letterSpacing: '-0.015em' },
      h3: { fontSize: '1.0625rem', fontWeight: 650, lineHeight: 1.4, letterSpacing: '-0.01em' },
      h4: { fontSize: '0.9375rem', fontWeight: 650, lineHeight: 1.45 },
      h5: { fontSize: '0.875rem', fontWeight: 650, lineHeight: 1.45 },
      h6: { fontSize: '1rem', fontWeight: 650, lineHeight: 1.45 },
      subtitle1: { fontSize: '0.9375rem', fontWeight: 600, lineHeight: 1.5 },
      subtitle2: { fontSize: '0.8125rem', fontWeight: 600, lineHeight: 1.5 },
      body1: { fontSize: '0.9375rem', lineHeight: 1.6 },
      body2: { fontSize: '0.8125rem', lineHeight: 1.55 },
      button: { fontSize: '0.875rem', fontWeight: 600, textTransform: 'none', letterSpacing: 0 },
      caption: { fontSize: '0.75rem', lineHeight: 1.45 },
      overline: {
        fontSize: '0.6875rem',
        fontWeight: 650,
        letterSpacing: '0.07em',
        lineHeight: 1.6,
        textTransform: 'uppercase',
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          // A single visible focus treatment across the whole app, keyboard only.
          '*:focus-visible': {
            outline: `2px solid ${brand[500]}`,
            outlineOffset: 2,
          },
          body: { WebkitFontSmoothing: 'antialiased', MozOsxFontSmoothing: 'grayscale', fontFeatureSettings: '"cv11", "ss01"' },
          '::selection': { backgroundColor: alpha(brand[500], 0.22) },
          '@media print': { '.no-print': { display: 'none !important' } },
        },
      },
      MuiAppBar: {
        defaultProps: { elevation: 0, color: 'inherit' },
        styleOverrides: {
          root: {
            backgroundColor: alpha('#ffffff', 0.86),
            backdropFilter: 'saturate(180%) blur(12px)',
            color: ink,
            borderBottom: `1px solid ${borderColor}`,
          },
        },
      },
      MuiDrawer: {
        styleOverrides: {
          paper: { borderRight: `1px solid ${borderColor}`, backgroundImage: 'none', backgroundColor: '#ffffff' },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: { backgroundImage: 'none' },
          outlined: { borderColor },
          rounded: { borderRadius: cardRadius },
          elevation1: {
            border: `1px solid ${borderColor}`,
            boxShadow: `0 1px 2px ${alpha(ink, 0.04)}`,
          },
        },
      },
      MuiCard: {
        defaultProps: { variant: 'outlined' },
        styleOverrides: { root: { borderRadius: cardRadius } },
      },
      MuiCardContent: {
        styleOverrides: {
          root: { padding: 20, '&:last-child': { paddingBottom: 20 } },
        },
      },
      MuiDialog: {
        styleOverrides: {
          paper: { borderRadius: 16, boxShadow: `0 24px 64px ${alpha(ink, 0.18)}` },
        },
      },
      MuiDialogTitle: { styleOverrides: { root: { fontWeight: 650, letterSpacing: '-0.01em' } } },
      MuiBackdrop: { styleOverrides: { root: { backgroundColor: alpha('#0b1020', 0.42) } } },
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: { borderRadius: radius, paddingInline: 16, transition: 'background-color .15s, border-color .15s, box-shadow .15s' },
          sizeLarge: { paddingBlock: 11 },
          containedPrimary: {
            boxShadow: `inset 0 1px 0 ${alpha('#fff', 0.14)}, 0 1px 2px ${alpha(brand[900], 0.25)}`,
            '&:hover': { backgroundColor: brand[700] },
          },
          outlined: { borderColor: borderStrong, color: ink, '&:hover': { borderColor: inkMuted, backgroundColor: alpha(ink, 0.03) } },
          outlinedPrimary: { color: brand[700], borderColor: alpha(brand[600], 0.4), '&:hover': { borderColor: brand[600], backgroundColor: alpha(brand[600], 0.06) } },
          text: { '&:hover': { backgroundColor: alpha(ink, 0.05) } },
        },
      },
      MuiIconButton: { styleOverrides: { root: { borderRadius: radius } } },
      MuiTextField: { defaultProps: { size: 'small' } },
      MuiInputLabel: { styleOverrides: { root: { fontSize: '0.875rem' } } },
      MuiOutlinedInput: {
        styleOverrides: {
          root: {
            borderRadius: radius,
            backgroundColor: '#ffffff',
            transition: 'box-shadow .15s',
            '& .MuiOutlinedInput-notchedOutline': { borderColor: borderStrong },
            '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: inkMuted },
            '&.Mui-focused': { boxShadow: focusRing(brand[600]) },
            '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: brand[600], borderWidth: 1 },
            '&.Mui-error.Mui-focused': { boxShadow: focusRing('#c0362c') },
            '&.Mui-disabled': { backgroundColor: alpha(ink, 0.03) },
          },
        },
      },
      MuiChip: {
        styleOverrides: {
          root: { borderRadius: 999, fontWeight: 600, fontSize: '0.75rem' },
          sizeSmall: { height: 22 },
        },
      },
      MuiTabs: {
        styleOverrides: {
          root: { minHeight: 44, borderBottom: `1px solid ${borderColor}` },
          indicator: { height: 3, borderRadius: '3px 3px 0 0' },
        },
      },
      MuiTab: { styleOverrides: { root: { minHeight: 44, textTransform: 'none', fontWeight: 600, fontSize: '0.875rem' } } },
      MuiToggleButtonGroup: {
        styleOverrides: {
          root: {
            backgroundColor: alpha(ink, 0.05),
            padding: 3,
            gap: 2,
            borderRadius: radius,
            '& .MuiToggleButtonGroup-grouped': {
              border: 0,
              borderRadius: `${radius - 3}px !important`,
              '&:not(:first-of-type)': { marginLeft: 0 },
            },
          },
        },
      },
      MuiToggleButton: {
        styleOverrides: {
          root: {
            textTransform: 'none',
            fontWeight: 600,
            fontSize: '0.8125rem',
            color: inkMuted,
            paddingBlock: 5,
            '&.Mui-selected': {
              color: ink,
              backgroundColor: '#ffffff',
              boxShadow: `0 1px 2px ${alpha(ink, 0.12)}`,
              '&:hover': { backgroundColor: '#ffffff' },
            },
          },
        },
      },
      MuiListItemButton: {
        styleOverrides: {
          root: {
            borderRadius: radius,
            '&.Mui-selected': { backgroundColor: alpha(brand[600], 0.09) },
          },
        },
      },
      MuiListItemIcon: { styleOverrides: { root: { minWidth: 36, color: inkMuted } } },
      MuiListSubheader: {
        styleOverrides: {
          root: {
            backgroundColor: 'transparent',
            color: alpha(inkMuted, 0.85),
            fontSize: '0.6875rem',
            fontWeight: 650,
            letterSpacing: '0.07em',
            textTransform: 'uppercase',
            lineHeight: 2.4,
          },
        },
      },
      MuiDivider: { styleOverrides: { root: { borderColor } } },
      MuiTableCell: {
        styleOverrides: {
          root: { borderColor, paddingBlock: 11 },
          head: {
            fontWeight: 650,
            fontSize: '0.6875rem',
            letterSpacing: '0.06em',
            textTransform: 'uppercase',
            color: inkMuted,
            backgroundColor: '#fafbfd',
          },
        },
      },
      MuiTableRow: {
        styleOverrides: {
          root: { '&:last-child td': { borderBottom: 0 }, '&.MuiTableRow-hover:hover': { backgroundColor: alpha(brand[600], 0.035) } },
        },
      },
      MuiAlert: {
        styleOverrides: { root: { borderRadius: radius, alignItems: 'center' } },
      },
      MuiMenu: { styleOverrides: { paper: { borderRadius: 12, border: `1px solid ${borderColor}`, boxShadow: `0 12px 32px ${alpha(ink, 0.12)}` } } },
      MuiPopover: { styleOverrides: { paper: { borderRadius: 12 } } },
      MuiSkeleton: { defaultProps: { animation: 'wave' } },
      MuiTooltip: {
        defaultProps: { arrow: true },
        styleOverrides: {
          tooltip: { backgroundColor: '#1e293b', fontSize: '0.75rem', fontWeight: 500, borderRadius: 8, padding: '6px 10px' },
          arrow: { color: '#1e293b' },
        },
      },
      MuiLink: { defaultProps: { underline: 'hover' } },
    },
  },
  srRS,
)
