import { useContext } from 'react'
import { CompanyContext } from './companyContext'

export function useActiveCompany() {
  const context = useContext(CompanyContext)
  if (!context) {
    throw new Error('useActiveCompany mora biti pozvan unutar ActiveCompanyProvider-a.')
  }
  return context
}
