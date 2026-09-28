import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditIcon from '@mui/icons-material/Edit'
import { Alert, Button, IconButton, MenuItem, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/problemDetails'
import { ConfirmDialog } from '../../shared/components/ConfirmDialog'
import { FormDialog } from '../../shared/components/FormDialog'
import { useAuth } from '../auth/useAuth'
import { StaffAccessForm } from '../master-data/components/StaffAccessForm'
import { masterDataApi } from '../master-data/masterDataApi'
import type { StaffDetail, StaffGrant } from './staffApi'

/** A user's StaffAccess grants across companies; add/edit/remove reuse the per-company StaffAccess endpoints. */
export function StaffGrantsSection({ staff }: { staff: StaffDetail }) {
  const { t } = useTranslation()
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const [editing, setEditing] = useState<StaffGrant | 'new'>()
  const [newCompanyId, setNewCompanyId] = useState<number | ''>('')
  const [removing, setRemoving] = useState<StaffGrant>()
  const refresh = () => queryClient.invalidateQueries({ queryKey: ['staff'] })
  const remove = useMutation({
    mutationFn: (grant: StaffGrant) => masterDataApi.staffAccess.remove(grant.companyId, grant.accessId),
    onSuccess: () => {
      setRemoving(undefined)
      void refresh()
      void queryClient.invalidateQueries({ queryKey: ['master-data'] })
    },
  })
  const freeCompanies = user?.companies.filter((c) => !staff.grants.some((g) => g.companyId === c.id)) ?? []
  const close = () => {
    setEditing(undefined)
    setNewCompanyId('')
  }
  const formCompanyId = editing === 'new' ? newCompanyId : editing?.companyId

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={2}>
        <Stack direction="row" justifyContent="space-between" alignItems="center">
          <Typography component="h2" variant="h6">{t('staffAdmin_.grants')}</Typography>
          <Button startIcon={<AddIcon />} onClick={() => setEditing('new')} disabled={freeCompanies.length === 0}>{t('staffAdmin_.addGrant')}</Button>
        </Stack>
        {remove.isError && <Alert severity="error">{getErrorMessage(remove.error, t('staffAdmin_.saveError'))}</Alert>}
        {staff.grants.length === 0 ? (
          <Typography color="text.secondary">{t('staffAdmin_.noGrants')}</Typography>
        ) : (
          <Table size="small" aria-label={t('staffAdmin_.grants')}>
            <TableHead>
              <TableRow>
                <TableCell>{t('staffAdmin_.company')}</TableCell>
                <TableCell>{t('staffAdmin_.role')}</TableCell>
                <TableCell align="right">{t('ui.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {staff.grants.map((grant) => (
                <TableRow key={grant.accessId}>
                  <TableCell>{grant.companyName}</TableCell>
                  <TableCell>{grant.staffRole}</TableCell>
                  <TableCell align="right">
                    <Tooltip title={t('ui.edit')}>
                      <IconButton size="small" aria-label={t('ui.edit')} onClick={() => setEditing(grant)}><EditIcon fontSize="small" /></IconButton>
                    </Tooltip>
                    <Tooltip title={t('staffAdmin_.removeGrant')}>
                      <IconButton size="small" aria-label={t('staffAdmin_.removeGrant')} onClick={() => setRemoving(grant)}><DeleteIcon fontSize="small" /></IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Stack>
      <FormDialog open={Boolean(editing)} title={editing === 'new' ? t('staffAdmin_.addGrant') : t('staffAccess_.editTitle')} onClose={close} maxWidth="xs">
        <Stack spacing={2}>
          {editing === 'new' && (
            <TextField size="small" select required label={t('staffAdmin_.company')} value={newCompanyId} onChange={(e) => setNewCompanyId(Number(e.target.value))}>
              {freeCompanies.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
            </TextField>
          )}
          {editing && typeof formCompanyId === 'number' && (
            <StaffAccessForm
              key={formCompanyId}
              companyId={formCompanyId}
              fixedStaffId={staff.id}
              access={editing === 'new' ? undefined : { id: editing.accessId, staffId: staff.id, staffEmail: staff.email, companyId: editing.companyId, staffRole: editing.staffRole }}
              onSaved={() => {
                close()
                void refresh()
              }}
              onCancel={close}
            />
          )}
        </Stack>
      </FormDialog>
      <ConfirmDialog
        open={Boolean(removing)}
        title={t('staffAdmin_.removeGrant')}
        description={t('staffAdmin_.removeGrantConfirm', { company: removing?.companyName })}
        confirmLabel={t('staffAdmin_.removeGrant')}
        destructive
        pending={remove.isPending}
        onClose={() => setRemoving(undefined)}
        onConfirm={() => removing && remove.mutate(removing)}
      />
    </Paper>
  )
}
