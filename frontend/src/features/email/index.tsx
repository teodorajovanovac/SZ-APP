/* eslint-disable react-refresh/only-export-components */
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Card, CardActions, CardContent, Stack, Typography } from '@mui/material'
import { apiRequest } from '../../api/generated/client'

export interface SentEmailItem { id: number; subject: string; toAddress: string; status: string; createdAt: string; sentAt?: string | null; sendDescription?: string | null }
const key = (companyId: number) => ['platform', companyId, 'emails'] as const
export function useSentEmails(companyId: number) { return useQuery({ queryKey: key(companyId), queryFn: () => apiRequest<SentEmailItem[]>(`/api/v1/companies/${companyId}/emails`) }) }
export function EmailPage({ companyId }: { companyId: number }) {
  const emails = useSentEmails(companyId); const client = useQueryClient();
  const send = useMutation({ mutationFn: (id: number) => apiRequest(`/api/v1/companies/${companyId}/emails/${id}/send`, { method: 'POST' }), onSuccess: () => client.invalidateQueries({ queryKey: key(companyId) }) })
  return <Stack spacing={2}><Typography component="h1" variant="h1">Email</Typography>{send.isError && <Alert severity="error">Email nije stavljen u red za slanje.</Alert>}{emails.data?.map(item => <Card key={item.id}><CardContent><Typography variant="h6">{item.subject}</Typography><Typography>{item.toAddress}</Typography><Typography color="text.secondary">{item.status}{item.sendDescription ? ` — ${item.sendDescription}` : ''}</Typography></CardContent><CardActions>{item.status !== 'Sent' && <Button onClick={() => send.mutate(item.id)}>Pošalji</Button>}</CardActions></Card>)}</Stack>
}
