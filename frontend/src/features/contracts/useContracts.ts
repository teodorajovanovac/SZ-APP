import { useQuery } from '@tanstack/react-query'
import { contractsApi } from './contractsApi'
import type { ContractsOverviewQuery } from './types'

export function useContracts(query: ContractsOverviewQuery) {
  return useQuery({
    queryKey: ['contracts-overview', query],
    queryFn: () => contractsApi.list(query),
  })
}
