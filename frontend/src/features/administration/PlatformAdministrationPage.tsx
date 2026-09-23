import { useQuery } from '@tanstack/react-query'
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined'
import TranslateOutlinedIcon from '@mui/icons-material/TranslateOutlined'
import {
  Alert,
  Box,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemText,
  Paper,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'

interface SettingItem { id: number; name: string; key: string; value?: string | null; category?: string | null }
interface LanguageItem { code: string; name?: string | null; isActive: boolean; isDefault: boolean }

export function PlatformAdministrationPage({ companyId }: { companyId: number }) {
  const { t } = useTranslation()
  const settings = useQuery({
    queryKey: ['platform', companyId, 'settings'],
    queryFn: () => apiRequest<SettingItem[]>(`/api/v1/companies/${companyId}/settings`),
  })
  const languages = useQuery({
    queryKey: ['platform', 'languages'],
    queryFn: () => apiRequest<LanguageItem[]>(`/api/v1/companies/${companyId}/languages`),
  })

  const settingItems = settings.data ?? []
  const languageItems = languages.data ?? []

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('administration_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>{t('administration_.subtitle')}</Typography>
      </Box>

      {settings.isError || languages.isError ? (
        <Alert severity="error">{t('administration_.loadFailed')}</Alert>
      ) : null}

      <Paper variant="outlined" component="section">
        <Stack direction="row" spacing={1} alignItems="center" sx={{ p: 2 }}>
          <SettingsOutlinedIcon fontSize="small" aria-hidden="true" />
          <Typography component="h2" variant="h6">{t('administration_.settingsTitle')}</Typography>
        </Stack>
        <Divider />
        {settings.isLoading ? (
          <Box sx={{ p: 2 }}>{[0, 1, 2].map((row) => <Skeleton key={row} height={40} />)}</Box>
        ) : settingItems.length === 0 ? (
          <Stack spacing={1} alignItems="center" sx={{ px: 3, py: 5, textAlign: 'center' }}>
            <Typography component="p" variant="subtitle1">{t('administration_.settingsEmptyTitle')}</Typography>
            <Typography color="text.secondary" sx={{ maxWidth: 440 }}>{t('administration_.settingsEmptyBody')}</Typography>
          </Stack>
        ) : (
          <List disablePadding>
            {settingItems.map((item) => (
              <ListItem key={item.id} divider>
                <ListItemText
                  primary={item.name}
                  secondary={`${item.key}: ${item.value ?? t('administration_.noValue')}`}
                />
                {item.category ? <Chip size="small" variant="outlined" label={item.category} /> : null}
              </ListItem>
            ))}
          </List>
        )}
      </Paper>

      <Paper variant="outlined" component="section">
        <Stack direction="row" spacing={1} alignItems="center" sx={{ p: 2 }}>
          <TranslateOutlinedIcon fontSize="small" aria-hidden="true" />
          <Typography component="h2" variant="h6">{t('administration_.languagesTitle')}</Typography>
        </Stack>
        <Divider />
        {languages.isLoading ? (
          <Box sx={{ p: 2 }}>{[0, 1].map((row) => <Skeleton key={row} height={40} />)}</Box>
        ) : languageItems.length === 0 ? (
          <Stack spacing={1} alignItems="center" sx={{ px: 3, py: 5, textAlign: 'center' }}>
            <Typography component="p" variant="subtitle1">{t('administration_.languagesEmptyTitle')}</Typography>
            <Typography color="text.secondary" sx={{ maxWidth: 440 }}>{t('administration_.languagesEmptyBody')}</Typography>
          </Stack>
        ) : (
          <List disablePadding>
            {languageItems.map((item) => (
              <ListItem key={item.code} divider>
                <ListItemText primary={item.name ?? item.code} secondary={item.code} />
                <Stack direction="row" spacing={1}>
                  {item.isDefault ? <Chip size="small" color="primary" label={t('administration_.default')} /> : null}
                  {!item.isActive ? <Chip size="small" variant="outlined" label={t('administration_.inactive')} /> : null}
                </Stack>
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Stack>
  )
}
