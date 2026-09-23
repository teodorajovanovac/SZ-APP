/* eslint-disable react-refresh/only-export-components */
import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, List, ListItem, ListItemText, Stack, Typography } from '@mui/material'
import { apiRequest } from '../../api/generated/client'

export interface DocumentItem { id: number; fileName: string; contentType: string; size: number; sha256: string; createdAt: string; description?: string | null }
const key = (companyId: number) => ['platform', companyId, 'documents'] as const

export function useDocuments(companyId: number) {
  return useQuery({ queryKey: key(companyId), queryFn: () => apiRequest<DocumentItem[]>(`/api/v1/companies/${companyId}/documents`) })
}

export function DocumentsPage({ companyId }: { companyId: number }) {
  const documents = useDocuments(companyId); const queryClient = useQueryClient(); const [file, setFile] = useState<File>()
  const upload = useMutation({
    mutationFn: async () => { const form = new FormData(); if (!file) throw new Error('Izaberite fajl.'); form.append('file', file); return apiRequest<DocumentItem>(`/api/v1/companies/${companyId}/documents`, { method: 'POST', body: form }) },
    onSuccess: () => { setFile(undefined); return queryClient.invalidateQueries({ queryKey: key(companyId) }) },
  })
  return <Stack spacing={2}><Typography component="h1" variant="h1">Dokumenti</Typography>{upload.isError && <Alert severity="error">Dokument nije sačuvan.</Alert>}<input aria-label="Izaberite dokument" type="file" accept=".pdf,.png,.jpg,.jpeg,.csv,.xml,.xlsx" onChange={(e) => setFile(e.target.files?.[0])}/><Button variant="contained" disabled={!file || upload.isPending} onClick={() => upload.mutate()}>Otpremi</Button><List>{documents.data?.map(item => <ListItem key={item.id}><ListItemText primary={item.fileName} secondary={`${item.contentType} · ${Math.ceil(item.size / 1024)} KB`}/></ListItem>)}</List></Stack>
}
