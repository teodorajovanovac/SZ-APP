import { Card, CardContent, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { usePartnerDetail } from '../useMasterData'
import { DetailField, DetailPageLayout } from './DetailPageLayout'

export function PartnerDetailPage() {
  const { t } = useTranslation()
  const { companyId, partnerId } = useParams()
  const companyIdNum = Number(companyId)
  const partnerIdNum = Number(partnerId)
  const validParams = Number.isFinite(companyIdNum) && Number.isFinite(partnerIdNum) && companyIdNum > 0 && partnerIdNum > 0

  const detail = usePartnerDetail(validParams ? companyIdNum : 0, validParams ? partnerIdNum : 0)
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))

  return (
    <DetailPageLayout
      title={t('masterDataDetail_.partnerTitle')}
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {detail.data && (
        <Card variant="outlined">
          <CardContent>
            <Typography variant="h5" component="h2">
              {detail.data.shortName}
            </Typography>
            <Typography color="text.secondary">{detail.data.name}</Typography>
            <Stack component="dl" spacing={1} sx={{ mt: 2 }}>
              <DetailField label="PIB" value={detail.data.taxNumber} />
              <DetailField label="Matični broj" value={detail.data.registrationNumber} />
              <DetailField label="JBKJS" value={detail.data.jbkjs} />
              <DetailField label="JMBG" value={detail.data.maskedJmbg} />
              <DetailField label="Broj lične karte" value={detail.data.maskedIdCardNumber} />
              <DetailField label="Jezik" value={detail.data.language} />
              <DetailField label="Napomena" value={detail.data.note} />
            </Stack>
          </CardContent>
        </Card>
      )}
    </DetailPageLayout>
  )
}

function isNotFoundStatus(status?: number): boolean {
  return status === 404 || status === 403
}
