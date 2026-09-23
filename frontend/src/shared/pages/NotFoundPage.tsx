import SearchOffIcon from '@mui/icons-material/SearchOff'
import { Button, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

export function NotFoundPage() {
  const { t } = useTranslation()

  return (
    <Stack
      component="main"
      minHeight="100vh"
      alignItems="center"
      justifyContent="center"
      spacing={2}
      textAlign="center"
      sx={{ p: 3, bgcolor: 'background.default' }}
    >
      <SearchOffIcon sx={{ fontSize: 56, color: 'text.disabled' }} aria-hidden="true" />
      <Typography component="h1" variant="h1">{t('notFound.title')}</Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 420 }}>{t('notFound.description')}</Typography>
      <Button component={Link} to="/" variant="contained">{t('notFound.back')}</Button>
    </Stack>
  )
}
