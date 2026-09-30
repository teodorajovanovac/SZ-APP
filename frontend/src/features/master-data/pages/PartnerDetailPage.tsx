import { Button, Card, CardContent } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { usePartnerDetail } from '../useMasterData'
import { DetailField, DetailGrid, DetailPageLayout } from './DetailPageLayout'

export function PartnerDetailPage() {
  const { t } = useTranslation()
  const { companyId, partnerId } = useParams()
  const companyIdNum = Number(companyId)
  const partnerIdNum = Number(partnerId)
  const validParams = Number.isFinite(companyIdNum) && Number.isFinite(partnerIdNum) && companyIdNum > 0 && partnerIdNum > 0

  const detail = usePartnerDetail(validParams ? companyIdNum : 0, validParams ? partnerIdNum : 0)
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))
  const data = detail.data

  return (
    <DetailPageLayout
      title={data?.shortName ?? t('masterDataDetail_.partnerTitle')}
      subtitle={data?.name}
      actions={
        data ? (
          <Button component={RouterLink} to={`/kartice?companyId=${companyIdNum}&partnerId=${partnerIdNum}&account=2040`} variant="outlined">
            {t('cards_.tabCard')}
          </Button>
        ) : null
      }
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {data && (
        <Card variant="outlined">
          <CardContent>
            <DetailGrid>
              <DetailField label={t('fields.taxNumber')} value={data.taxNumber} />
              <DetailField label={t('fields.registrationNumber')} value={data.registrationNumber} />
              <DetailField label={t('fields.jbkjs')} value={data.jbkjs} />
              <DetailField label={t('fields.jmbg')} value={data.maskedJmbg} />
              <DetailField label={t('fields.idCardNumber')} value={data.maskedIdCardNumber} />
              <DetailField label={t('fields.language')} value={data.language} />
              <DetailField label={t('fields.note')} value={data.note} />
            </DetailGrid>
          </CardContent>
        </Card>
      )}
    </DetailPageLayout>
  )
}

function isNotFoundStatus(status?: number): boolean {
  return status === 404 || status === 403
}
