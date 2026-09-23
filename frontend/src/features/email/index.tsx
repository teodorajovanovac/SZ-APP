/* eslint-disable react-refresh/only-export-components */
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import MarkEmailReadOutlinedIcon from '@mui/icons-material/MarkEmailReadOutlined'
import SendIcon from '@mui/icons-material/Send'
import {
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Chip,
  Paper,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'

export interface SentEmailItem {
  id: number
  subject: string
  toAddress: string
  status: string
  createdAt: string
  sentAt?: string | null
  sendDescription?: string | null
}

const key = (companyId: number) => ['platform', companyId, 'emails'] as const

export function useSentEmails(companyId: number) {
  return useQuery({
    queryKey: key(companyId),
    queryFn: () => apiRequest<SentEmailItem[]>(`/api/v1/companies/${companyId}/emails`),
  })
}

const statusColor: Record<string, 'default' | 'info' | 'success' | 'error'> = {
  Draft: 'default',
  Queued: 'info',
  Sent: 'success',
  Failed: 'error',
}

export function EmailPage({ companyId }: { companyId: number }) {
  const { t, i18n } = useTranslation()
  const emails = useSentEmails(companyId)
  const client = useQueryClient()
  const send = useMutation({
    mutationFn: (id: number) => apiRequest(`/api/v1/companies/${companyId}/emails/${id}/send`, { method: 'POST' }),
    onSuccess: () => client.invalidateQueries({ queryKey: key(companyId) }),
  })

  const items = emails.data ?? []
  const formatDate = (value: string) => new Date(value).toLocaleString(i18n.language)

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('email_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1 }}>{t('email_.subtitle')}</Typography>
      </Box>

      {send.isError ? <Alert severity="error">{t('email_.queueFailed')}</Alert> : null}
      {emails.isError ? <Alert severity="error">{t('email_.loadFailed')}</Alert> : null}

      {emails.isLoading ? (
        <Stack spacing={2}>
          {[0, 1, 2].map((row) => <Skeleton key={row} variant="rounded" height={112} />)}
        </Stack>
      ) : items.length === 0 ? (
        <Paper variant="outlined" component="section">
          <Stack spacing={1} alignItems="center" sx={{ px: 3, py: 6, textAlign: 'center' }}>
            <MarkEmailReadOutlinedIcon fontSize="large" color="disabled" aria-hidden="true" />
            <Typography component="p" variant="subtitle1">{t('email_.emptyTitle')}</Typography>
            <Typography color="text.secondary" sx={{ maxWidth: 440 }}>{t('email_.emptyBody')}</Typography>
          </Stack>
        </Paper>
      ) : (
        <Stack spacing={2} component="section">
          {items.map((item) => (
            <Card key={item.id} variant="outlined">
              <CardContent>
                <Stack
                  direction={{ xs: 'column', sm: 'row' }}
                  spacing={1}
                  justifyContent="space-between"
                  alignItems={{ sm: 'flex-start' }}
                >
                  <Box>
                    <Typography component="h2" variant="subtitle1">{item.subject}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {t('email_.recipient')}: {item.toAddress}
                    </Typography>
                  </Box>
                  <Chip
                    size="small"
                    color={statusColor[item.status] ?? 'default'}
                    label={t(`email_.status.${item.status}`, { defaultValue: item.status })}
                  />
                </Stack>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>
                  {item.sentAt
                    ? t('email_.sentAt', { date: formatDate(item.sentAt) })
                    : t('email_.createdAt', { date: formatDate(item.createdAt) })}
                </Typography>
                {item.sendDescription ? (
                  <Typography variant="body2" color="text.secondary">{item.sendDescription}</Typography>
                ) : null}
              </CardContent>
              {item.status !== 'Sent' ? (
                <CardActions>
                  <Button
                    size="small"
                    startIcon={<SendIcon />}
                    disabled={send.isPending}
                    onClick={() => send.mutate(item.id)}
                  >
                    {send.isPending ? t('email_.sending') : t('email_.send')}
                  </Button>
                </CardActions>
              ) : null}
            </Card>
          ))}
        </Stack>
      )}
    </Stack>
  )
}
