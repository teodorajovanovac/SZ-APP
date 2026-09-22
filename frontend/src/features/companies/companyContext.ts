import { createContext } from 'react'
import type { CompanySummary } from '../../api/generated/client'

export interface CompanyContextValue {
  companies: CompanySummary[]
  activeCompany: CompanySummary
  selectCompany: (companyId: number) => void
}

export const CompanyContext = createContext<CompanyContextValue | null>(null)
