/* eslint-disable react-refresh/only-export-components */
import { useQuery } from '@tanstack/react-query'
import { Chip, List, ListItem, ListItemText, Stack, Typography } from '@mui/material'
import { apiRequest } from '../../api/generated/client'

export interface ImportDefinitionItem { id: number; name: string; code: string; fileMask: string; targetHeaderTable: string; isActive: boolean; sortIndex: number; rowVersion: string }
export function useImportDefinitions(companyId: number) { return useQuery({ queryKey: ['platform', companyId, 'imports'], queryFn: () => apiRequest<ImportDefinitionItem[]>(`/api/v1/companies/${companyId}/import-definitions`) }) }
export function ImportsPage({ companyId }: { companyId: number }) { const imports = useImportDefinitions(companyId); return <Stack spacing={2}><Typography variant="h4">Definicije importa</Typography><List>{imports.data?.map(item => <ListItem key={item.id} secondaryAction={<Chip label={item.isActive ? 'Aktivan' : 'Neaktivan'} color={item.isActive ? 'success' : 'default'}/>}><ListItemText primary={item.name} secondary={`${item.code} · ${item.fileMask} → ${item.targetHeaderTable}`}/></ListItem>)}</List></Stack> }
