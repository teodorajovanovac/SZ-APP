import ConstructionIcon from '@mui/icons-material/Construction'
import { Paper, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useActiveCompany } from '../../features/companies/useActiveCompany'

interface FeaturePlaceholderProps {
  titleKey: string
}

export function FeaturePlaceholder({ titleKey }: FeaturePlaceholderProps) {
  const { t } = useTranslation()
  const { activeCompany } = useActiveCompany()

  return (
    <Paper component="section" sx={{ p: { xs: 3, md: 5 } }}>
      <Stack spacing={2} alignItems="flex-start">
        <ConstructionIcon color="secondary" fontSize="large" aria-hidden="true" />
        <Typography component="h1" variant="h1">{t(titleKey)}</Typography>
        <Typography color="text.secondary">{activeCompany.name}</Typography>
        <Typography>{t('comingSoon')}</Typography>
      </Stack>
    </Paper>
  )
}
