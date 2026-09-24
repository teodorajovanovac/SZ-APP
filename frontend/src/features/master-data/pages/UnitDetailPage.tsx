import { Card, CardContent, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { useUnitDetail } from '../useMasterData'
import { DetailField, DetailPageLayout } from './DetailPageLayout'

export function UnitDetailPage() {
  const { t } = useTranslation()
  const { companyId, unitId } = useParams()
  const companyIdNum = Number(companyId)
  const unitIdNum = Number(unitId)
  const validParams = Number.isFinite(companyIdNum) && Number.isFinite(unitIdNum) && companyIdNum > 0 && unitIdNum > 0

  const detail = useUnitDetail(validParams ? companyIdNum : 0, validParams ? unitIdNum : 0)
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))

  return (
    <DetailPageLayout
      title={t('masterDataDetail_.unitTitle')}
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {detail.data && (
        <Card variant="outlined">
          <CardContent>
            <Typography variant="h5" component="h2">
              {detail.data.name ?? `#${detail.data.id}`}
            </Typography>
            <Stack component="dl" spacing={1} sx={{ mt: 2 }}>
              <DetailField label="Ulaz (ID)" value={detail.data.buildingEntranceId} />
              <DetailField label="Tip jedinice (ID)" value={detail.data.unitTypeId} />
              <DetailField label="Broj sprata" value={detail.data.floorNumber} />
              <DetailField label="Redni broj" value={detail.data.sortingNumber} />
              <DetailField label="K1" value={detail.data.k1} />
              <DetailField label="K2" value={detail.data.k2} />
              <DetailField label="K3" value={detail.data.k3} />
              <DetailField label="K4" value={detail.data.k4} />
              <DetailField label="K5" value={detail.data.k5} />
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
