import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined'
import { Box, Stack, Typography } from '@mui/material'
import type { ReactNode } from 'react'

interface EmptyStateProps {
  message: string
  hint?: string
  action?: ReactNode
  icon?: ReactNode
}

/** Empty list / nothing-selected state: icon + message + optional hint and action. */
export function EmptyState({ message, hint, action, icon }: EmptyStateProps) {
  return (
    <Stack alignItems="center" spacing={1} sx={{ py: 6, px: 2, textAlign: 'center', color: 'text.secondary' }}>
      <Box
        sx={{
          width: 48,
          height: 48,
          borderRadius: '50%',
          display: 'grid',
          placeItems: 'center',
          bgcolor: 'action.hover',
          color: 'text.secondary',
          mb: 0.5,
        }}
        aria-hidden="true"
      >
        {icon ?? <InboxOutlinedIcon />}
      </Box>
      <Typography variant="subtitle1" color="text.primary">{message}</Typography>
      {hint ? <Typography variant="body2" sx={{ maxWidth: 420 }}>{hint}</Typography> : null}
      {action ? <Box sx={{ pt: 1 }}>{action}</Box> : null}
    </Stack>
  )
}
