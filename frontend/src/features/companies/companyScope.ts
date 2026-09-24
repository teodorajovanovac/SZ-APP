export type CompanyScope =
  | { mode: 'single'; companyId: number }
  | { mode: 'all' }
  | { mode: 'location'; locationCategoryId: number }

// Turns a scope into the query-string params the Ugovori backend endpoint expects.
export function companyScopeToQueryParams(scope: CompanyScope): Record<string, string> {
  switch (scope.mode) {
    case 'single': return { mode: 'single', companyId: String(scope.companyId) }
    case 'all': return { mode: 'all' }
    case 'location': return { mode: 'location', locationCategoryId: String(scope.locationCategoryId) }
  }
}
