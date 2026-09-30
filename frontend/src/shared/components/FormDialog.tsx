import { Dialog, DialogContent, DialogTitle, IconButton } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import { useEffect, useRef, useState, type KeyboardEvent, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { comboFromEvent } from '../keyboard/shortcuts'
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
 *
 * UX-50: the first field gets focus on open; Ctrl+S submits the form inside, Ctrl+Enter
 * clicks its `data-save-new` button ("Sačuvaj i novi") when the form offers one.
 */
export function FormDialog({ open, title, onClose, children, maxWidth = 'sm' }: FormDialogProps) {
  const { t } = useTranslation()
  const [dirty, setDirty] = useState(false)
  const [confirmingClose, setConfirmingClose] = useState(false)
  const contentRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (open) setDirty(false)
  }, [open])

  const requestClose = () => {
    if (dirty) setConfirmingClose(true)
    else onClose()
  }

  const focusFirstField = () => {
    contentRef.current
      ?.querySelector<HTMLElement>('input:not([type="hidden"]):not([disabled]):not([readonly]), textarea:not([disabled]), [role="combobox"]')
      ?.focus()
  }

  const onKeyDown = (event: KeyboardEvent) => {
    const combo = comboFromEvent(event.nativeEvent)
    if (combo !== 'ctrl+s' && combo !== 'ctrl+enter') return
    const form = contentRef.current?.querySelector('form')
    if (!form) return
    event.preventDefault()
    const saveNew = combo === 'ctrl+enter' ? form.querySelector<HTMLButtonElement>('[data-save-new]:not([disabled])') : null
    if (saveNew) saveNew.click()
    else form.requestSubmit()
  }

  return (
    <>
      <Dialog
        open={open}
        onClose={requestClose}
        onChange={() => setDirty(true)}
        onKeyDown={onKeyDown}
        fullWidth
        maxWidth={maxWidth}
        slotProps={{ transition: { onEntered: focusFirstField } }}
      >
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
        <DialogContent dividers ref={contentRef}>{children}</DialogContent>
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
