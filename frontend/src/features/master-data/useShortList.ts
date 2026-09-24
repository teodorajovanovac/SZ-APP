import { useQuery } from '@tanstack/react-query'
import { masterDataApi } from './masterDataApi'
import { masterDataKeys } from './useMasterData'

/** ShortList lookup values for a given group (e.g. 'SupplierDocumentType', 'UnitType'). */
export function useShortList(companyId: number, tableName: string) {
  return useQuery({
    queryKey: masterDataKeys.shortLists(companyId, tableName),
    queryFn: () => masterDataApi.shortLists.list(companyId, tableName),
    enabled: companyId > 0 && tableName.length > 0,
    staleTime: 5 * 60_000,
  })
}
