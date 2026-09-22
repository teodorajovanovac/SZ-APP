import type { PropsWithChildren } from 'react'
import { useState } from 'react'
import type { CompanySummary } from '../../api/generated/client'
import { CompanyContext } from './companyContext'

interface ActiveCompanyProviderProps extends PropsWithChildren {
  companies: CompanySummary[]
}

function initialCompanyId(companies: CompanySummary[]) {
  const stored = Number(localStorage.getItem('sz.activeCompanyId'))
  return companies.some((company) => company.id === stored) ? stored : companies[0]?.id
}

export function ActiveCompanyProvider({ companies, children }: ActiveCompanyProviderProps) {
  const [activeCompanyId, setActiveCompanyId] = useState(() => initialCompanyId(companies))
  const activeCompany = companies.find((company) => company.id === activeCompanyId) ?? companies[0]

  if (!activeCompany) {
    throw new Error('Prijavljeni korisnik nema pristup nijednoj kompaniji.')
  }

  const selectCompany = (companyId: number) => {
    if (!companies.some((company) => company.id === companyId)) {
      return
    }
    localStorage.setItem('sz.activeCompanyId', String(companyId))
    setActiveCompanyId(companyId)
  }

  return (
    <CompanyContext.Provider value={{ companies, activeCompany, selectCompany }}>
      {children}
    </CompanyContext.Provider>
  )
}
