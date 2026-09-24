import { useContext } from 'react'
import { CompanyScopeContext } from './companyScopeContext'

export function useCompanyScope() {
  const context = useContext(CompanyScopeContext)
  if (!context) {
    throw new Error('useCompanyScope mora biti pozvan unutar CompanyScopeProvider-a.')
  }
  return context
}
