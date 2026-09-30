import { Button, Stack, Tooltip } from '@mui/material'
import { useTranslation } from 'react-i18next'

interface FormActionsProps {
  onCancel?: () => void
  pending?: boolean
  disabled?: boolean
  /** Shows "Sačuvaj i novi" (Ctrl+Enter); the form checks `isSaveNewSubmit(event)` after saving. */
  saveNew?: boolean
  saveLabel?: string
}

/** True when the form was submitted with the "Sačuvaj i novi" button (or Ctrl+Enter). */
// eslint-disable-next-line react-refresh/only-export-components
export function isSaveNewSubmit(event?: { nativeEvent?: Event } | Event): boolean {
  const native = event && 'nativeEvent' in event ? event.nativeEvent : event
  const submitter = (native as SubmitEvent | undefined)?.submitter
  return Boolean(submitter?.hasAttribute('data-save-new'))
}

/** Cancel / [Save & new] / Save row for dialog forms, with the keyboard hints (UX-50). */
export function FormActions({ onCancel, pending, disabled, saveNew, saveLabel }: FormActionsProps) {
  const { t } = useTranslation()
  const label = saveLabel ?? t('common.save')
  return (
    <Stack direction="row" spacing={1} justifyContent="flex-end">
      {onCancel && <Button onClick={onCancel}>{t('common.cancel')}</Button>}
      {saveNew && (
        <Tooltip title="Ctrl+Enter">
          <span>
            <Button type="submit" data-save-new disabled={pending || disabled} aria-keyshortcuts="Control+Enter">
              {t('ui.saveAndNew')}
            </Button>
          </span>
        </Tooltip>
      )}
      <Tooltip title={t('ui.shortcuts.withKeys', { label, keys: 'Ctrl+S' })}>
        <span>
          <Button type="submit" variant="contained" disabled={pending || disabled} aria-keyshortcuts="Control+S">
            {label}
          </Button>
        </span>
      </Tooltip>
    </Stack>
  )
}
