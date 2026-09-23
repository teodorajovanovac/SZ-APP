/* eslint-disable react-refresh/only-export-components */
import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import DescriptionOutlinedIcon from '@mui/icons-material/DescriptionOutlined'
import FolderOffOutlinedIcon from '@mui/icons-material/FolderOffOutlined'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import {
  Alert,
  Box,
  Button,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material'
import { useTranslation } from 'react-i18next'
import { apiRequest } from '../../api/generated/client'

export interface DocumentItem {
  id: number
  fileName: string
  contentType: string
  size: number
  sha256: string
  createdAt: string
  description?: string | null
}

const key = (companyId: number) => ['platform', companyId, 'documents'] as const

export function useDocuments(companyId: number) {
  return useQuery({
    queryKey: key(companyId),
    queryFn: () => apiRequest<DocumentItem[]>(`/api/v1/companies/${companyId}/documents`),
  })
}

export function DocumentsPage({ companyId }: { companyId: number }) {
  const { t, i18n } = useTranslation()
  const documents = useDocuments(companyId)
  const queryClient = useQueryClient()
  const [file, setFile] = useState<File>()

  const upload = useMutation({
    mutationFn: async () => {
      if (!file) throw new Error(t('documents_.noFile'))
      const form = new FormData()
      form.append('file', file)
      return apiRequest<DocumentItem>(`/api/v1/companies/${companyId}/documents`, { method: 'POST', body: form })
    },
    onSuccess: () => {
      setFile(undefined)
      return queryClient.invalidateQueries({ queryKey: key(companyId) })
    },
  })

  const items = documents.data ?? []

  return (
    <Stack spacing={3}>
      <Box component="header">
        <Typography component="h1" variant="h1">{t('documents_.title')}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1 }}>{t('documents_.subtitle')}</Typography>
      </Box>

      {upload.isError ? <Alert severity="error">{t('documents_.uploadFailed')}</Alert> : null}
      {documents.isError ? <Alert severity="error">{t('documents_.loadFailed')}</Alert> : null}

      <Paper variant="outlined" component="section" sx={{ p: { xs: 2, md: 3 } }}>
        <Stack spacing={2}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems={{ sm: 'center' }}>
            <Button component="label" variant="outlined" startIcon={<UploadFileIcon />}>
              {file?.name ?? t('documents_.chooseFile')}
              <input
                hidden
                type="file"
                aria-label={t('documents_.chooseFileLabel')}
                accept=".pdf,.png,.jpg,.jpeg,.csv,.xml,.xlsx"
                onChange={(event) => setFile(event.target.files?.[0])}
              />
            </Button>
            <Button variant="contained" disabled={!file || upload.isPending} onClick={() => upload.mutate()}>
              {upload.isPending ? t('documents_.uploading') : t('documents_.upload')}
            </Button>
          </Stack>
          <Typography variant="body2" color="text.secondary">{t('documents_.allowedTypes')}</Typography>
        </Stack>
      </Paper>

      <Paper variant="outlined" component="section">
        <Typography component="h2" variant="h6" sx={{ p: 2 }}>{t('documents_.listTitle')}</Typography>
        <Divider />
        {documents.isLoading ? (
          <Box sx={{ p: 2 }}>
            {[0, 1, 2].map((row) => <Skeleton key={row} height={48} />)}
          </Box>
        ) : items.length === 0 ? (
          <Stack spacing={1} alignItems="center" sx={{ px: 3, py: 6, textAlign: 'center' }}>
            <FolderOffOutlinedIcon fontSize="large" color="disabled" aria-hidden="true" />
            <Typography component="p" variant="subtitle1">{t('documents_.emptyTitle')}</Typography>
            <Typography color="text.secondary" sx={{ maxWidth: 440 }}>{t('documents_.emptyBody')}</Typography>
          </Stack>
        ) : (
          <List disablePadding>
            {items.map((item) => (
              <ListItem key={item.id} divider>
                <ListItemIcon><DescriptionOutlinedIcon aria-hidden="true" /></ListItemIcon>
                <ListItemText
                  primary={item.fileName}
                  secondary={t('documents_.meta', {
                    type: item.contentType,
                    size: Math.ceil(item.size / 1024),
                    date: new Date(item.createdAt).toLocaleDateString(i18n.language),
                  })}
                />
              </ListItem>
            ))}
          </List>
        )}
      </Paper>
    </Stack>
  )
}
