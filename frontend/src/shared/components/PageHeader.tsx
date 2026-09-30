import AddIcon from '@mui/icons-material/Add'
import { Box, Button, Stack, Tooltip, Typography } from '@mui/material'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useDocumentTitle } from '../hooks/useDocumentTitle'
import { useShortcut } from '../keyboard/shortcuts'

interface PageHeaderProps {
  title: string
  subtitle?: ReactNode
  /** Extra actions, rendered left of the primary "new" button. */
  actions?: ReactNode
  /** Primary "new" action; also bound to Alt+N. */
  onNew?: () => void
  newLabel?: string
  /** Filter controls, laid out in one wrapping row under the title. */
  filters?: ReactNode
}

/** DES-02: one page bar everywhere — title + subtitle left, primary action right, filters below. */
export function PageHeader({ title, subtitle, actions, onNew, newLabel, filters }: PageHeaderProps) {
  const { t } = useTranslation()
  useDocumentTitle(title)
  useShortcut('alt+n', onNew)
  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }} justifyContent="space-between">
        <Box sx={{ minWidth: 0 }}>
          <Typography component="h1" variant="h1">{title}</Typography>
          {subtitle ? <Typography color="text.secondary" sx={{ mt: 0.5 }}>{subtitle}</Typography> : null}
        </Box>
        {actions || onNew ? (
          <Stack direction="row" spacing={1} alignItems="center" sx={{ flexShrink: 0 }}>
            {actions}
            {onNew ? (
              <Tooltip title={t('ui.shortcuts.withKeys', { label: newLabel ?? t('ui.new'), keys: 'Alt+N' })}>
                <Button variant="contained" startIcon={<AddIcon />} onClick={onNew} aria-keyshortcuts="Alt+N">
                  {newLabel ?? t('ui.new')}
                </Button>
              </Tooltip>
            ) : null}
          </Stack>
        ) : null}
      </Stack>
      {filters ? (
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap" alignItems="center">
          {filters}
        </Stack>
      ) : null}
    </Stack>
  )
}
