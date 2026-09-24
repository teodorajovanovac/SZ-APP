import { createContext } from 'react'
import type { CompanyScope } from './companyScope'

export interface CompanyScopeContextValue {
  scope: CompanyScope
  setScope: (scope: CompanyScope) => void
}

export const CompanyScopeContext = createContext<CompanyScopeContextValue | null>(null)
