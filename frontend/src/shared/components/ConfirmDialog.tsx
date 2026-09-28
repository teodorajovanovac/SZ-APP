import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

interface ConfirmDialogProps {
  open: boolean
  title: string
  /** Short summary of what's about to happen (amount, item count, recipients, ...). */
  description: ReactNode
  confirmLabel: string
  onConfirm: () => void
  onClose: () => void
  pending?: boolean
  /** Use the warning color for reversible-but-risky actions (e.g. discarding input). */
  destructive?: boolean
}

/** Shared confirmation dialog for consequential actions (posting, sending, importing, discarding). */
export function ConfirmDialog({ open, title, description, confirmLabel, onConfirm, onClose, pending, destructive }: ConfirmDialogProps) {
  const { t } = useTranslation()
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <DialogContentText component="div">{description}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('common.cancel')}</Button>
        <Button color={destructive ? 'warning' : 'primary'} variant="contained" disabled={pending} onClick={onConfirm}>
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
