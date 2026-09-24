import { Card, CardContent, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { useBuildingEntranceDetail } from '../useMasterData'
import { DetailField, DetailPageLayout } from './DetailPageLayout'

export function BuildingEntranceDetailPage() {
  const { t } = useTranslation()
  const { companyId, entranceId } = useParams()
  const companyIdNum = Number(companyId)
  const entranceIdNum = Number(entranceId)
  const validParams =
    Number.isFinite(companyIdNum) && Number.isFinite(entranceIdNum) && companyIdNum > 0 && entranceIdNum > 0

  const detail = useBuildingEntranceDetail(validParams ? companyIdNum : 0, validParams ? entranceIdNum : 0)
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))

  return (
    <DetailPageLayout
      title={t('masterDataDetail_.buildingEntranceTitle')}
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {detail.data && (
        <Card variant="outlined">
          <CardContent>
            <Typography variant="h5" component="h2">
              {detail.data.entranceName ?? detail.data.buildingName ?? `#${detail.data.id}`}
            </Typography>
            <Stack component="dl" spacing={1} sx={{ mt: 2 }}>
              <DetailField label="Zgrada" value={detail.data.buildingName} />
              <DetailField label="Ulaz" value={detail.data.entranceName} />
              <DetailField label="Oznaka zgrade" value={detail.data.buildingLabel} />
              <DetailField label="Adresa (ID)" value={detail.data.addressId} />
              <DetailField label="Opis" value={detail.data.description} />
              <DetailField label="Redni broj" value={detail.data.sortIndex} />
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
