import { Box, Skeleton, Stack } from '@mui/material'

/** Suspense fallback while a lazily loaded screen's chunk arrives: header row + table block. */
export function PageSkeleton() {
  return (
    <Stack spacing={2} role="status" aria-busy="true" sx={{ p: { xs: 0, sm: 0 } }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Box>
          <Skeleton variant="text" width={220} height={36} />
          <Skeleton variant="text" width={320} />
        </Box>
        <Skeleton variant="rounded" width={140} height={36} />
      </Stack>
      <Skeleton variant="rounded" height={40} />
      <Skeleton variant="rounded" height={320} />
    </Stack>
  )
}
