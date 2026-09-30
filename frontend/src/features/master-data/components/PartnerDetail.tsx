import { useTranslation } from 'react-i18next'
import { Button, Card, CardActions, CardContent, Stack, Typography } from '@mui/material'
import type { Partner } from '../types'

interface PartnerDetailProps {
  partner: Partner
  onEdit?: () => void
}

export function PartnerDetail({ partner, onEdit }: PartnerDetailProps) {
  const { t } = useTranslation()
  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h5" component="h2">{partner.shortName}</Typography>
        <Typography color="text.secondary">{partner.name}</Typography>
        <Stack component="dl" spacing={1} sx={{ mt: 2 }}>
          <div><Typography component="dt" fontWeight={600}>{t('fields.taxNumber')}</Typography><Typography component="dd">{partner.taxNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>{t('fields.registrationNumber')}</Typography><Typography component="dd">{partner.registrationNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>{t('fields.jmbg')}</Typography><Typography component="dd">{partner.maskedJmbg || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>{t('fields.idCardNumber')}</Typography><Typography component="dd">{partner.maskedIdCardNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>{t('fields.note')}</Typography><Typography component="dd">{partner.note || '—'}</Typography></div>
        </Stack>
      </CardContent>
      {onEdit && <CardActions><Button onClick={onEdit}>{t('common.edit')}</Button></CardActions>}
    </Card>
  )
}

