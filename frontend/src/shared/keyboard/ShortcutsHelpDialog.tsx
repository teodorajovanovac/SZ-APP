import { Box, Dialog, DialogContent, DialogTitle, Stack, Typography } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { NAV_SHORTCUTS } from './shortcuts'

function Keys({ keys }: { keys: string[] }) {
  return (
    <Stack direction="row" spacing={0.5} sx={{ flexShrink: 0 }}>
      {keys.map((key, index) => (
        <Box
          key={index}
          component="kbd"
          sx={{
            px: 0.75,
            minWidth: 24,
            textAlign: 'center',
            fontFamily: 'inherit',
            fontSize: '0.75rem',
            fontWeight: 600,
            lineHeight: '22px',
            border: 1,
            borderColor: 'divider',
            borderBottomWidth: 2,
            borderRadius: 1,
            bgcolor: 'background.default',
          }}
        >
          {key}
        </Box>
      ))}
    </Stack>
  )
}

function Row({ keys, label }: { keys: string[]; label: string }) {
  return (
    <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2} sx={{ py: 0.5 }}>
      <Typography variant="body2">{label}</Typography>
      <Keys keys={keys} />
    </Stack>
  )
}

export function ShortcutsHelpDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { t } = useTranslation()
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('ui.shortcuts.title')}</DialogTitle>
      <DialogContent dividers>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, columnGap: 4, rowGap: 2 }}>
          <Box>
            <Typography variant="overline" color="text.secondary">{t('ui.shortcuts.general')}</Typography>
            <Row keys={['?']} label={t('ui.shortcuts.help')} />
            <Row keys={['Ctrl', 'K']} label={t('ui.shortcuts.search')} />
            <Row keys={['/']} label={t('ui.shortcuts.pageSearch')} />
            <Row keys={['Alt', 'N']} label={t('ui.shortcuts.new')} />
            <Typography variant="overline" color="text.secondary" component="p" sx={{ mt: 2 }}>{t('ui.shortcuts.forms')}</Typography>
            <Row keys={['Ctrl', 'S']} label={t('ui.shortcuts.save')} />
            <Row keys={['Ctrl', 'Enter']} label={t('ui.shortcuts.saveNew')} />
            <Row keys={['Esc']} label={t('ui.shortcuts.close')} />
          </Box>
          <Box>
            <Typography variant="overline" color="text.secondary">{t('ui.shortcuts.goTo')}</Typography>
            {Object.keys(NAV_SHORTCUTS).map((letter) => (
              <Row key={letter} keys={['G', letter.toUpperCase()]} label={t(`ui.shortcuts.nav.${letter}`)} />
            ))}
          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  )
}
