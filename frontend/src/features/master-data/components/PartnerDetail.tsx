import { Button, Card, CardActions, CardContent, Stack, Typography } from '@mui/material'
import type { Partner } from '../types'

interface PartnerDetailProps {
  partner: Partner
  onEdit?: () => void
}

export function PartnerDetail({ partner, onEdit }: PartnerDetailProps) {
  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h5" component="h2">{partner.shortName}</Typography>
        <Typography color="text.secondary">{partner.name}</Typography>
        <Stack component="dl" spacing={1} sx={{ mt: 2 }}>
          <div><Typography component="dt" fontWeight={600}>PIB</Typography><Typography component="dd">{partner.taxNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>Matični broj</Typography><Typography component="dd">{partner.registrationNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>JMBG</Typography><Typography component="dd">{partner.maskedJmbg || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>Broj lične karte</Typography><Typography component="dd">{partner.maskedIdCardNumber || '—'}</Typography></div>
          <div><Typography component="dt" fontWeight={600}>Napomena</Typography><Typography component="dd">{partner.note || '—'}</Typography></div>
        </Stack>
      </CardContent>
      {onEdit && <CardActions><Button onClick={onEdit}>Izmeni</Button></CardActions>}
    </Card>
  )
}

