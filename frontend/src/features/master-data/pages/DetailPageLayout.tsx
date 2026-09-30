import type { ReactNode } from 'react'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { Alert, Box, Button, CircularProgress, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { useDocumentTitle } from '../../../shared/hooks/useDocumentTitle'

interface DetailPageLayoutProps {
  title: string
  subtitle?: ReactNode
  actions?: ReactNode
  isLoading: boolean
  isError: boolean
  isNotFound: boolean
  children: ReactNode
}

/** Shared chrome (back, title bar, loading/error/not-found states) for the entity detail pages. */
export function DetailPageLayout({ title, subtitle, actions, isLoading, isError, isNotFound, children }: DetailPageLayoutProps) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  useDocumentTitle(title)
  // UX-24: back to wherever the user came from (list with its URL filters), not always /contracts.
  const back = () => (window.history.length > 1 ? navigate(-1) : navigate('/contracts'))
  return (
    <Stack spacing={2}>
      <Box>
        <Button size="small" startIcon={<ArrowBackIcon />} onClick={back} sx={{ ml: -1 }}>
          {t('ui.back')}
        </Button>
      </Box>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="space-between" alignItems={{ sm: 'center' }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography component="h1" variant="h1">{title}</Typography>
          {subtitle ? <Typography color="text.secondary" sx={{ mt: 0.5 }}>{subtitle}</Typography> : null}
        </Box>
        {actions ? <Stack direction="row" spacing={1}>{actions}</Stack> : null}
      </Stack>
      {isLoading && <CircularProgress size={24} aria-label={t('masterDataDetail_.loading')} />}
      {isNotFound && <Alert severity="warning">{t('masterDataDetail_.notFound')}</Alert>}
      {isError && !isNotFound && <Alert severity="error">{t('masterDataDetail_.error')}</Alert>}
      {!isLoading && !isError && !isNotFound && children}
    </Stack>
  )
}

export function DetailField({ label, value }: { label: string; value: ReactNode }) {
  return (
    <Box>
      <Typography component="dt" variant="caption" color="text.secondary" fontWeight={600}>
        {label}
      </Typography>
      <Typography component="dd" sx={{ m: 0 }}>{value ?? '—'}</Typography>
    </Box>
  )
}

/** Two/three-column definition list for detail cards. */
export function DetailGrid({ children }: { children: ReactNode }) {
  return (
    <Box component="dl" sx={{ m: 0, display: 'grid', gap: 2, gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: 'repeat(3, 1fr)' } }}>
      {children}
    </Box>
  )
}
