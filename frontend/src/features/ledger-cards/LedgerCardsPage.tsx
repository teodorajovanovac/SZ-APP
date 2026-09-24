import ConstructionIcon from '@mui/icons-material/Construction'
import { Paper, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'

// Placeholder drill-through target for /kartice?companyId=&accountNumber=&partnerAccountId=
// linked from the Ugovori search page. The real konto kartica report is out of scope here.
const PARAM_NAMES = ['companyId', 'accountNumber', 'partnerAccountId'] as const

export function LedgerCardsPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const params = PARAM_NAMES.map((name) => [name, searchParams.get(name)] as const)
  const hasParams = params.some(([, value]) => value !== null)

  return (
    <Stack spacing={2}>
      <Typography variant="h4">{t('kartice.title')}</Typography>
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1.5} alignItems="flex-start">
          <ConstructionIcon sx={{ fontSize: 40, color: 'text.disabled' }} aria-hidden="true" />
          <Typography variant="h6">{t('kartice.inProgress')}</Typography>
          <Typography color="text.secondary">{t('kartice.description')}</Typography>
          <Typography variant="subtitle2" sx={{ mt: 1 }}>{t('kartice.params')}</Typography>
          {hasParams ? (
            <Stack component="dl" spacing={0.5} sx={{ m: 0 }}>
              {params.map(([name, value]) => (
                <Stack key={name} direction="row" spacing={1}>
                  <Typography component="dt" variant="body2" fontWeight={600}>{name}:</Typography>
                  <Typography component="dd" variant="body2" sx={{ m: 0 }}>{value ?? '—'}</Typography>
                </Stack>
              ))}
            </Stack>
          ) : (
            <Typography variant="body2" color="text.secondary">{t('kartice.noParams')}</Typography>
          )}
        </Stack>
      </Paper>
    </Stack>
  )
}
