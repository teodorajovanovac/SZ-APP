import { Dialog, DialogContent, DialogTitle, IconButton } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

interface FormDialogProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg'
}

/** MUI Dialog with a labelled title and a close button; focus trap / Esc come from Dialog. */
export function FormDialog({ open, title, onClose, children, maxWidth = 'sm' }: FormDialogProps) {
  const { t } = useTranslation()
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth={maxWidth}>
      <DialogTitle sx={{ pr: 6 }}>
        {title}
        <IconButton
          aria-label={t('ui.close')}
          onClick={onClose}
          sx={{ position: 'absolute', right: 8, top: 8 }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>{children}</DialogContent>
    </Dialog>
  )
}
