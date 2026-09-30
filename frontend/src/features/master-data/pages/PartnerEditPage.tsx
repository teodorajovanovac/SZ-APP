import { Alert, CircularProgress, Link, Paper, Stack, Typography } from '@mui/material'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { useActiveCompany } from '../../companies/useActiveCompany'
import { PartnerForm } from '../components/PartnerForm'
import { usePartnerDetail } from '../useMasterData'

/** Full-page create/edit of a partner (replaces the former dialog). */
export function PartnerEditPage() {
  const { companyId, partnerId } = useParams()
  const navigate = useNavigate()
  const { activeCompany } = useActiveCompany()
  const isNew = partnerId === undefined
  const companyIdNum = isNew ? activeCompany.id : Number(companyId)
  const partnerIdNum = Number(partnerId)
  const valid = Number.isFinite(companyIdNum) && companyIdNum > 0 && (isNew || (Number.isFinite(partnerIdNum) && partnerIdNum > 0))
  const detail = usePartnerDetail(valid && !isNew ? companyIdNum : 0, valid && !isNew ? partnerIdNum : 0)

  return (
    <Stack spacing={2}>
      <Link component={RouterLink} to="/partners" underline="hover">← Partneri</Link>
      <Typography component="h1" variant="h1">{isNew ? 'Novi partner' : 'Podaci partner'}</Typography>
      {!valid && <Alert severity="warning">Partner nije pronađen.</Alert>}
      {detail.isLoading && <CircularProgress size={24} />}
      {detail.isError && <Alert severity="error">Partner nije mogao da se učita.</Alert>}
      {valid && (isNew || detail.data) && (
        <Paper variant="outlined" sx={{ p: { xs: 2, md: 3 } }}>
          <PartnerForm
            // Remount when the loaded partner changes so defaultValues are re-read.
            key={detail.data ? `${detail.data.id}-${detail.dataUpdatedAt}` : 'new'}
            companyId={companyIdNum}
            partner={detail.data}
            onSaved={(saved) =>
              // After creating, stay on the edit page so addresses can be added.
              navigate(isNew ? `/partners/${saved.companyId}/${saved.id}/edit` : '/partners', { replace: isNew })}
            onCancel={() => navigate('/partners')}
          />
        </Paper>
      )}
    </Stack>
  )
}
