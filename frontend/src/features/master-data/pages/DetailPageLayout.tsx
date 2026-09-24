import type { ReactNode } from 'react'
import { Alert, CircularProgress, Link, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink } from 'react-router-dom'

interface DetailPageLayoutProps {
  title: string
  isLoading: boolean
  isError: boolean
  isNotFound: boolean
  children: ReactNode
}

/** Shared chrome (back link, loading/error/not-found states) for the entity detail pages. */
export function DetailPageLayout({ title, isLoading, isError, isNotFound, children }: DetailPageLayoutProps) {
  const { t } = useTranslation()
  return (
    <Stack spacing={2}>
      <Link component={RouterLink} to="/contracts" underline="hover">
        {t('masterDataDetail_.back')}
      </Link>
      <Typography component="h1" variant="h1">
        {title}
      </Typography>
      {isLoading && <CircularProgress size={24} aria-label={t('masterDataDetail_.loading')} />}
      {isNotFound && <Alert severity="warning">{t('masterDataDetail_.notFound')}</Alert>}
      {isError && !isNotFound && <Alert severity="error">{t('masterDataDetail_.error')}</Alert>}
      {!isLoading && !isError && !isNotFound && children}
    </Stack>
  )
}

export function DetailField({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div>
      <Typography component="dt" fontWeight={600}>
        {label}
      </Typography>
      <Typography component="dd">{value ?? '—'}</Typography>
    </div>
  )
}
