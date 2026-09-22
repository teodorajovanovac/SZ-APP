import { Card, CardContent, Grid, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../../features/auth/useAuth'
import { useActiveCompany } from '../../features/companies/useActiveCompany'

export function DashboardPage() {
  const { t } = useTranslation()
  const { user } = useAuth()
  const { activeCompany } = useActiveCompany()

  return (
    <section>
      <Typography component="h1" variant="h1" gutterBottom>
        {t('welcome')}, {user?.displayName}
      </Typography>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="overline">{t('activeCompany')}</Typography>
              <Typography variant="h2">{activeCompany.name}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardContent>
              <Typography variant="overline">Uloge</Typography>
              <Typography variant="h2">{user?.roles.join(', ')}</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </section>
  )
}
