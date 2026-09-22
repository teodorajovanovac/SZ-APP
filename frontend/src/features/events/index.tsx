/* eslint-disable react-refresh/only-export-components */
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Alert, Button, Card, CardActions, CardContent, Stack, Typography } from '@mui/material'
import { apiRequest } from '../../api/generated/client'

export interface EventItem { id: number; description: string; status: string; requestedAt: string; requestedByStaffId: number; rowVersion: string; decisionReason?: string | null }
const key = (companyId: number) => ['platform', companyId, 'events'] as const
export function useEvents(companyId: number) { return useQuery({ queryKey: key(companyId), queryFn: () => apiRequest<EventItem[]>(`/api/v1/companies/${companyId}/events`) }) }
export function EventsPage({ companyId }: { companyId: number }) {
  const events = useEvents(companyId); const client = useQueryClient();
  const transition = useMutation({ mutationFn: ({ item, action }: { item: EventItem; action: 'approve' | 'reject' | 'execute' }) => apiRequest(`/api/v1/companies/${companyId}/events/${item.id}/${action}`, { method: 'POST', body: JSON.stringify(action === 'execute' ? { rowVersion: item.rowVersion } : { rowVersion: item.rowVersion, reason: null, emergencyOverride: false }) }), onSuccess: () => client.invalidateQueries({ queryKey: key(companyId) }) })
  return <Stack spacing={2}><Typography variant="h4">Zahtevi za promene</Typography>{transition.isError && <Alert severity="error">Promena statusa nije dozvoljena.</Alert>}{events.data?.map(item => <Card key={item.id} variant="outlined"><CardContent><Typography variant="h6">{item.description}</Typography><Typography color="text.secondary">{item.status}</Typography></CardContent><CardActions>{item.status === 'Requested' && <><Button onClick={() => transition.mutate({ item, action: 'approve' })}>Odobri</Button><Button color="error" onClick={() => transition.mutate({ item, action: 'reject' })}>Odbij</Button></>}{item.status === 'Approved' && <Button onClick={() => transition.mutate({ item, action: 'execute' })}>Izvrši</Button>}</CardActions></Card>)}</Stack>
}
