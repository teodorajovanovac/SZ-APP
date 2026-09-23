import { useQuery } from '@tanstack/react-query'
import { List, ListItem, ListItemText, Stack, Typography } from '@mui/material'
import { apiRequest } from '../../api/generated/client'

interface SettingItem { id: number; name: string; key: string; value?: string | null; category?: string | null }
interface LanguageItem { code: string; name?: string | null; isActive: boolean; isDefault: boolean }
export function PlatformAdministrationPage({ companyId }: { companyId: number }) {
  const settings = useQuery({ queryKey: ['platform', companyId, 'settings'], queryFn: () => apiRequest<SettingItem[]>(`/api/v1/companies/${companyId}/settings`) })
  const languages = useQuery({ queryKey: ['platform', 'languages'], queryFn: () => apiRequest<LanguageItem[]>(`/api/v1/companies/${companyId}/languages`) })
  return <Stack spacing={3}><Typography component="h1" variant="h1">Administracija platforme</Typography><section><Typography variant="h5">Podešavanja bez tajni</Typography><List>{settings.data?.map(item => <ListItem key={item.id}><ListItemText primary={item.name} secondary={`${item.key}: ${item.value ?? '—'}`}/></ListItem>)}</List></section><section><Typography variant="h5">Jezici</Typography><List>{languages.data?.map(item => <ListItem key={item.code}><ListItemText primary={item.name ?? item.code} secondary={`${item.code}${item.isDefault ? ' · podrazumevani' : ''}`}/></ListItem>)}</List></section></Stack>
}

