import { useActiveCompany } from '../companies/useActiveCompany'

/**
 * Stopgap for the CompanyScope contract that `features/companies/companyScope.ts`
 * (built by a parallel agent) will export once merged - same shape:
 *   type CompanyScope = { mode: 'single'; companyId: number } | { mode: 'all' } | { mode: 'location'; locationCategoryId: number }
 *   companyScopeToQueryParams(scope) -> { mode, companyId? } | { mode } | { mode, locationCategoryId? }
 *   useCompanyScope() -> { scope, setScope }
 * That file did not exist yet when this page was built, and this feature is not
 * allowed to touch features/companies/**, so this local adapter implements the
 * identical contract backed by the app's current single-company selection
 * (ActiveCompanyProvider/useActiveCompany - the only scope the app supports today).
 * Once companyScope.ts merges, swap the import in ContractsPage.tsx for the real
 * one and delete this file - the shape matches exactly, so it's a one-line change.
 */
export type CompanyScope =
  | { mode: 'single'; companyId: number }
  | { mode: 'all' }
  | { mode: 'location'; locationCategoryId: number }

export function companyScopeToQueryParams(scope: CompanyScope): Record<string, string> {
  switch (scope.mode) {
    case 'single':
      return { mode: 'single', companyId: String(scope.companyId) }
    case 'all':
      return { mode: 'all' }
    case 'location':
      return { mode: 'location', locationCategoryId: String(scope.locationCategoryId) }
  }
}

export function useCompanyScope(): { scope: CompanyScope; setScope: (scope: CompanyScope) => void } {
  const { activeCompany, selectCompany } = useActiveCompany()
  return {
    scope: { mode: 'single', companyId: activeCompany.id },
    setScope: (scope) => {
      if (scope.mode === 'single') {
        selectCompany(scope.companyId)
      }
    },
  }
}
