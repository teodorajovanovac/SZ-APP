import { Dialog, DialogContent, DialogTitle, IconButton } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import { useEffect, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { ConfirmDialog } from './ConfirmDialog'

interface FormDialogProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  maxWidth?: 'xs' | 'sm' | 'md' | 'lg'
}

/**
 * MUI Dialog with a labelled title and a close button; focus trap / Esc come from Dialog.
 *
 * Esc / backdrop click / the X button normally discard whatever the form inside holds. To
 * avoid silently losing input, this dialog watches for any `change` bubbling up from its
 * content (typing in a field, picking a select, ...) and, once that happens, asks for
 * confirmation before actually closing. No form needs to report its own dirty state —
 * DialogContent's native change events bubble up through the React tree regardless of
 * MUI's portal, so this works for any form dropped in as children.
 */
export function FormDialog({ open, title, onClose, children, maxWidth = 'sm' }: FormDialogProps) {
  const { t } = useTranslation()
  const [dirty, setDirty] = useState(false)
  const [confirmingClose, setConfirmingClose] = useState(false)

  useEffect(() => {
    if (open) setDirty(false)
  }, [open])

  const requestClose = () => {
    if (dirty) setConfirmingClose(true)
    else onClose()
  }

  return (
    <>
      <Dialog open={open} onClose={requestClose} onChange={() => setDirty(true)} fullWidth maxWidth={maxWidth}>
        <DialogTitle sx={{ pr: 6 }}>
          {title}
          <IconButton
            aria-label={t('ui.close')}
            onClick={requestClose}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent dividers>{children}</DialogContent>
      </Dialog>
      <ConfirmDialog
        open={confirmingClose}
        title={t('formDialog.discardTitle')}
        description={t('formDialog.discardBody')}
        confirmLabel={t('formDialog.discardConfirm')}
        destructive
        onClose={() => setConfirmingClose(false)}
        onConfirm={() => {
          setConfirmingClose(false)
          setDirty(false)
          onClose()
        }}
      />
    </>
  )
}
