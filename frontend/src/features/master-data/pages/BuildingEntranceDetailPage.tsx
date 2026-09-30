import { Card, CardContent } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { useBuildingEntranceDetail } from '../useMasterData'
import { DetailField, DetailGrid, DetailPageLayout } from './DetailPageLayout'

export function BuildingEntranceDetailPage() {
  const { t } = useTranslation()
  const { companyId, entranceId } = useParams()
  const companyIdNum = Number(companyId)
  const entranceIdNum = Number(entranceId)
  const validParams =
    Number.isFinite(companyIdNum) && Number.isFinite(entranceIdNum) && companyIdNum > 0 && entranceIdNum > 0

  const detail = useBuildingEntranceDetail(validParams ? companyIdNum : 0, validParams ? entranceIdNum : 0)
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))
  const data = detail.data

  return (
    <DetailPageLayout
      title={data ? [data.buildingName, data.entranceName].filter(Boolean).join(' · ') || `#${data.id}` : t('masterDataDetail_.buildingEntranceTitle')}
      subtitle={data?.buildingLabel}
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {data && (
        <Card variant="outlined">
          <CardContent>
            <DetailGrid>
              <DetailField label={t('fields.building')} value={data.buildingName} />
              <DetailField label={t('fields.entrance')} value={data.entranceName} />
              <DetailField label={t('fields.buildingLabel')} value={data.buildingLabel} />
              {/* ponytail: the API has no address lookup by id for non-Root users; shows the id until it does. */}
              <DetailField label={t('fields.address')} value={data.addressId ? `#${data.addressId}` : null} />
              <DetailField label={t('fields.description')} value={data.description} />
              <DetailField label={t('fields.ordinal')} value={data.sortIndex} />
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
