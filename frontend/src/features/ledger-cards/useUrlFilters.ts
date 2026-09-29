import { useSearchParams } from 'react-router-dom'
import { useActiveCompany } from '../companies/useActiveCompany'

/** 9.2: accounts in real use; shown even if the chart-of-accounts endpoint returns nothing. */
export const CORE_ACCOUNTS = ['2040', '2410', '4350', '4900', '5590']

/**
 * UX-21: every filter lives in the URL so "Back" after a drill-down restores the view.
 * Changing any filter other than page/pageSize resets to the first page.
 */
export function useUrlFilters() {
  const [params, setParams] = useSearchParams()
  const { activeCompany } = useActiveCompany()
  const companyId = Number(params.get('companyId')) || activeCompany.id
  const get = (key: string) => params.get(key) ?? ''
  const set = (patch: Record<string, string | number | null | undefined>) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current)
        for (const [key, value] of Object.entries(patch)) {
          if (value === null || value === undefined || value === '') next.delete(key)
          else next.set(key, String(value))
        }
        if (!('page' in patch)) next.delete('page')
        return next
      },
      { replace: false },
    )
  return { companyId, get, set }
}
