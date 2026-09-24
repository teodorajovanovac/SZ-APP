import type { PropsWithChildren } from 'react'
import { useEffect, useState } from 'react'
import type { CompanyScope } from './companyScope'
import { CompanyScopeContext } from './companyScopeContext'
import { useActiveCompany } from './useActiveCompany'

const STORAGE_KEY = 'sz.companyScope'

function isValidScope(value: unknown): value is CompanyScope {
  if (!value || typeof value !== 'object') return false
  const mode = (value as { mode?: unknown }).mode
  if (mode === 'single') return typeof (value as { companyId?: unknown }).companyId === 'number'
  if (mode === 'all') return true
  if (mode === 'location') return typeof (value as { locationCategoryId?: unknown }).locationCategoryId === 'number'
  return false
}

function loadStoredScope(): CompanyScope | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const parsed: unknown = JSON.parse(raw)
    return isValidScope(parsed) ? parsed : null
  } catch {
    return null
  }
}

// Additive to ActiveCompanyProvider, not a replacement: existing pages keep reading
// useActiveCompany exactly as before. This only adds a broader "browsing scope" for
// the Ugovori page, and mirrors it back onto the single active company when relevant.
export function CompanyScopeProvider({ children }: PropsWithChildren) {
  const { activeCompany, selectCompany } = useActiveCompany()
  const [scope, setScopeState] = useState<CompanyScope>(
    () => loadStoredScope() ?? { mode: 'single', companyId: activeCompany.id },
  )

  const setScope = (next: CompanyScope) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    setScopeState(next)
    if (next.mode === 'single') {
      selectCompany(next.companyId)
    }
  }

  // The active company can change without going through setScope (e.g. any future
  // direct selectCompany call) — keep single-mode scope mirrored either way.
  useEffect(() => {
    setScopeState((current) =>
      current.mode === 'single' && current.companyId !== activeCompany.id
        ? { mode: 'single', companyId: activeCompany.id }
        : current,
    )
  }, [activeCompany.id])

  return (
    <CompanyScopeContext.Provider value={{ scope, setScope }}>
      {children}
    </CompanyScopeContext.Provider>
  )
}
