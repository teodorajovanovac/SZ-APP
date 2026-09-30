import { Card, CardContent, Link, Tab, Tabs } from '@mui/material'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { ApiProblemError } from '../../../api/generated/client'
import { useShortList } from '../useShortList'
import { useBuildingEntranceDetail, useUnitDetail } from '../useMasterData'
import { DetailField, DetailGrid, DetailPageLayout } from './DetailPageLayout'
import { ContractChangeSection } from './ContractChangeSection'

export function UnitDetailPage() {
  const { t } = useTranslation()
  const { companyId, unitId } = useParams()
  const [tab, setTab] = useState(0)
  const companyIdNum = Number(companyId)
  const unitIdNum = Number(unitId)
  const validParams = Number.isFinite(companyIdNum) && Number.isFinite(unitIdNum) && companyIdNum > 0 && unitIdNum > 0
  const cid = validParams ? companyIdNum : 0

  const detail = useUnitDetail(cid, validParams ? unitIdNum : 0)
  const data = detail.data
  // UX-24: names instead of raw ids.
  const entrance = useBuildingEntranceDetail(cid, data?.buildingEntranceId ?? 0)
  const unitTypes = useShortList(data?.unitTypeId ? cid : 0, 'UnitType')
  const isNotFound = !validParams || (detail.error instanceof ApiProblemError && isNotFoundStatus(detail.error.problem.status))
  const entranceName = entrance.data ? [entrance.data.buildingName, entrance.data.entranceName].filter(Boolean).join(' · ') : null
  const unitType = unitTypes.data?.find((item) => item.id === data?.unitTypeId)?.caption

  return (
    <DetailPageLayout
      title={data ? data.name ?? `#${data.id}` : t('masterDataDetail_.unitTitle')}
      subtitle={unitType}
      isLoading={validParams && detail.isLoading}
      isError={detail.isError}
      isNotFound={isNotFound}
    >
      {data && (
        <Card variant="outlined">
          <Tabs value={tab} onChange={(_, value: number) => setTab(value)} sx={{ px: 2, borderBottom: 1, borderColor: 'divider' }}>
            <Tab label={t('ui.tabs.details')} />
            <Tab label={t('ui.tabs.contracts')} />
          </Tabs>
          <CardContent>
            {tab === 0 ? (
              <DetailGrid>
                <DetailField
                  label={t('fields.entrance')}
                  value={data.buildingEntranceId ? (
                    <Link component={RouterLink} to={`/building-entrances/${cid}/${data.buildingEntranceId}`}>
                      {entranceName || `#${data.buildingEntranceId}`}
                    </Link>
                  ) : null}
                />
                <DetailField label={t('fields.unitType')} value={unitType ?? (data.unitTypeId ? `#${data.unitTypeId}` : null)} />
                <DetailField label={t('fields.floor')} value={data.floorNumber} />
                <DetailField label={t('fields.ordinal')} value={data.sortingNumber} />
                <DetailField label="K1" value={data.k1} />
                <DetailField label="K2" value={data.k2} />
                <DetailField label="K3" value={data.k3} />
                <DetailField label="K4" value={data.k4} />
                <DetailField label="K5" value={data.k5} />
                <DetailField label={t('fields.note')} value={data.note} />
              </DetailGrid>
            ) : (
              <ContractChangeSection companyId={cid} unitId={unitIdNum} />
            )}
          </CardContent>
        </Card>
      )}
    </DetailPageLayout>
  )
}

function isNotFoundStatus(status?: number): boolean {
  return status === 404 || status === 403
}
